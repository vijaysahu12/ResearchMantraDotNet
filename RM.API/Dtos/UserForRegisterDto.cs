using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace RM.API.Dtos
{
    public class UserForRegisterDto
    {
        public int Id { get; set; }


        [Required]
        [EmailAddress(ErrorMessage = "Username should be a valid email address")]
        public string EmailId { get; set; }

        [Required]

        [StringLength(15, MinimumLength = 4, ErrorMessage = "Password length should be between 4 to 15 characters.")]
        public string Password { get; set; }

        [StringLength(300)]
        public byte[] PasswordHash { get; set; }

        [StringLength(300)]
        public byte[] PasswordSalt { get; set; }

        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(100)]
        public string MobileNumber { get; set; }

        [StringLength(100)]
        public string DOJ { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        [StringLength(100)]
        public string RoleKey { get; set; }

        [StringLength(100)]
        public string Gender { get; set; }

        [StringLength(300)]
        public string UserImage { get; set; }

        public byte? IsDisabled { get; set; }

        public byte? IsDelete { get; set; }

        public Guid? PublicKey { get; set; }

        public DateTime? CreatedOn { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        [StringLength(100)]
        public string ModifiedBy { get; set; }
        public IFormFile file { get; set; }
    }

    public class UserImageDto
    {
        public int Id { get; set; }
        public IFormFile file { get; set; }
    }
}
