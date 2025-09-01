using System;

namespace RM.API.Models
{
    public class ValidateInstaMojoPaymentResponseModel
    {
        public string FullName { get; set; }
        public long Id { get; set; }
        public Guid PublicKey { get; set; }
        public Guid AssignedTo { get; set; }
        public string TransasctionReference { get; set; }
    }
}
