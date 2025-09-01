using System.ComponentModel;

namespace RM.API.Models.Constants
{
    public class Constants
    {
    }


    public enum PartnerAccountStatus
    {
        Fresh = 0,
        Pending = 1,
        Accepted = 2,
        Rejected = 3,
        Followup = 4,
        [Description("Not Interested")]
        NotInterested = 5,
        NPC = 6,
        LinkedToJarvis = 7,
        InsufficentFunds = 8,
        SignatureMismatch = 9,
        InvalidAPIKeys = 10,
        LinkShared = 11

    }

    public enum LeadStatus
    {
        Fresh = 1,
        Pending = 2,
        High = 3,
        Medium = 4,
        Low = 5
    }
}
