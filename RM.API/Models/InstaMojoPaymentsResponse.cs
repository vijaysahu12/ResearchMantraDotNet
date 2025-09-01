namespace RM.API.Models
{
    public class InstaMojoPaymentsResponse
    {
        public string JsonData { get; set; }
        public int TotalCount { get; set; }

        public string Purpose { get; set; }
        public int CheckedInCount { get; set; }
        public int NotCheckedInCount { get; set; }


    }
}
