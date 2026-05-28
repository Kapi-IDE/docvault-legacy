using System;
using System.Collections.Generic;

namespace Innocap.Legacy.Domain.Models
{
    // Fund master record — managed account structure, one fund per investor sleeve
    public class Fund
    {
        public int FundId { get; set; }

        // e.g. "PINE-MASTER-01", "EASTVALE-FA-02"
        public string FundCode { get; set; }

        public string FundName { get; set; }

        // LEI — Legal Entity Identifier (20 chars, alphanumeric)
        public string LegalEntityIdentifier { get; set; }

        public string DomicileCountry { get; set; }

        // USD, EUR, CAD — base currency for NAV
        public string BaseCurrency { get; set; }

        public DateTime InceptionDate { get; set; }

        public bool IsActive { get; set; }

        public List<ShareClass> ShareClasses { get; set; }
    }
}
