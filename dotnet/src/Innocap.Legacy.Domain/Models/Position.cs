using System;

namespace Innocap.Legacy.Domain.Models
{
    // MNPI WARNING: This model contains material non-public information.
    // Do NOT expose position data to unauthenticated callers.
    // (This warning has been here since 2019 and ignored at least once — see AdminController.)
    public class Position
    {
        public int PositionId { get; set; }

        public int InvestorId { get; set; }

        public int FundId { get; set; }

        // Ticker or ISIN depending on asset class — inconsistent, Carlos's note: "fix later"
        public string Instrument { get; set; }

        public string InstrumentType { get; set; }

        // CRITICAL SMELL: PositionSizeUsd typed as double.
        // USD financial values must NEVER use double — floating-point rounding
        // causes P&L discrepancies. Should be decimal.
        // Day 3 exercise: change this to decimal and fix the cascade.
        // — Carlos, 2019 ("I'll fix it in v2")
        public double PositionSizeUsd { get; set; }

        // Also double. Same problem.
        public double MarketValueUsd { get; set; }

        // Quantity of units/shares held
        public double Quantity { get; set; }

        public double AverageCostBasis { get; set; }

        public DateTime AsOfDate { get; set; }

        public string FundCode { get; set; }

        public Investor Investor { get; set; }

        public Fund Fund { get; set; }
    }
}
