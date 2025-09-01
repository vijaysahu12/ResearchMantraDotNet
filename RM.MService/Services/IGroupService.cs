using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.RequestModel;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;

namespace RM.MService.Services
{
    public interface IGroupService
    {
        Task<List<Groups>> GetAllGroupsAsync(GroupRequestModel request);
        Task<Group> CreateGroup(Group group);
    }

    public class GroupService : IGroupService
    {
        private readonly KingResearchContext _context;
        private readonly ApiCommonResponseModel apiCommonResponse = new ApiCommonResponseModel();

        public GroupService(KingResearchContext context)
        {
            _context = context;
        }

        public async Task<List<Groups>> GetAllGroupsAsync(GroupRequestModel request)
        {
            try
            {
                var sqlParameters = new List<SqlParameter>();

                if (request.GroupId.HasValue)
                    sqlParameters.Add(new SqlParameter("@Id", request.GroupId));
                if (!string.IsNullOrEmpty(request.GroupName))
                    sqlParameters.Add(new SqlParameter("@GroupName", request.GroupName));
                if (request.IsCommentEnabled.HasValue)
                    sqlParameters.Add(new SqlParameter("@IsCommentEnabled", request.IsCommentEnabled));

                sqlParameters.Add(new SqlParameter("@Action", "GetAllGroups"));
                sqlParameters.Add(new SqlParameter("@CreatedBy", request.CreatedBy));
                sqlParameters.Add(new SqlParameter("@ModifiedBy", request.ModifiedBy));

                var result = await _context.SqlQueryToListAsync<Groups>(
                    ProcedureCommonSqlParametersText.ManageGroup,
                    sqlParameters.ToArray()
                );

                apiCommonResponse.Data = result;
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Message = "All groups retrieved successfully.";

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                //...

                throw; // Rethrow the exception
            }
        }

        public async Task<Group> CreateGroup(Group group)
        {
            //if (group == null)
            //{
            //    throw new ArgumentNullException(nameof(group), "Group data is null");
            //}

            //try
            //{
            //    _context.Groups.Add(group);
            //    await _context.SaveChangesAsync(); // Save changes to the database

            //    return group;
            //}
            //catch (Exception ex)
            //{
            //    // Log the exception
            //    //...

            //    throw; // Rethrow the exception
            //}
            return null;
        }
    }

}
