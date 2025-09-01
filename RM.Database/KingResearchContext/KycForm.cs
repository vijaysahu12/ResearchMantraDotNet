using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Database.KingResearchContext
{
    public class RpfForm
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Pan { get; set; }
        public string? Aadhaar { get; set; }
        public string? Email { get; set; }
        public string? CountryCode { get; set; }
        public string? Mobile { get; set; }

        public string? Q1 { get; set; }
        public string? Q2 { get; set; }
        public string? Q3 { get; set; }
        public string? Q4 { get; set; }
        public string? Q5 { get; set; }
        public string? Q6 { get; set; }
        public string? Q7 { get; set; }
        public string? Q8 { get; set; }
        public string? Q9 { get; set; }
        public string? Q10 { get; set; }
        public string? Q11 { get; set; }
        public string? Q12 { get; set; }
        public string? Q13 { get; set; }
        public string? Q14 { get; set; }
        public string? Q14OtherText { get; set; }
        public string? Q15 { get; set; }
        public string? Q16 { get; set; }
        public string? Q17 { get; set; }
        public string? Q18 { get; set; }
        public string? Q19 { get; set; }
        public string? Q20 { get; set; }
        public string? Q21 { get; set; }
        public string? Q22 { get; set; }

        public string? RiskConsent { get; set; }
        public string? SupportConsent { get; set; }
        public string? PdfFileLocation { get; set; }

        public DateTime? CreatedOn { get; set; }
        public string? ServiceName { get; set; }
        public string? Status { get; set; }
        public string? AltMobile { get; set; }
        public string? RpfApprovedBy { get; set; }
        public DateTime? RpfApprovedOn { get; set; }
        public bool? IsRpfApproved { get; set; }
        public string? AdviserConsent { get; set; }
        public string? DeclarationConsent { get; set; }
    }


}