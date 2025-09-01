//namespace RM.API.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;
//    using System.Data.Entity.Spatial;

//    public partial class CustomerService
//    {
//        public long Id { get; set; }

//        [StringLength(100)]
//        public string CustomerKey { get; set; }

//        [StringLength(100)]
//        public string ServiceKey { get; set; }

//        public decimal? ActualCost { get; set; }
//        public decimal? AmountPaid { get; set; }
//        public decimal? AmountOutstanding { get; set; }
//        public decimal? Discount { get; set; }


//        [StringLength(100)]
//        public string PaymentModeKey { get; set; }

//        public byte? IsWon { get; set; }

//        [StringLength(100)]
//        public string OrderId { get; set; }

//        [StringLength(500)]
//        public string Remarks { get; set; }

//        public byte? IsDisabled { get; set; }

//        public byte? IsDelete { get; set; }

//        public Guid? PublicKey { get; set; }

//        public DateTime? CreatedOn { get; set; }

//        [StringLength(100)]
//        public string CreatedBy { get; set; }

//        public DateTime? ModifiedOn { get; set; }

//        [StringLength(100)]
//        public string ModifiedBy { get; set; }
//    }
//}
