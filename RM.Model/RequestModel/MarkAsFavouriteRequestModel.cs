using System;

namespace RM.Model.RequestModel
{
    public class MarkAsFavouriteRequestModel
    {
        public Guid LeadKey { get; set; }
        public bool Favourite { get; set; }
    }
}
