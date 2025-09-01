using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace RM.Model.RequestModel.MobileApi
{
    public class ManageTicketRequestModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public Guid CreatedBy { get; set; }
        public string TicketType { get; set; }
        public string Priority { get; set; }
        public char Status { get; set; }
        public string Comment { get; set; }
        public IFormFileCollection? Images { get; set; }
        public bool? IsActive { get; set; }
        public string Subject { get; set; }
        public List<string> AspectRatios { get; set; }
    }
}