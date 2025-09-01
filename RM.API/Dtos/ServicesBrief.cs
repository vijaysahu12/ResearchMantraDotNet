using System.ComponentModel.DataAnnotations;


namespace RM.API.Dtos
{
    public class ServicesBrief
    {
        public int Id { get; set; }

        [StringLength(250)]
        public string ServiceName { get; set; }

        [StringLength(250)]
        public string ServiceType { get; set; }

        [StringLength(250)]
        public string ServiceCategory { get; set; }


        public string PublicKey { get; set; }
    }
}
