using System;
using System.Collections.Generic;

namespace RM.Database.ResearchMantraContext;

public partial class PartnerAccount
{
    public long Id { get; set; }

    public string FullName { get; set; }

    public string MobileNumber { get; set; }

    public string EmailId { get; set; }

    public string City { get; set; }

    public string Source { get; set; }

    public int? AssignedTo { get; set; }

    public string TelegramId { get; set; }


    public double? Brokerage { get; set; }

    public Guid? LeadKey { get; set; }

    public string CustomerKey { get; set; }

    public string Remarks { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public Guid PublicKey { get; set; }

    public string CreatedIpAddress { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }

    public int? Status { get; set; }

}
public partial class PartnerComment
{
    public long Id { get; set; }

    public long PartnerId { get; set; }

    public string Comments { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }
}
public partial class PartnerAccountActivity
{
    public long Id { get; set; }

    public long PartnerAccountDetailId { get; set; }

    public string Comments { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }
}

 
public class PartnerAccountDetails
{
    public long Id { get; set; }
    public long PartnerAccountId { get; set; }
    public string PartnerCode { get; set; }
    public string PartnerCId { get; set; }
    public string GenerateToken { get; set; }
    public string Remarks { get; set; }
    public int? StatusId { get; set; }
    public int? AssignedTo { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public bool? IsVerified { get; set; }
    public bool? PartnerWith { get; set; }
}

public class AllParnterAccountCode : PartnerAccount
{
    public List<PartnerDetailDto> PartnerDetails { get; set; }
}

public class PartnerAccountRequestModel : AllParnterAccountCode
{
    public bool SoftUpdate { get; set; } = true;
}

public class PartnerCommnetResponseModel
{
    public object Comments { get; set; }
    public object Activities { get; set; }
    public object Details { get; set; }
}
public class PartnerDetailDto
{
    public long partnerAccountDetailId { get; set; }
    public string PartnerCode { get; set; }
    public string ClientId { get; set; }
    public int? AssignedTo { get; set; }
    public bool? PartnerWith { get; set; }
    public bool? IsVerified { get; set; }
    public int? StatusId { get; set; }
}
