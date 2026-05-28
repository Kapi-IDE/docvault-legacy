using FluentAssertions;
using Innocap.Mcp.FundAdmin.Repositories;
using Innocap.Mcp.FundAdmin.Tools;
using Xunit;

namespace Innocap.Mcp.FundAdmin.Tests;

file sealed class FakeJiraProjectRegistry : IJiraProjectRegistry
{
    public IReadOnlyList<string> Prefixes { get; } = ["INNOCAP", "PLAT", "RISK", "PORTAL"];
}

public sealed class TicketToolsTests
{
    private readonly TicketTools _sut = new(new FakeJiraProjectRegistry());

    [Fact]
    public void ListJiraTicketIdsInText_FindsKnownPrefixedIds()
    {
        var result = _sut.ListJiraTicketIdsInText(
            "Fixed in INNOCAP-4242 and PLAT-100. Relates to RISK-7 as well.");

        result.Should().Contain("INNOCAP-4242");
        result.Should().Contain("PLAT-100");
        result.Should().Contain("RISK-7");
    }

    [Fact]
    public void ListJiraTicketIdsInText_IgnoresUnknownPrefixes()
    {
        var result = _sut.ListJiraTicketIdsInText(
            "See JIRA-999 and UNKNOWNPROJECT-1 — not our prefix.");

        result.Should().Contain("No Jira ticket IDs matching known prefixes");
    }

    [Fact]
    public void ListJiraTicketIdsInText_DeduplicatesRepeatedIds()
    {
        var result = _sut.ListJiraTicketIdsInText(
            "INNOCAP-100 merged. Reverted in INNOCAP-100 same day.");

        result.Should().ContainSingle(line => line.Contains("INNOCAP-100"));
    }

    [Fact]
    public void ListJiraTicketIdsInText_EmptyInput_ReturnsNoTextProvided()
    {
        var result = _sut.ListJiraTicketIdsInText(string.Empty);

        result.Should().Contain("No text provided");
    }
}
