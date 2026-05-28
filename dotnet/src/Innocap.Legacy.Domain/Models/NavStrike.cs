using System;

namespace Innocap.Legacy.Domain.Models
{
    // NAV strike record — calculated daily by fund admin, stored here for investor portal
    public class NavStrike
    {
        public int NavStrikeId { get; set; }

        public int FundId { get; set; }

        public int ShareClassId { get; set; }

        public DateTime StrikeDate { get; set; }

        // NAV per unit — also a double. See Position.cs for the rant.
        // The 2022 Q4 hotfix in NavController works around the rounding errors this causes.
        public double NavPerUnit { get; set; }

        public double TotalNetAssets { get; set; }

        public double UnitsOutstanding { get; set; }

        public string CurrencyCode { get; set; }

        // Provisional or Final
        public string StrikeType { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public Fund Fund { get; set; }

        public ShareClass ShareClass { get; set; }
    }
}
