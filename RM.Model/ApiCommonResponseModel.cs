using System.Net;

namespace RM.Model
{
    public class ApiCommonResponseModel : CommonResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object Exceptions { get; set; }

    }

    //public class ApiCommonResponseModel
    //{
    //    public HttpStatusCode StatusCode { get; set; }
    //    public string Message { get; set; }
    //    public object Data { get; set; }
    //    public int Total { get; set; }
    //}


    public class CommonResponse
    {
        public int Total { get; set; }
    }
}
