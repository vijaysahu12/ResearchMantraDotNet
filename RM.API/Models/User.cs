//namespace RM.API.Models
//{
//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel.DataAnnotations;
//    using System.ComponentModel.DataAnnotations.Schema;
//    using System.Data.Entity.Spatial;

//    public partial class User
//    {
//        public int Id { get; set; }

//        [StringLength(100)]
//        public string FirstName { get; set; }

//        [StringLength(100)]
//        public string LastName { get; set; }

//        [StringLength(100)]
//        public string MobileNumber { get; set; }

//        [StringLength(300)]
//        public byte[] PasswordHash { get; set; }

//        [StringLength(50)]
//        public string Password { get; set; }

//        [StringLength(300)]
//        public byte[] PasswordSalt { get; set; }

//        [StringLength(100)]
//        [Required]
//        public string EmailId { get; set; }

//        [StringLength(100)]
//        public string DOJ { get; set; }

//        [StringLength(300)]
//        public string Address { get; set; }

//        [StringLength(100)]
//        public string RoleKey { get; set; }

//        [StringLength(100)]
//        public string Gender { get; set; }

//        [StringLength(300)]
//        public string UserImage { get; set; }

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
