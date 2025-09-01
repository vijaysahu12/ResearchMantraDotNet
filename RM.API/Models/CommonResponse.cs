namespace RM.API.Models
{
    public class CommonResponse
    {
        public int Total { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public object Data { get; set; }
    }
}
