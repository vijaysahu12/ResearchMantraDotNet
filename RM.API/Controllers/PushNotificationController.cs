using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Hub;
using RM.API.Services;
using RM.CommonServices.Services;
using RM.Database.ResearchMantraContext;
using RM.Database.ResearchMantraContext.Tables;
using RM.Model.Common;
using RM.Model.RequestModel;
using RM.Model.RequestModel.Notification;
using RM.Model.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PushNotificationController : ControllerBase
    {
        private readonly ResearchMantraContext _context;
        private readonly IPushNotificationService _pushNotification;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly LandingPageService _landingPageService;

        public PushNotificationController(ResearchMantraContext context, IPushNotificationService pushNotification, IHubContext<NotificationHub> hubContext,LandingPageService landingPageService)
        {
            _hubContext = hubContext;
            _context = context;
            _pushNotification = pushNotification;
            _landingPageService = landingPageService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("notification")]
        public string Get(string message)
        {
            List<PushNotificationRequestModel> notifications = new()
                {
                    new PushNotificationRequestModel
                    {
                        Message =message,
                    }
                };

            _ = _hubContext.Clients.All.SendAsync("ReceiveToAll", "PR", "BB74D26F-AA28-EB11-BEE5-00155D53687A", notifications);
            return "Notification sent successfully to all users!";
        }

        [HttpPost]
        [Route("onetoonenotification")]
        public string SendNotification(ConnectedUser user, string message)
        {
            //notificationHub.Clients.Client(user.ConnId).ReceiveMessage("ReceiveMessage",user, message);
            return "Notification sent successfully to specific users!";
        }

        [HttpPost]
        [Route("broadcastnotification")]
        public string BroadNotification(ConnectedUser user, string message)
        {
            //notificationHub.Clients.AllExcept(user.ConnId).ReceiveMessage("ReceiveMessage", user, message);
            return "Notification sent successfully to specific users!";
        }

        // GET: api/Notification
        [HttpPost("FilterNotification")]
        public async Task<ActionResult<IEnumerable<PushNotification>>> GetFilteredNotification(QueryValues model)
        {
            TokenVariables tokenVariables = TokenAnalyserStatic.FetchTokenPart2(HttpContext);
            model.RequestedBy = tokenVariables.PublicKey; // userId
            Model.ApiCommonResponseModel response = await _pushNotification.GetPushNotification(model);
            return Ok(response);
        }

        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<PushNotification>>> GetUnReadNotification()
        {
            return await _context.PushNotifications.Where(x => x.IsRead == false).ToListAsync();

        }

        // GET: api/Notification/GetNotificationByUserId
        [HttpGet("GetByUserId/{userId}")]
        public async Task<ActionResult<IEnumerable<PushNotification>>> GetNotificationByUserId(string userId)
        {
            return await _context.PushNotifications.Where(x => x.Userkey.ToString() == userId).ToListAsync();
        }

        // GET: api/Notification/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PushNotification>> GetNotification(int id)
        {
            PushNotification Notification = await _context.PushNotifications.FirstOrDefaultAsync(c => c.Id == id);
            return Notification == null ? (ActionResult<PushNotification>)NotFound() : (ActionResult<PushNotification>)Notification;
        }

        // GET: api/Notification/markasread
        [HttpGet("markasread/{id}")]
        public async Task<ActionResult<PushNotification>> MarkAsReadNotification(int id)
        {


            PushNotification Notification = _context.PushNotifications.FirstOrDefault(c => c.Id == id);




            if (Notification == null)
            {
                return NotFound();
            }
            Notification.IsRead = true;
            Notification.ReadDate = DateTime.Now;
            Notification.ModifiedDate = DateTime.Now;

            _ = await _context.SaveChangesAsync();

            return Notification;


        }

        // PUT: api/Notification/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotification(int id, PushNotification Notification)
        {
            if (id != Notification.Id)
            {
                return BadRequest();
            }

            _context.Entry(Notification).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Notification

        [HttpPost]
        public async Task<ActionResult<PushNotification>> PostNotification(PushNotification notification)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            notification.CreatedBy = Guid.Parse(tokenVariables.PublicKey.ToString());
            Model.ApiCommonResponseModel response = await _pushNotification.PostPushNotification(notification, null);
            return Ok(response);
            //return CreatedAtAction("GetNotification", new { id = Notification.Id }, Notification);
        }

        // DELETE: api/Notification/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<PushNotification>> DeleteNotification(int id)
        {
            PushNotification Notification = await _context.PushNotifications.FirstOrDefaultAsync(c => c.Id == id);
            if (Notification == null)
            {
                return NotFound();
            }

            _ = _context.PushNotifications.Remove(Notification);
            _ = await _context.SaveChangesAsync();

            return Notification;
        }



        //Calling From Angular CRM to shoot the free notification to all valid Mobile Users.
        [HttpPost("SendNotificationViaCrm")]
        [AllowAnonymous]
        public async Task<IActionResult> SendNotificationViaCrm([FromForm] SendFreeNotificationRequestModel param)
        {
            var receiver = await _pushNotification.SendNotificationViaCrm(param);
            return Ok(receiver);
        }

        [HttpPost("ScheduleNotification")]
        [AllowAnonymous]
        public async Task<IActionResult> ManageScheduleNotification([FromForm] ScheduledNotificationRequestModel notification)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _pushNotification.ManageScheduleNotification(notification));
            }
            else
            {
                return Ok(new { StatusCode = HttpStatusCode.BadRequest, Message = "Send required data to continue." });
            }
        }


        private bool NotificationExists(int id)
        {
            return _context.PushNotifications.Any(e => e.Id == id);
        }

        /// This method reminds users via push notifications about products expiring soon in their bucket.

        [HttpPost("SendRemainderToUser")]
        public async Task<IActionResult> SendRemainderToUser(List<SendReminderToUserPushNotificationModel> param)
        {
            var response = await _pushNotification.SendRemainderToUser(param);
            return Ok(response);
        }

        [HttpGet("GetLandingPageList")]
        public IActionResult GetLandingPageList()
        {
            var response = _landingPageService.GetLandingPageList();
            return Ok(response);
        }

    }


}

