using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class GetProductContentRequestModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Userkey { get; set; }
    }
}
