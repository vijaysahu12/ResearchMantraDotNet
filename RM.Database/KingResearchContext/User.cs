using System;
using System.Collections.Generic;

namespace RM.Database.KingResearchContext;

public partial class User
{
    public int Id { get; set; }

    //public Guid UserKey { get; set; } = Guid.NewGuid();
    public int? SupervisorId { get; set; }
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MobileNumber { get; set; }

    public byte[] PasswordHash { get; set; }

    public byte[] PasswordSalt { get; set; }

    public string EmailId { get; set; }

    public string Doj { get; set; }

    public string Address { get; set; }

    public string RoleKey { get; set; }

    public string Gender { get; set; }

    public string UserImage { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }

    public string Password { get; set; }

    public string PasswordKey { get; set; }
    public bool? IsUpdateAllow { get; set; }

  //  public Guid UserKey { get; set; }
}

public partial class UserMappings
{
    public int Id { get; set; }
    public Guid? UserKey { get; set; }
    public string UserType { get; set; }
    public bool IsActive { get; set; }
    public bool IsDelete { get; set; }
}

public class UserActivity
{
    public int Id { get; set; }
    public Guid PublicKey { get; set; }
    public Guid? LeadKey { get; set; }
    public string Message { get; set; }
    public string Description { get; set; }
    public DateTime CreatedOn { get; set; }
    public int ActivityType { get; set; }
}
public partial class UserType
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class AssignBDERequest
{
    public int? SalesLeadId { get; set; }
    public List<int> BdeUserIds { get; set; }
}

