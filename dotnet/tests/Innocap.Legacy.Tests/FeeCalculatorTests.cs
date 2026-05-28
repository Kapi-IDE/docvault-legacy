using System;
using FluentAssertions;
using Innocap.Legacy.Domain.Models;
using Innocap.Legacy.Services;
using Xunit;

// FeeCalculator unit tests — Priya, 2021
// Happy-path tests only. No edge cases. No tests for:
//   - HWM below opening NAV (should return 0 — does it? untested)
//   - Negative NAV (possible for some fund structures — untested)
//   - Very large notional values that expose double precision errors
//   - Zero units outstanding (divide-by-zero risk — untested)
//   - Day count convention differences (360 vs 365 — hardcoded to 365, untested)
// TODO: add edge cases (Priya, 2021) — never done before she left
namespace Innocap.Legacy.Tests
{
    public class FeeCalculatorTests
    {
        private readonly FeeCalculator _calc = new FeeCalculator();

        [Fact]
        public void ManagementFee_HappyPath_ReturnsCorrectAccrual()
        {
            // 2% annual management fee on $10M NAV for 31 days
            double result = _calc.CalculateManagementFee(10_000_000.0, 200, 31);

            // Expected: 10,000,000 * (0.02 / 365) * 31 = 16,986.30...
            result.Should().BeApproximately(16_986.30, 0.1);
        }

        [Fact]
        public void PerformanceFee_AboveHWM_ReturnsAccrual()
        {
            // 20% perf fee, NAV went from $100 to $110 per unit, 1000 units
            double result = _calc.CalculatePerformanceFeeAccrual(
                currentNav:       110.0,
                highWaterMark:    100.0,
                performanceFeePct: 20.0,
                unitsOutstanding:  1000.0);

            // Outperformance: $10 * 1000 units = $10,000. Fee: 20% of $10,000 = $2,000
            result.Should().Be(2000.0);
        }

        [Fact]
        public void PerformanceFee_BelowHWM_ReturnsZero()
        {
            double result = _calc.CalculatePerformanceFeeAccrual(
                currentNav:       95.0,
                highWaterMark:    100.0,
                performanceFeePct: 20.0,
                unitsOutstanding:  1000.0);

            result.Should().Be(0.0);
        }

        [Fact]
        public void AnnualisedReturn_HappyPath()
        {
            // 5% return over 180 days — annualised should be ~10.25%
            double result = _calc.CalculateAnnualisedReturn(100.0, 105.0, 180);

            result.Should().BeApproximately(0.1025, 0.001);
        }

        // NOTE: CalcularComisiones is NOT tested here — it requires NavStrike and ShareClass objects
        // and the share class setup in tests is "too much work" (Priya's TODO from 2021).
        // The HWM logic in CalcularComisiones has a known discrepancy found by Fleetguard (2022 Q3)
        // but there is no regression test for it.
    }
}
