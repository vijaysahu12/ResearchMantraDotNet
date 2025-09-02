using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.RequestModel.MobileApi;
using RM.Model.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Net;

namespace RM.MService.Services
{
    public interface ITicketService
    {
        Task<ApiCommonResponseModel> Get(Guid mobileUserKey);

        Task<ApiCommonResponseModel> Add(ManageTicketRequestModel requestObj);

        Task<ApiCommonResponseModel> Manage(ManageTicketRequestModel requestObj, Guid modifiedBy);

        ApiCommonResponseModel GetSupportMobile();

        Task<ApiCommonResponseModel> AddTicketComment(AddTicketCommentRequestModel request);

        Task<ApiCommonResponseModel> GetTicketDetails(int id);
    }

    public class TicketService : ITicketService
    {
        private readonly ResearchMantraContext _context;
        private ApiCommonResponseModel apiCommonResponseModel = null;
        private readonly IConfiguration _config;
        private readonly ApiCommonResponseModel responseModel = new();

        public TicketService(ResearchMantraContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        public async Task<ApiCommonResponseModel> Get(Guid mobileUserKey)
        {
            responseModel.Data = await _context.TicketM
                .Where(item => item.CreatedBy == mobileUserKey && item.IsDelete != true)
                .OrderByDescending(order => order.CreatedOn).ToListAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> Add(ManageTicketRequestModel requestObj)
        {
            try
            {
                var imagesJson = requestObj.Images is not null ? await ConvertImagesToJsonAsync(requestObj.Images, requestObj.AspectRatios) : null;

                var newTicket = new TicketM
                {
                    CreatedBy = requestObj.CreatedBy,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsDelete = false,
                    Priority = requestObj.Priority,
                    TicketType = requestObj.TicketType,
                    Subject = requestObj.Subject,
                    Description = requestObj.Description,
                    Status = 'O',
                    Images = imagesJson
                };
                await _context.TicketM.AddAsync(newTicket);

                responseModel.StatusCode = HttpStatusCode.OK;
                _ = await _context.SaveChangesAsync();


                List<SqlParameter> sqlParameters =
                [
                    new SqlParameter
                    {
                        ParameterName = "Comment",
                        Value = requestObj.Description,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 1000
                    },
                    new SqlParameter
                    {
                        ParameterName = "TicketId",
                        Value = newTicket.Id,
                        SqlDbType = SqlDbType.Int
                    },
                    new SqlParameter
                    {
                        ParameterName = "MobileUserKey",
                        Value = requestObj.CreatedBy,
                        SqlDbType = SqlDbType.UniqueIdentifier,
                    },
                    new SqlParameter
                    {
                        ParameterName = "CrmUserKey",
                        Value = DBNull.Value,
                        SqlDbType = SqlDbType.UniqueIdentifier,
                    },
                    new()
                    {
                        ParameterName = "Images",
                        Value = imagesJson is null ? DBNull.Value : imagesJson,
                        SqlDbType = SqlDbType.VarChar
                    },
                ];

                var spResult =
                    await _context.SqlQueryFirstOrDefaultAsync2<AddTicketSpResponseModel>(
                        ProcedureCommonSqlParametersText.AddTicketCommentM, sqlParameters.ToArray());

                if (string.Equals(spResult.Message, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Ticket raised successfully.";
                    responseModel.Data = new
                    {
                        ticketId = newTicket.Id,
                        comment = new
                        {
                            id = newTicket.Id,
                            content = newTicket.Comment,
                            sender = "user",
                            images = imagesJson is not null ? JsonConvert.DeserializeObject<List<ImageResponseModel>>(newTicket.Images) : null,
                            timestamp = newTicket.CreatedOn
                        }
                    };
                }
                else
                {
                    responseModel.StatusCode = HttpStatusCode.InternalServerError;
                    responseModel.Message = "An error occured while adding comment.";
                }

                return responseModel;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<ApiCommonResponseModel> AddTicketComment(AddTicketCommentRequestModel request)
        {
            ApiCommonResponseModel responseModel = new();
            var imagesJson = request.Images is not null ? await ConvertImagesToJsonAsync(request.Images, request.AspectRatios) : null;
            try
            {
                List<SqlParameter> sqlParameters =
                [
                    new()
                    {
                        ParameterName = "Comment",
                        Value = request.Comment ?? (object)DBNull.Value,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 1000
                    },
                    new()
                    {
                        ParameterName = "TicketId",
                        Value = request.TicketId,
                        SqlDbType = SqlDbType.Int
                    },
                    new()
                    {
                        ParameterName = "MobileUserKey",
                        Value = request.MobileUserKey,
                        SqlDbType = SqlDbType.UniqueIdentifier,
                    },
                    new()
                    {
                        ParameterName = "CrmUserKey",
                        Value = DBNull.Value,
                        SqlDbType = SqlDbType.UniqueIdentifier,
                    },
                    new()
                    {
                        ParameterName = "Images",
                        Value = imagesJson is null ? DBNull.Value : imagesJson,
                        SqlDbType = SqlDbType.VarChar
                    },
                ];

                var spResult = await _context.SqlQueryFirstOrDefaultAsync2<AddTicketSpResponseModel>(ProcedureCommonSqlParametersText.AddTicketCommentM, [.. sqlParameters]);

                if (spResult != null && string.Equals(spResult.Message, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    var ticket = await _context.TicketM.FirstOrDefaultAsync(x => x.Id == request.TicketId);
                    if (ticket != null)
                    {
                        ticket.ModifiedBy = request.MobileUserKey;
                        ticket.ModifiedOn = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Added successfully.";
                    responseModel.Data = new
                    {
                        id = spResult.Id,
                        content = request.Comment,
                        sender = "user",
                        timestamp = DateTime.Now,
                        ticketStatus = ticket?.Status,
                        images = imagesJson is not null ? JsonConvert.DeserializeObject<List<ImageResponseModel>>(imagesJson) : null
                    };
                }
                else
                {
                    responseModel.StatusCode = HttpStatusCode.InternalServerError;
                    responseModel.Message = "An error occurred while adding the comment.";
                }
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = "An unexpected error occurred.";
                responseModel.Data = new { Error = ex.Message };
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> Manage(ManageTicketRequestModel requestObj, Guid modifiedBy)
        {
            var ticketObj = _context.TicketM.Where(item => item.Id == requestObj.Id).FirstOrDefault();

            if (ticketObj != null)
            {
                ticketObj.Status = requestObj.Status;
                ticketObj.Comment = requestObj.Comment;
                ticketObj.IsActive = requestObj.IsActive ?? true;
                ticketObj.ModifiedBy = modifiedBy;
                ticketObj.ModifiedOn = DateTime.Now;
                await _context.SaveChangesAsync();
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Successfully updated";
            }
            else
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "Ticket Not Found.";
            }

            return responseModel;
        }

        public ApiCommonResponseModel GetSupportMobile()
        {
            responseModel.Data = _config["Support:Mobile"];
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetTicketDetails(int id)
        {
            var ticket = await _context.TicketM.FirstOrDefaultAsync(x => x.Id == id);

            if (ticket == null)
            {
                responseModel.Data = null;
                responseModel.Message = "Ticket not found.";
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            var messages = await _context.TicketCommentsM
                .Where(tc => tc.TicketId == id && !tc.IsDelete)
                .OrderByDescending(tc => tc.CreatedOn)
                .Select(tc => new
                {
                    id = tc.Id,
                    content = tc.Comment,
                    sender = tc.CommentMobileUserId.HasValue ? "user" : "support",
                    timestamp = tc.CreatedOn,
                    ticketStatus = ticket.Status,
                    images = tc.Images != null ? JsonConvert.DeserializeObject<List<ImageResponseModel>>(tc.Images) : null

                })
                .ToListAsync();

            var response = new
            {
                id,
                messages,
                ticketStatus = ticket.Status
            };

            responseModel.Data = response;
            responseModel.Message = "Data fetched successfully.";
            responseModel.StatusCode = HttpStatusCode.OK;

            return responseModel;
        }

        private async Task<string> ConvertImagesToJsonAsync(IFormFileCollection images, List<string> aspectRatios)
        {
            List<object> imageList = new();
            if (images != null && images.Any())
            {
                for (int i = 0; i < images.Count; i++)
                {
                    var item = images[i];
                    string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(item.FileName)}";
                    var name = await SaveImageToAssetsFolderAsync(item, fileName);

                    var imageModel = new
                    {
                        Name = name,
                        AspectRatio = i < aspectRatios.Count ? aspectRatios[i] : ""
                    };
                    imageList.Add(imageModel);
                }
            }

            return JsonConvert.SerializeObject(imageList);
        }

        private async Task<string> SaveImageToAssetsFolderAsync(IFormFile ticketImage, string fileName)
        {
            string assetsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Ticket-Images");
            if (!Directory.Exists(assetsDirectory))
            {
                Directory.CreateDirectory(assetsDirectory);
            }

            string filePath = Path.Combine(assetsDirectory, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ticketImage.CopyToAsync(fileStream);
            }

            return fileName;
        }
    }
}