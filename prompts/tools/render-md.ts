#!/usr/bin/env node
/**
 * render-md.ts
 *
 * Read every prompts/*.yaml + voice.yaml, expand {{>snippet}} partials,
 * and emit one .prompt.md per prompt under build/. Engineers who use
 * shells (Cursor, copilot.vim, GH CLI agents) consume the markdown
 * shims; YAML stays the source of truth in the repo.
 *
 * Usage:  node tools/render-md.ts            # writes to ./build
 *         node tools/render-md.ts --check    # exits non-zero if drift
 *
 * Drift is detected by comparing the freshly rendered files against
 * whatever is checked in under build/ — used in CI to enforce that
 * committed shims match the YAML source.
 */

import * as fs from "node:fs";
import * as path from "node:path";
import * as crypto from "node:crypto";
import yaml from "js-yaml";
import Mustache from "mustache";

const ROOT = path.resolve(__dirname, "..");
const OUT  = path.join(ROOT, "build");
const CHECK = process.argv.includes("--check");

type Voice = Record<string, unknown>;

interface PromptDef {
  key: string;
  description: string;
  inputs: Array<{ name: string; type: string; required: boolean; description?: string }>;
  snippets?: string[];
  tools?: Array<{ name: string; scope?: string; side_effects?: string; requires_human_approval?: boolean }>;
  system: string;
  user: string;
  eval_set: string;
}

interface Family {
  extends: string;
  namespace: string;
  owner: string;
  prompts: Record<string, PromptDef>;
}

function readYaml<T>(p: string): T {
  return yaml.load(fs.readFileSync(p, "utf8")) as T;
}

function loadSnippets(): Record<string, string> {
  const dir = path.join(ROOT, "snippets");
  const out: Record<string, string> = {};
  for (const f of fs.readdirSync(dir)) {
    if (!f.endsWith(".md")) continue;
    const name = f.slice(0, -3);
    out[name] = fs.readFileSync(path.join(dir, f), "utf8").trim();
  }
  return out;
}

function expand(template: string, snippets: Record<string, string>): string {
  // Mustache partial syntax is `{{>name}}`. We pass the snippets bag
  // as `partials`; values must be strings.
  return Mustache.render(template, {}, snippets);
}

function shimFor(family: Family, prompt: PromptDef, voice: Voice, snippets: Record<string, string>): string {
  const system = expand(prompt.system, snippets);
  const user   = expand(prompt.user, snippets);
  const inputsTable = prompt.inputs.length
    ? prompt.inputs.map(i => `- \`${i.name}\` (${i.type}${i.required ? ", required" : ""}): ${i.description ?? ""}`).join("\n")
    : "_(no inputs)_";
  const toolsBlock = (prompt.tools ?? []).length
    ? prompt.tools!.map(t => `- \`${t.name}\`${t.scope ? ` — scope: ${t.scope}` : ""}${t.requires_human_approval ? " — **requires human approval**" : ""}`).join("\n")
    : "_(no tools)_";
  return [
    `# ${prompt.key}`,
    ``,
    `> ${prompt.description}`,
    ``,
    `**Owner:** ${family.owner}  `,
    `**Voice:** inherits \`${family.extends}\` (house voice + MNPI stance).`,
    ``,
    `## Inputs`,
    inputsTable,
    ``,
    `## Tools`,
    toolsBlock,
    ``,
    `## System`,
    "",
    "```text",
    system,
    "```",
    ``,
    `## User`,
    "",
    "```text",
    user,
    "```",
    ``,
    `## Eval set`,
    `\`${prompt.eval_set}\``,
    ``,
  ].join("\n");
}

function sha(s: string): string {
  return crypto.createHash("sha256").update(s).digest("hex").slice(0, 16);
}

function main() {
  const voice = readYaml<Voice>(path.join(ROOT, "voice.yaml"));
  const snippets = loadSnippets();
  const familyFiles = fs.readdirSync(path.join(ROOT, "prompts")).filter(f => f.endsWith(".yaml"));

  if (!CHECK) fs.mkdirSync(OUT, { recursive: true });
  let drift = 0;

  for (const file of familyFiles) {
    const family = readYaml<Family>(path.join(ROOT, "prompts", file));
    for (const [name, prompt] of Object.entries(family.prompts)) {
      if (!prompt.key.startsWith(family.namespace + ".")) {
        console.error(`::error file=prompts/${file}::prompt '${name}' key '${prompt.key}' does not match namespace '${family.namespace}'`);
        process.exit(2);
      }
      const md = shimFor(family, prompt, voice, snippets);
      const outPath = path.join(OUT, `${prompt.key}.prompt.md`);
      if (CHECK) {
        const existing = fs.existsSync(outPath) ? fs.readFileSync(outPath, "utf8") : "";
        if (sha(existing) !== sha(md)) {
          console.error(`::error::drift in build/${prompt.key}.prompt.md — run \`node tools/render-md.ts\` and commit`);
          drift++;
        }
      } else {
        fs.writeFileSync(outPath, md);
        console.log(`wrote ${path.relative(ROOT, outPath)}`);
      }
    }
  }

  process.exit(drift > 0 ? 1 : 0);
}

main();
