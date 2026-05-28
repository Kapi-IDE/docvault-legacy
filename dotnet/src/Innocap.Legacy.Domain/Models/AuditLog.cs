using System;

namespace Innocap.Legacy.Domain.Models
{
    // Audit log for MNPI-sensitive operations — SEC/CFTC requirement
    // NOTE: audit log writes are currently DISABLED in StatementsController
    // since a 2022 incident caused duplicate entries. Re-enable after the
    // schema fix (open since 2022-09-14, assigned to nobody).
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public int InvestorId { get; set; }

        // e.g. "STATEMENT_VIEWED", "POSITION_QUERIED", "NAV_STRIKE_CREATED"
        public string EventType { get; set; }

        public string EntityType { get; set; }

        public int EntityId { get; set; }

        public string Description { get; set; }

        public string PerformedBy { get; set; }

        public string IpAddress { get; set; }

        public DateTime Timestamp { get; set; }

        // Serialised JSON snapshot of the entity at time of event
        public string Snapshot { get; set; }
    }
}
