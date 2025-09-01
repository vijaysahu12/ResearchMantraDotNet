using Microsoft.AspNetCore.Http;

namespace RM.Model.RequestModel
{
    public class CompanyReportExcelImportModal
    {
        public IFormFile excelFile { get; set; }
        //public int BasketId { get; set; }
        //public bool IsPublished { get; set; }
        //public Guid LoggedInUserKey { get; set; }

    }
}
