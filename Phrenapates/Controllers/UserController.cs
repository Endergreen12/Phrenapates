using Microsoft.AspNetCore.Mvc;
using Plana.Database;
using Phrenapates.Models;

namespace Phrenapates.Controllers
{
    [ApiController]
    [Route("/user")]
    public class UserController : ControllerBase
    {
        private readonly SCHALEContext context;

        public UserController(SCHALEContext _context)
        {
            context = _context;
        }

        [HttpGet("agreement")]
        public IResult Agreement([FromQuery] string version = "v0.0")
        {
            return Results.Json(new UserAgreementResponse()
            {
                Version = version,
                Data = []
            });
        }

        [HttpPost("create")]
        public IResult Create([FromForm] string deviceId)
        {
            UserCreateResponse rsp = new() { Result = 0, IsNew = 0 };
            var account = context.GuestAccounts.SingleOrDefault(x => x.DeviceId == deviceId);

            if (account is null)
            {
                account = new() { DeviceId = deviceId, Token = Guid.NewGuid().ToString() };
                context.GuestAccounts.Add(account);
                context.SaveChanges();

                rsp.IsNew = 1;
            }

            rsp.Uid = account.Uid;
            rsp.Token = account.Token;

            return Results.Json(rsp);
        }

        [HttpPost("login")]
        public IResult Login([FromForm] uint uid, [FromForm] string token, [FromForm] string storeId)
        {
            var account = context.GuestAccounts.SingleOrDefault(x => x.Uid == uid && x.Token == token);
            if (account is not null)
            {
                return Results.Json(new UserLoginResponse()
                {
                    AccessToken = account.Token,
                    Birth = null,
                    ChannelId = storeId,
                    CurrentTimestampMs = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    KrKmcStatus = 2,
                    Result = 0,
                    Transcode = "NULL",
                    Check7Until = 0,
                    Migrated = false,
                    ShowMigratePage = false
                });
            }

            return Results.Json(new
            {
                result = 1
            });
        }

        [HttpPost("destroy")]
        public IResult Destroy([FromForm] uint uid, [FromForm] string token)
        {
            var account = context.GuestAccounts.SingleOrDefault(x => x.Uid == uid && x.Token == token);
            context.Entry(account).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            context.SaveChanges();

            return Results.Json(new UserDestroyResponse()
            {
                CurrentTimestampMs = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                HasCoolDays = false,
                RebornBeforeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds(), // idk whats this, in dumped data, this value is lower than CurrentTimestampMs
                UserDestroyWaitDays = 0
            });
        }
    }
}
