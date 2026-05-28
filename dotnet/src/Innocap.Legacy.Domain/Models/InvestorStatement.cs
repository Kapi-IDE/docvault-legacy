using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Innocap.Legacy.Domain.Models
{
    // Monthly investor statement — generated on-demand or scheduled
    // The generation logic lives in PositionNavStatementService.GenererReleve (oui, French)
    public class InvestorStatement
    {
        public int StatementId { get; set; }

        public int InvestorId { get; set; }

        public int FundId { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public DateTime GeneratedAt { get; set; }

        public string GeneratedBy { get; set; }

        // Opening NAV for the period
        public double OpeningNav { get; set; }

        // Closing NAV for the period
        public double ClosingNav { get; set; }

        // Net P&L — still a double. See Position.cs.
        public double NetPnl { get; set; }

        // Performance fee accrual — see FeeCalculator.cs (HWM TODO unresolved since 2020)
        public double PerformanceFeeAccrual { get; set; }

        // PDF blob path on disk — no cloud storage configured
        // TODO: move to Azure Blob (Jean-Pierre, 2022)
        public string PdfFilePath { get; set; }

        public string Status { get; set; }

        [JsonProperty("positions")]
        public List<Position> Positions { get; set; }

        public Investor Investor { get; set; }

        public Fund Fund { get; set; }
    }
}
