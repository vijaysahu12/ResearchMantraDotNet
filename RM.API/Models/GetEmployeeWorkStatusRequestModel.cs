using System;

namespace RM.API.Models
{
    public class GetEmployeeWorkStatusRequestModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
