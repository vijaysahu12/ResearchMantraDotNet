using Microsoft.AspNetCore.Http;

namespace RM.API.Models
{
    public class ExcelImportModal
    {
        public IFormFile excelFile { get; set; }
        public string leadType { get; set; }
        public string leadSource { get; set; }
        public string leadService { get; set; }
        public string publicKey { get; set; }
        public string comments { get; set; }
    }
}
