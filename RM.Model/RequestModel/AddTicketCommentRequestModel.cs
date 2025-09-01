using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace RM.Model.RequestModel
{
    public class AddTicketCommentRequestModel
    {
        public int TicketId { get; set; }
        public string Comment { get; set; }
        public IFormFileCollection? Images { get; set; }

        public Guid MobileUserKey { get; set; }
        public Guid? CrmUserKey { get; set; }
        public List<string> AspectRatios { get; set; }
    }
}