using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class BlockUserRequestModel
    {
        [Required]
        public Guid UserKey { get; set; }
        [Required]
        public string BlockedId { get; set; }
        [Required]
        public string Type { get; set; }
    }
}
