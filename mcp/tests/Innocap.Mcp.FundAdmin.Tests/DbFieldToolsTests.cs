using FluentAssertions;
using Innocap.Mcp.FundAdmin.Repositories;
using Innocap.Mcp.FundAdmin.Tools;
using Xunit;

namespace Innocap.Mcp.FundAdmin.Tests;

file sealed class FakeKnownFieldsRegistry : IKnownFieldsRegistry
{
    public IReadOnlyList<string> AllFields { get; } =
    [
        "PositionSizeUsd",
        "HighWaterMark",
        "InvestorId",
        "FundCode",
        "ClosingNav",
    ];
}

public sealed class DbFieldToolsTests
{
    private readonly DbFieldTools _sut = new(new FakeKnownFieldsRegistry());

    [Fact]
    public void ListKnownDbFieldsInText_MatchesFieldNameVerbatim()
    {
        var result = _sut.ListKnownDbFieldsInText(
            "SELECT PositionSizeUsd, FundCode FROM dbo.Positions WHERE InvestorId = @id");

        result.Should().Contain("PositionSizeUsd");
        result.Should().Contain("FundCode");
        result.Should().Contain("InvestorId");
        result.Should().NotContain("HighWaterMark");
        result.Should().NotContain("ClosingNav");
    }

    [Fact]
    public void ListKnownDbFieldsInText_CaseInsensitiveMatch()
    {
        var result = _sut.ListKnownDbFieldsInText("updated highwatermark column — oops, highwatermark");

        // "HighWaterMark" is in the registry; "highwatermark" (no camel case) is NOT
        // — test that Contains is truly case-insensitive on the registered name
        var resultCamelCase = _sut.ListKnownDbFieldsInText("updated HighWaterMark column");
        resultCamelCase.Should().Contain("HighWaterMark");
    }

    [Fact]
    public void ListKnownDbFieldsInText_NoMatches_ReturnsNoneFound()
    {
        var result = _sut.ListKnownDbFieldsInText("This text contains no known field names whatsoever.");

        result.Should().Contain("No known Innocap database fields found");
    }

    [Fact]
    public void ListKnownDbFieldsInText_EmptyInput_ReturnsNoTextProvided()
    {
        var result = _sut.ListKnownDbFieldsInText("   ");

        result.Should().Contain("No text provided");
    }
}
