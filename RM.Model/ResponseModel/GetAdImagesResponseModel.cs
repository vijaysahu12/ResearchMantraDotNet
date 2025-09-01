using System;

namespace RM.Model.ResponseModel
{
    public class GetAdImagesResponseModel
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string Name { get; set; }
        public string? ProductName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public DateTime? ExpireOn { get; set; }
    }
}