using AutoMapper.Internal;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public SettingsController(ResearchMantraContext context)
        {
            _context = context;
        }

        // Update the relevant part of the GetSettings method
        [HttpGet]
        public async Task<IActionResult> GetSettings(
                int pageNumber = 1,
                int pageSize = 10,
                string orderBy = null,
                string sortDirection = "asc",
                string searchText = null,
                string currentStatus = null)
        {
            var query = _context.Settings.ToList();

            // Apply search
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(s => s.Value.ToLower().Contains(searchText)
                                      || s.Code.ToLower().Contains(searchText)).ToList();
            }

            // Always order active first
            var query1 = query.Where(s => s.IsActive == false).OrderByDescending(s=> s.Id).ToList();
            var query2 = query.Where(s => s.IsActive == true).OrderByDescending(s => s.Id).ToList();

            // Apply secondary ordering
            if (!string.IsNullOrEmpty(orderBy))
            {
                switch (orderBy.ToLower())
                {
                    case "value":
                        query1 = sortDirection.ToLower() == "desc"
                            ? query.OrderByDescending(s => s.Value).ToList()
                            : query.OrderBy(s => s.Value).ToList();
                        break;
                    case "code":
                        query1 = sortDirection.ToLower() == "desc"
                            ? query.OrderByDescending(s => s.Code).ToList()
                            : query.OrderBy(s => s.Code).ToList();
                        break;
                    case "status":
                        // already handled by IsActive descending
                        break;
                    default:
                        query1 = query.OrderByDescending(s => s.Id).ToList();
                        break;
                }
            }
            else
            {
                query1 = query1.OrderByDescending(s => s.Id).ToList();
            }
            var query3 = query1;
            if (currentStatus == null || currentStatus == "")
            {
                query3 = query2.TryAdd(query1);
            }
            else
            {
                query3 = currentStatus == "active" ? query2 : query1;
            }

            var totalCount = query3.Count();

            var data = query3
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                StatusCode = HttpStatusCode.OK,
                Data = data,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }





        [HttpPost]
        public async Task<IActionResult> ManageSetting([FromBody] ManageSettingRequestModel request)
        {
            if (request == null)
            {
                return BadRequest(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid request",
                });
            }

            var setting = _context.Settings.FirstOrDefault(s => s.Id == request.Id);

            if (request.Id.HasValue && setting == null)
            {
                return NotFound(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Setting not found"
                });
            }

            if (setting != null)
            {
                setting.Value = request.Value;
                setting.Code = request.Code;
                setting.IsActive = request.IsActive;
            }
            else
            {
                setting = new Setting
                {
                    Value = request.Value,
                    Code = request.Code,
                    IsActive = request.IsActive
                };

                await _context.Settings.AddAsync(setting);
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = request.Id.HasValue ? "Setting updated successfully" : "Setting added successfully",
                Data = setting
            });
        }
    }
}