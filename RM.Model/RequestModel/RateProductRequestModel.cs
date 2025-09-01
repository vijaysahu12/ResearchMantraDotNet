using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class RateProductRequestModel
    {
        [Required(ErrorMessage = "User key is required")]
        public string UserKey { get; set; }

        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        public float Rating { get; set; }
    }
}
