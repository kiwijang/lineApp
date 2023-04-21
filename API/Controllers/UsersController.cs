using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using API.Data;
using API.Entities;
using API.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static API.Service.LoginService;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDbContextFactory<DataContext> _contextFactory;
        private readonly ILoginService _loginService;
        private readonly IJWTService _jwtService;

        public UsersController(IDbContextFactory<DataContext> contextFactory, ILoginService loginService, IJWTService jwtService)
        {
            _contextFactory = contextFactory;
            _loginService = loginService;
            _jwtService = jwtService;
        }

        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken([FromForm] OAuthCode auth)
        {
            var result = await this._loginService.getAccessTokenAsync(auth);
            if (result.IsSuccess)
            {
                string token = JsonSerializer.Serialize(result.Success);

                // 將 "jwt" 存到 Cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // 僅可通過 HTTP 訪問
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddDays(30), //同  Line 預設值 30 天
                };

                // 將 JWT Token 寫入 Cookie
                Response.Cookies.Append("jwt", token, cookieOptions);

                // 登入者加入 db
                var context = _contextFactory.CreateDbContext();
                var jwtObj_login = JsonSerializer.Deserialize<AccessTokenRes>(token);
                var tokenS = this._jwtService.ParseIDToken2Jwt(jwtObj_login.id_token);
                var sub = tokenS.Claims.First(claim => claim.Type == "sub").Value;
                var user = context.Users.Find(sub);
                if (user == null)
                {
                    var userName = tokenS.Claims.First(claim => claim.Type == "name").Value ?? "no name user";
                    AppUser u = new AppUser
                    {
                        UserName = userName,
                        Sub = sub,
                        isSubscribeNotify = false,

                    };
                    context.Add(u);
                    await context.SaveChangesAsync();
                }

                //result.Success
                return Redirect("http://localhost:3030/notify");
            }
            else
            {
                return Ok(result.Error);
            }
        }

        /// <summary>
        /// 驗證是否有 line login 授權
        /// </summary>
        /// <returns></returns>
        [HttpPost("VerifyLogin")]
        public async Task<ActionResult<bool>> VerifyLogin()
        {
            var jwt = HttpContext.Request.Cookies["jwt"];
            if (jwt == null)
            {
                return Unauthorized(false);
            }
            var jwtObj = JsonSerializer.Deserialize<AccessTokenRes>(jwt);

            bool valid = await this._loginService.verifyLoginSuccess(jwtObj);
            if (valid)
            {
                return Ok(true);
            }
            return BadRequest(false);
        }

        [HttpPost("NotifyMsg")]
        public async Task<IActionResult> NotifyMsg([FromForm] string message)
        {
            var jwt = HttpContext.Request.Cookies["notify"];
            var jwt_login = HttpContext.Request.Cookies["jwt"];
            if (jwt_login == null)
            {
                return Unauthorized();
            }

            var context = _contextFactory.CreateDbContext();
            var jwtObj = JsonSerializer.Deserialize<AccessTokenRes>(jwt);
            var jwtObj_login = JsonSerializer.Deserialize<AccessTokenRes>(jwt_login);
            var tokenS = this._jwtService.ParseIDToken2Jwt(jwtObj_login.id_token);
            var sub = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            NotifyData? data = null;
            string? db_token = null;

            if (jwt == null)
            {
                // cookie 沒有的話撈 DB
                data = await context.NotifyData.FirstOrDefaultAsync(x => x.AppUserSub == sub);
                if (data == null)
                {
                    return Unauthorized();
                }
                else
                {
                    db_token = DecryptStringFromBytes_Aes(Convert.FromBase64String(data.AccessTokenEncrypt));

                    // 不合規就刪掉
                    if (!(await this._loginService.checkNotifyStatus(db_token)))
                    {
                        context.NotifyData.Remove(data);
                        await context.SaveChangesAsync();

                        return Unauthorized();
                    }
                }
            }

            var token = jwtObj.access_token ?? db_token;

            bool valid = await this._loginService.notify(message, token);
            if (!valid)
            {
                return BadRequest();
            }

            // 儲存 使用者 sub 與訊息
            // 將資料存入 DB
            var result = context.Users.Find(sub);
            if (result == null)
            {
                var userName = tokenS.Claims.First(claim => claim.Type == "name").Value ?? "no name user";
                AppUser u = new AppUser
                {
                    UserName = userName,
                    Sub = sub,
                    isSubscribeNotify = false,

                };
                context.Add(u);

                NotifyHist h = new NotifyHist
                {
                    AuthorSub = u,
                    Content = message,
                    CreateTime = DateTime.Now,
                };
                context.Add(h);
            }
            else
            {
                NotifyHist h = new NotifyHist
                {
                    AuthorSub = result,
                    Content = message,
                    CreateTime = DateTime.Now,
                };
                context.Add(h);
            }

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("GetNotifyToken")]
        public async Task<IActionResult> GetNotifyToken([FromForm] OAuthCode auth)
        {
            // 要記錄是否有訂閱，防止重複訂閱 (notify 可重複訂閱且不會過期)
            var jwt = HttpContext.Request.Cookies["jwt"];
            if (jwt == null)
            {
                return Unauthorized(false);
            }
            var jwtObj = JsonSerializer.Deserialize<AccessTokenRes>(jwt);
            var tokenS = this._jwtService.ParseIDToken2Jwt(jwtObj.id_token);
            // 取得 sub
            var sub = tokenS.Claims.FirstOrDefault(claim => claim.Type == "sub").Value;

            var context = _contextFactory.CreateDbContext();
            var user = context.Users.Find(sub);
            // 已訂閱過了
            if (user != null && user.isSubscribeNotify)
            {
                return Ok();
            }

            var result = await this._loginService.getNotifyAccessTokenAsync(auth);
            if (result.IsSuccess)
            {
                // 將 notify access_token 存到 Cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // 僅可通過 HTTP 訪問
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddDays(30), // Line  notify 預設值不會過期，同 line login 30 天
                };

                // 寫入 Cookie
                Response.Cookies.Append("notify", result.access_token, cookieOptions);

                // 要記錄是否有訂閱，防止重複訂閱 (notify 可重複訂閱且不會過期)
                if (user != null)
                {
                    user.isSubscribeNotify = true;
                    context.Users.Update(user);
                }

                var code = EncryptStringToBytes_Aes(result.access_token);
                // 紀錄到 db
                context.NotifyData.Add(new NotifyData
                {
                    AccessTokenEncrypt = Convert.ToBase64String(code),
                    AppUserSub = sub,
                });

                await context.SaveChangesAsync();

                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// revoke line login
        /// </summary>
        /// <returns></returns>
        [HttpGet("RevokeLogin")]
        public async Task<ActionResult<bool>> RevokeLogin()
        {
            var jwt = HttpContext.Request.Cookies["jwt"];
            if (jwt == null)
            {
                return Unauthorized(false);
            }
            var jwtObj = JsonSerializer.Deserialize<AccessTokenRes>(jwt);

            bool valid = await this._loginService.revokeLineLogin(jwtObj.access_token);

            if (valid)
            {
                Response.Cookies.Delete("jwt");
                return Ok(true);
            }
            return BadRequest(false);
        }

        /// <summary>
        /// revoke line notify
        /// </summary>
        /// <returns></returns>
        [HttpGet("RevokeNotify")]
        public async Task<ActionResult<bool>> RevokeNotify()
        {
            var jwt_login = HttpContext.Request.Cookies["jwt"];
            if (jwt_login == null)
            {
                return Unauthorized(false);
            }
            var jwtObj_login = JsonSerializer.Deserialize<AccessTokenRes>(jwt_login);
            var tokenS = this._jwtService.ParseIDToken2Jwt(jwtObj_login.id_token);
            // 取得 sub
            var sub = tokenS.Claims.FirstOrDefault(claim => claim.Type == "sub").Value;

            var context = _contextFactory.CreateDbContext();
            var user = context.Users.Find(sub);
            // 更新狀態 沒訂閱
            user.isSubscribeNotify = false;
            context.Users.Update(user);
            // 刪除 NotifyData
            var data = await context.NotifyData.FirstOrDefaultAsync(x => x.AppUserSub == sub);
            if (data != null)
            {
                context.NotifyData.Remove(data);
            }

            await context.SaveChangesAsync();

            var jwt = HttpContext.Request.Cookies["notify"];
            if (jwt == null)
            {
                return Unauthorized(false);
            }
            var jwtObj = JsonSerializer.Deserialize<AccessTokenRes>(jwt);

            bool valid = await this._loginService.revokeLineNotify(jwtObj.access_token);

            if (valid)
            {
                Response.Cookies.Delete("notify");
                return Ok(true);
            }
            return BadRequest(false);
        }

        [HttpGet("GetNotifyHist")]
        public IEnumerable<NotifyHistViewModel> GetNotifyHist()
        {
            // 從 cookie 取得 line login jwt
            string jwt = HttpContext.Request.Cookies["jwt"];
            if (jwt == null)
            {
                return null;
            }
            var jwtObj = JsonSerializer.Deserialize<AccessTokenRes>(jwt);
            var tokenS = this._jwtService.ParseIDToken2Jwt(jwtObj.id_token);

            // 取得 sub
            var sub = tokenS.Claims.FirstOrDefault(claim => claim.Type == "sub").Value;

            var context = _contextFactory.CreateDbContext();

            return context.NotifyHist.Include(x => x.AuthorSub).Where(u => u.AuthorSub.Sub == sub).Select(h => new NotifyHistViewModel
            {
                Content = h.Content,
                CreateTime = h.CreateTime
            });
        }


        /// <summary>
        /// notify token 是否有效
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetNotifyStatus")]
        public async Task<ActionResult> GetNotifyStatus()
        {
            // 先從 cookie 找
            string jwt = HttpContext.Request.Cookies["notify"];
            var jwt_login = HttpContext.Request.Cookies["jwt"];
            if (jwt_login == null)
            {
                return Unauthorized("notLogin");
            }
            var jwtObj = JsonSerializer.Deserialize<AccessTokenRes>(jwt_login);

            JwtSecurityToken? tokenS = null;
            string? sub;
            DataContext? context;
            string? db_token = null;

            if (jwt == null)
            {
                // 沒有 cookie 就去 db 找
                tokenS = this._jwtService.ParseIDToken2Jwt(jwtObj.id_token);
                // 取得 sub
                sub = tokenS.Claims.FirstOrDefault(claim => claim.Type == "sub").Value;
                context = _contextFactory.CreateDbContext();
                NotifyData notifyData = await context.NotifyData.FirstOrDefaultAsync(x => x.AppUserSub == sub);
                // 已訂閱過了
                if (notifyData != null)
                {
                    db_token = DecryptStringFromBytes_Aes(Convert.FromBase64String(notifyData.AccessTokenEncrypt));
                }
                else
                {
                    return Unauthorized(false);
                }
            }

            if (await this._loginService.checkNotifyStatus(db_token ?? jwt))
            {
                return Ok(true);
            }

            context = _contextFactory.CreateDbContext();
            tokenS = this._jwtService.ParseIDToken2Jwt(jwtObj.id_token);
            sub = tokenS.Claims.First(claim => claim.Type == "sub").Value;
            AppUser user = context.Users.Find(sub);
            // 更新狀態 沒訂閱
            user.isSubscribeNotify = false;
            context.Users.Update(user);
            // 刪除 NotifyData
            var data = await context.NotifyData.FirstOrDefaultAsync(x => x.AppUserSub == sub);
            if (data != null)
            {
                context.NotifyData.Remove(data);
            }
            await context.SaveChangesAsync();

            return Unauthorized(false);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetUserViewModel>> GetUser(string id)
        {
            string jwt = HttpContext.Request.Cookies["jwt"];
            if (jwt == null)
            {
                return Unauthorized(null);
            }
            var context = _contextFactory.CreateDbContext();
            var r = await context.Users.Select(u => new
            {
                u.UserName,
                u.Sub,
                u.isSubscribeNotify,
            }).FirstOrDefaultAsync(u => u.Sub == id);
            return Ok(r);
        }

        public static byte[] EncryptStringToBytes_Aes(string plainText)
        {
            var key = Convert.FromBase64String("bunKH7tV2GZyspwY7ulWG2ERlavjRmUmiCsPrjaPUGw=");
            var IV = Convert.FromBase64String("fZAgskQojc72d1UOcfDJOg==");
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(IV));

            byte[] encrypted;

            using Aes aes = Aes.Create();

            aes.Key = key;
            aes.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            // Create the streams used for encryption.
            using MemoryStream msEncrypt = new MemoryStream();
            using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using StreamWriter swEncrypt = new StreamWriter(csEncrypt, leaveOpen: true);

            // Write all data to the crypto stream and flush it.
            swEncrypt.Write(plainText);
            swEncrypt.Flush();
            csEncrypt.FlushFinalBlock();

            // Get the encrypted bytes from the memory stream.
            encrypted = msEncrypt.ToArray();

            return encrypted;
        }

        public static string DecryptStringFromBytes_Aes(byte[] cipherText)
        {
            var key = Convert.FromBase64String("bunKH7tV2GZyspwY7ulWG2ERlavjRmUmiCsPrjaPUGw=");
            var IV = Convert.FromBase64String("fZAgskQojc72d1UOcfDJOg==");
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(IV));

            string plaintext = null;

            using Aes aes = Aes.Create();

            aes.Key = key;
            aes.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            // Create the streams used for decryption.
            using MemoryStream msDecrypt = new MemoryStream(cipherText);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt, leaveOpen: true);

            // Read the decrypted bytes from the decrypting stream
            // and place them in a string.
            plaintext = srDecrypt.ReadToEnd();

            return plaintext;
        }

        public class GetUserViewModel
        {
            public string UserName { get; set; }
            public string Sub { get; set; }
            public bool isSubscribeNotify { get; set; }
        }

        public class NotifyHistViewModel
        {
            public string Content { get; set; }
            public DateTime CreateTime { get; set; }
        }
    }
}