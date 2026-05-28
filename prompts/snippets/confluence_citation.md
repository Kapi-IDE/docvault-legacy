# confluence_citation — included via {{>confluence_citation}}

When citing a Confluence page, use the canonical format:

    [<page title>](confluence:<space>/<page_id>)

Example:

    [NAV reconciliation runbook](confluence:OPS/1234567)

Rules:
- Always include the numeric page ID — titles change, IDs do not.
- Quote step text verbatim when summarizing a runbook. Do not
  paraphrase the operative instructions.
- If you cannot find a citation, say so plainly:
    NO_RUNBOOK_MATCH: <query>
  Never invent a page ID or fabricate steps.
