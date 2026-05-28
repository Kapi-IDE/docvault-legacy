using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Innocap.Legacy.Domain.Models
{
    // Original model — Carlos M., 2019
    // Note: InvestorId as int (not Guid). Discussed changing this after the merger but
    // Jean-Pierre said "not worth the migration risk". Still an int.
    public class Investor
    {
        public int InvestorId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        // Raw hash stored here — see PasswordHasher.cs
        // TODO: consider separating credentials to own table (Carlos, 2019)
        public string PasswordHash { get; set; }

        public string LegalEntityName { get; set; }

        public string JurisdictionCode { get; set; }

        // e.g. QUALIFIED_PURCHASER, ACCREDITED_INVESTOR, INSTITUTIONAL
        public string InvestorType { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        // Nullable annotations not enabled in this project — no ?
        public bool IsActive { get; set; }

        [JsonIgnore]
        public List<Position> Positions { get; set; }
    }
}
