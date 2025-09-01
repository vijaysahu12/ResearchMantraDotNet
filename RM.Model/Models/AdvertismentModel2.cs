using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RM.Model
{
    public class AdvertismentModel2
    {


        public Dictionary<string, string> Data { get; set; }


        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        public string Topic { get; set; }
    }
}