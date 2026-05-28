using FluentAssertions;
using Innocap.Mcp.FundAdmin.Repositories;
using Innocap.Mcp.FundAdmin.Tools;
using Xunit;

namespace Innocap.Mcp.FundAdmin.Tests;

file sealed class FakeFundRegistry : IFundRegistry
{
    private static readonly FundMetadata CaymanFund = new(
        FundCode: "ABC-123",
        FundName: "Alpha Builders Cayman Master Fund Ltd",
        Jurisdiction: "Cayman Islands",
        AifmdApplicable: false,
        UcitsApplicable: false,
        ShareClassCount: 3,
        Custodian: "BNY Mellon",
        BaseCurrency: "USD");

    private static readonly FundMetadata EuFund = new(
        FundCode: "PINEGROVE-001",
        FundName: "Pinegrove Eurozone QIAIF",
        Jurisdiction: "Ireland",
        AifmdApplicable: true,
        UcitsApplicable: false,
        ShareClassCount: 4,
        Custodian: "State Street",
        BaseCurrency: "EUR");

    private readonly Dictionary<string, FundMetadata> _funds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ABC-123"] = CaymanFund,
        ["PINEGROVE-001"] = EuFund,
    };

    public FundMetadata? FindByCode(string fundCode) =>
        _funds.TryGetValue(fundCode.Trim(), out var f) ? f : null;
}

public sealed class FundToolsTests
{
    private readonly FundTools _sut = new(new FakeFundRegistry());

    [Fact]
    public void GetFundMetadata_KnownCode_ReturnsStructuredMetadata()
    {
        var result = _sut.GetFundMetadata("ABC-123");

        result.Should().Contain("ABC-123");
        result.Should().Contain("Cayman Islands");
        result.Should().Contain("BNY Mellon");
        result.Should().Contain("3"); // share classes
        // Must NOT mention NAV/position/investor details
        result.Should().NotContain("NAV");
        result.Should().NotContain("position");
    }

    [Fact]
    public void GetFundMetadata_AifmdFund_IncludesAifmdRegulation()
    {
        var result = _sut.GetFundMetadata("PINEGROVE-001");

        result.Should().Contain("AIFMD");
        result.Should().Contain("Ireland");
    }

    [Fact]
    public void GetFundMetadata_UnknownCode_ReturnsNotFoundMessage()
    {
        var result = _sut.GetFundMetadata("DOES-NOT-EXIST");

        result.Should().Contain("not found");
    }
}
