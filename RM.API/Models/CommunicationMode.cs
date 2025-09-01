//namespace RM.API.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;
//    using System.Data.Entity.Spatial;

//    public partial class CommunicationMode
//    {
//        public int Id { get; set; }

//        [StringLength(100)]
//        public string Name { get; set; }

//        [StringLength(300)]
//        public string Description { get; set; }

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
