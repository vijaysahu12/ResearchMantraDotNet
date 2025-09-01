//namespace RM.API.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;
//    using System.Data.Entity.Spatial;

//    public partial class Service
//    {
//        public int Id { get; set; }

//        [StringLength(250)]
//        public string Name { get; set; }

//        public string Description { get; set; }

//        public decimal? ServiceCost { get; set; }

//        [StringLength(100)]
//        public string ServiceTypeKey { get; set; }

//        [StringLength(100)]
//        public string ServiceCategoryKey { get; set; }

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
