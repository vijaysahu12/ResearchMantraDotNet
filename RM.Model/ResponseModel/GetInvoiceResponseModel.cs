using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RM.Model.ResponseModel
{
    public class GetInvoiceResponseModel
    {
        public Guid? PublicKey { get; set; }
        public long? LeadId { get; set; }
        public string? FileName { get; set; }
        public string? TransactionId { get; set; }
        public string? TransasctionReference { get; set; }
        public string? CouponCode { get; set; }
        public string? ClientName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        [JsonIgnore]
        public DateTime? StartDate { get; set; }

        [JsonIgnore]
        public DateTime? EndDate { get; set; }

        [JsonIgnore]
        public DateTime? CreatedOn { get; set; }

        [JsonIgnore]
        public DateTime? PaymentDate { get; set; }

        [JsonPropertyName("startDate")]
        public string? FormattedStartDate => StartDate?.ToString("dd-MMM-yyyy");

        [JsonPropertyName("endDate")]
        public string? FormattedEndDate => EndDate?.ToString("dd-MMM-yyyy");

        [JsonPropertyName("createdOn")]
        public string? FormattedCreatedOn => CreatedOn?.ToString("dd-MMM-yyyy hh:mm tt");

        [JsonPropertyName("paymentDate")]
        public string? FormattedPaymentDate => PaymentDate?.ToString("dd-MMM-yyyy hh:mm tt");
        public string? ProductName { get; set; }
        public decimal? PaidAmount { get; set; }

        [NotMapped]
        public string? DownloadUrl { get; set; }
    }
}
