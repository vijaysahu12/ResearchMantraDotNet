//namespace RM.API.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;
//    using System.Data.Entity.Spatial;

//    public partial class Pincode
//    {
//        public long Id { get; set; }

//        [Column("Pincode")]
//        [Required]
//        [StringLength(50)]
//        public string PincodeValue { get; set; }

//        [StringLength(150)]
//        public string Area { get; set; }

//        [StringLength(150)]
//        public string Division { get; set; }

//        [StringLength(150)]
//        public string District { get; set; }

//        [StringLength(150)]
//        public string State { get; set; }

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
