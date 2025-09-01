using System;

namespace RM.API.Models
{
    public class InstaMojoQrCodeResponse
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public string Quantity { get; set; }
        public string Status { get; set; }
        public double Fees { get; set; }
        public string Purpose { get; set; }
        public string PaymentId { get; set; }
        public string Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public int MailSent { get; set; }
    }
}
