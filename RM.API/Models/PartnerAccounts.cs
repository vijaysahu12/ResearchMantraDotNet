//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;

//namespace RM.API.Models
//{
//    public partial class PartnerComments
//    {
//        public int Id { get; set; }
//        public string Comments { get; set; }
//        public DateTime CreatedOn { get; set; }
//        public int PartnerId { get; set; }
//        public int CreatedBy { get; set; }
//    }
//    public partial class PartnerAccounts
//    {
//        public long Id { get; set; }
//        public string FullName { get; set; }
//        public string MobileNumber { get; set; }
//        public string EmailId { get; set; }
//        public string City { get; set; }
//        public string ZerodhaCid { get; set; }
//        public string AngelCid { get; set; }
//        public string AliceBlueCid { get; set; }
//        public string EdelweissCId { get; set; }
//        public string FyersCId { get; set; }

//        public string Source { get; set; }

//        [Required]
//        public int AssignedTo { get; set; }
//        public string MotilalCId { get; set; }
//        public string TelegramId { get; set; }
//        public string LeadKey { get; set; }
//        public string CustomerKey { get; set; }
//        public string Remarks { get; set; }
//        public byte? IsDisabled { get; set; }
//        public byte? IsDelete { get; set; }
//        public Guid? PublicKey { get; set; }
//        public string CreatedIpAddress { get; set; }
//        public DateTime? CreatedOn { get; set; }
//        public string CreatedBy { get; set; }
//        public DateTime? ModifiedOn { get; set; }
//        public string ModifiedBy { get; set; }

//        public int Status { get; set; }

//        public float Brokerage { get; set; }
//    }

using System;

public class PartnerAccountsSP
{
    public long Id { get; set; }
    public long PartnerAccountDetailId { get; set; }
    public string FullName { get; set; }
    public string MobileNumber { get; set; }
    public string EmailId { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public string City { get; set; }
    public string PartnerCode { get; set; }
    public string PartnerCId { get; set; }
    //public string ClientId { get; set; }
    public string Details { get; set; }
    public string? PartnerWith { get; set; }
    public string Remarks { get; set; }
    public string PartnerComments { get; set; }
    public string Status { get; set; }
    public string StatusType { get; set; }
    public string TelegramId { get; set; }
    public double Brokerage { get; set; }
    public string Source { get; set; }
    public int AssignedTo { get; set; }
    public string AssignedToName { get; set; }
   

}
public class PartnerCommentsSP
{
    public long Id { get; set; }               // pa.Id
    public Guid PublicKey { get; set; }        // pa.PublicKey (uniqueidentifier in SQL)
    public string FullName { get; set; }       // pa.FullName
    public string MobileNumber { get; set; }   // pa.MobileNumber
    public string EmailId { get; set; }        // pa.EmailId
    public string City { get; set; }           // pa.City

    public string PartnerComment { get; set; } // pc.Comments
    public DateTime? CommentCreatedOn { get; set; } // pc.CreatedOn (nullable in case no comment exists)
    public string CommentCreatedBy { get; set; } // usCreated.FirstName + LastName

    public string Remarks { get; set; }        // pa.Remarks
}

public class CommonResponseSp
{
    public int Result { get; set; }
}
//}
