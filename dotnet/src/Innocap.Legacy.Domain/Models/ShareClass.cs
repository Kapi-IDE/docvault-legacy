using System;

namespace Innocap.Legacy.Domain.Models
{
    // Share class within a fund — each investor holds a specific class
    public class ShareClass
    {
        public int ShareClassId { get; set; }

        public int FundId { get; set; }

        // e.g. "Class A", "Class B Founders", "Class I Institutional"
        public string ClassName { get; set; }

        public string CurrencyCode { get; set; }

        // Management fee in basis points (e.g. 150 = 1.50%)
        public int ManagementFeeBps { get; set; }

        // Performance fee as a percentage (e.g. 20.0 = 20%)
        // TODO: verify if this should be decimal — Priya, 2021
        public double PerformanceFeePct { get; set; }

        public DateTime InceptionDate { get; set; }

        public Fund Fund { get; set; }
    }
}
