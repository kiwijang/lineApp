using System.Text.Json;
using API.Entities;
using static API.Service.LoginService;

namespace API.Service
{
    public interface ILoginService
    {
        Task<bool> checkNotifyStatus(string jwt_notify_cookie);
        Task<ApiResult> getAccessTokenAsync(OAuthCode code);
        Task<NotifyApiResult> getNotifyAccessTokenAsync(OAuthCode code);
        Task<bool> notify(string message, string access_token);
        Task<bool> revokeLineLogin(string access_token);
        Task<bool> revokeLineNotify(string access_token);
        Task<bool> verifyLoginSuccess(AccessTokenRes jwtObj);
    }

    public class LoginService : ILoginService
    {
        private IHttpClientFactory _clientFactory { get; set; }
        private readonly IConfiguration _configuration;

        public LoginService(IHttpClientFactory clientFactory, IConfiguration configurationBuilder)
        {
            _clientFactory = clientFactory;
            _configuration = configurationBuilder;
        }

        /// <summary>
        /// 取得 LineLogin access token 等資料，請參考 class AccessTokenRes
        /// </summary>
        /// <param name="code">LineLogin 登入認證成功回傳的 code 和 state</param>
        /// <returns>ApiResult</returns>
        public async Task<ApiResult> getAccessTokenAsync(OAuthCode code)
        {
            var accessTokenURL = _configuration.GetValue<string>("LineLogin:AccessTokenURL");

            var client = this._clientFactory.CreateClient();

            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "grant_type", "authorization_code" },
                            { "code", code.code },
                            { "client_id", _configuration.GetValue<string>("LineLogin:ClientID") },
                            { "redirect_uri", _configuration.GetValue<string>("LineLogin:CallbackURL") },
                            { "client_secret", _configuration.GetValue<string>("LineLogin:ClientSecret") },
                            { "id_token_key_type", _configuration.GetValue<string>("LineLogin:IdTokenKeyType") },
                        });

            using HttpResponseMessage response = await client.PostAsync(accessTokenURL, formData);
            var result = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                return new ApiResult
                {
                    IsSuccess = false,
                    Error = JsonSerializer.Deserialize<ErrorAccessTokenRes>(result)
                };
            }

            return new ApiResult
            {
                IsSuccess = true,
                Success = JsonSerializer.Deserialize<AccessTokenRes>(result)
            };
        }

        /// <summary>
        /// 驗證 LineLogin access_token 和 ID Token 是否真實存在
        /// </summary>
        /// <param name="jwtObj"></param>
        /// <returns>存在就回傳 true; 反之 false</returns>
        public async Task<bool> verifyLoginSuccess(AccessTokenRes jwtObj)
        {
            var client = this._clientFactory.CreateClient();

            // get
            var verifyAccessTokenUrl = $"https://api.line.me/oauth2/v2.1/verify?access_token={jwtObj.access_token}";
            using HttpResponseMessage response_access_token = await client.GetAsync(verifyAccessTokenUrl);

            // post
            var verifyIDTokenUrl = "https://api.line.me/oauth2/v2.1/verify";
            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "client_id", _configuration.GetValue<string>("LineLogin:ClientID") },

                            { "id_token", jwtObj.id_token },
                        });
            using HttpResponseMessage response_id_token = await client.PostAsync(verifyIDTokenUrl, formData);

            if (response_access_token.IsSuccessStatusCode && response_id_token.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 取得 Line Notify access_token
        /// https://notify-bot.line.me/doc/en/
        /// </summary>
        /// <param name="code">LineNotify 認證成功回傳的 code 和 state</param>
        /// <returns></returns>
        public async Task<NotifyApiResult> getNotifyAccessTokenAsync(OAuthCode code)
        {
            var accessTokenURL = _configuration.GetValue<string>("LineNotify:AccessTokenURL");

            var client = this._clientFactory.CreateClient();

            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "grant_type", "authorization_code" },
                            { "code", code.code },
                            { "client_id", _configuration.GetValue<string>("LineNotify:ClientID") },
                            { "redirect_uri", _configuration.GetValue<string>("LineNotify:CallbackURL") },
                            { "client_secret", _configuration.GetValue<string>("LineNotify:ClientSecret") },
                        });

            using HttpResponseMessage response = await client.PostAsync(accessTokenURL, formData);
            var result = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                return new NotifyApiResult
                {
                    IsSuccess = false,
                    access_token = result
                };
            }

            return new NotifyApiResult
            {
                IsSuccess = true,
                access_token = result
            };
        }

        /// <summary>
        /// 發送 Line Notify message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public async Task<bool> notify(string message, string access_token)
        {
            var client = this._clientFactory.CreateClient();

            // post
            var notifyUrl = "https://notify-api.line.me/api/notify";
            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "message", message },
                        });

            // Set a default request header
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {access_token}");

            using HttpResponseMessage response = await client.PostAsync(notifyUrl, formData);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> checkNotifyStatus(string jwt_notify_cookie)
        {
            var client = this._clientFactory.CreateClient();
            var jwtObj = JsonSerializer.Deserialize<AccessTokenRes>(jwt_notify_cookie);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtObj.access_token}");

            string statusURL = _configuration.GetValue<string>("LineNotify:AccessTokenStatus");
            using HttpResponseMessage response_access_token = await client.GetAsync(statusURL);

            if (response_access_token.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> revokeLineNotify(string access_token)
        {
            var client = this._clientFactory.CreateClient();

            // post
            var notifyUrl = "https://notify-api.line.me/api/revoke";

            // Set a default request header
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {access_token}");

            using HttpResponseMessage response = await client.PostAsync(notifyUrl, null);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> revokeLineLogin(string access_token)
        {
            var client = this._clientFactory.CreateClient();

            // post
            var notifyUrl = "https://api.line.me/oauth2/v2.1/revoke";
            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "access_token", access_token },
                            { "client_id", _configuration.GetValue<string>("LineLogin:ClientID") },
                            { "client_secret", _configuration.GetValue<string>("LineLogin:ClientSecret") },
                        });

            using HttpResponseMessage response = await client.PostAsync(notifyUrl, formData);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public class ApiResult
        {
            public bool IsSuccess { get; set; }
            public AccessTokenRes Success { get; set; }
            public ErrorAccessTokenRes Error { get; set; }
        }

        public class NotifyApiResult
        {
            public bool IsSuccess { get; set; }
            public string access_token { get; set; }
        }

        /// <summary>
        /// https://developers.line.biz/en/reference/line-login/#issue-token-response
        /// </summary>
        public class AccessTokenRes
        {
            public string access_token { get; set; }
            public string id_token { get; set; }
            public long expires_in { get; set; }
            public string refresh_token { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }
        }

        /// <summary>
        /// https://developers.line.biz/en/reference/line-login/#issue-token-response
        /// </summary>
        public class ErrorAccessTokenRes
        {
            public string error { get; set; }
            public string error_description { get; set; }
        }
    }
}