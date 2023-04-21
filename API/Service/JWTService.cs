
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

namespace API.Service
{
    public interface IJWTService
    {
        // string genLineMessageAPIJWT();
        JwtSecurityToken ParseIDToken2Jwt(string idToken);
    }

    // 安裝 jwt 套件
    // https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/
    // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet
    // dotnet add package System.IdentityModel.Tokens.Jwt --version 6.29.0
    public class JWTService : IJWTService
    {
        private readonly IConfiguration _configuration;

        public JWTService(IConfiguration configurationBuilder)
        {
            _configuration = configurationBuilder;
        }

        /// <summary>
        /// Parse LineLogin ID Token to JWT Data
        /// </summary>
        /// <param name="idToken"></param>
        /// <returns></returns>
        public JwtSecurityToken ParseIDToken2Jwt(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(idToken);

            return jwtSecurityToken;
        }

        // 這專案沒用到 請忽略這塊
        #region
        private static string privateKey = @"{
                ""alg"": ""RS256"",
                ""d"": ""BiEBEBau79yPPURGWovFWo3TAB3tTKGSI-Pq9o_Cq6FeTMmSAvSCzEW2ZtnLVF-J3DcvkVoEF0eX4pKvwbJmo2KIl-MRf_5odj16dwALYZtmbs5Y34CUfbkYG-FdnKxYRvVlmWZqm4xriuRae_wQB6DYDfFLB_mPNcc192pdAhjwfEt2qI4z_QF15tVjw4UygyBmt98ynaV-KaATnJK_puioCLn1NkcLaBojIJdshJIoD9HJptBaZIQCBYYbmn1ERaE1bf0OVDw6HlF1gAhsCrIQpCuXRsQ77urOD62GM3d6D9s1Mui-8S2B7rM81x-fub3LtYpvJ3f_o7-JgjCB-Q"",
                ""dp"": ""deLEKPm5VOwWy05rgwgpipKI_Y_BeWRCbMlx7jQ_ewhZ8UNF7iz6A6Q_OLrRAQfM8IAn8fMQ0pIb3I19iNRQED7BsUV_a0CzyxVEhMPYy7mc2cpSu0ZtrGBFPMKwR9PlWn2HLjcz9P-x4qzCGtHzh7hAqfcyYBw3UBJycByuLEU"",
                ""dq"": ""E4vpQ5hP8OigYZHIdrccqt30Wr1a4lVT37oUTc-oUVyXr4dkGMmfeVsA5lvoDnW7KFvwAMtBPnCy3AJ_axd1egttZGLjsRAVINXzIgbkQEyR3CN-SNgeAMXnYR31EdYhvqnUSMoVsFJAwhn6qf_b70h-xTpfBnDlr14XiWuOA1U"",
                ""e"": ""AQAB"",
                ""ext"": true,
                ""key_ops"": [
                    ""sign""
                ],
                ""kty"": ""RSA"",
                ""n"": ""tXKo2ahwDVs-KVIwsDV2QonpQfdaC_c1MQcqHW8aNx7DwZVyKow_O19hN5zMeiCdhjWoGozEJHXn8TesmJM604I5PvOutbYoEIHJin7nr6YUifvGzvY0SGQguWKYxQFY6Dh_n7GkDzLh0YW9LI0iC11eWhp0yN8k2S5PDpMUAHaNH6r3xTtJId8ADzqme6_PGzSXxwP_Yx3dbhikStVbcn_Q-JofKK8chvrrnU2WUQZTr8eLbGSuhA4uzRbETp-3mVUQ4LBSlsKDu2Ge_bTyx3yQ7FsFazSwWhzyuX8-n2wBchw6KnNLLxDl7HVemA3QLRtoqN8BgDqGaNknAGb2AQ"",
                ""p"": ""2lNPeziNfjZqEJqwirGMV-llmztOmC2_lDVDmhlYwJrKwpFjly5_BjOhUxnCZ_ZvoJofZclfR7_OVYibF1RZ4gbqYHw0EDwREkY2k2TocwOG-vsMXEn_zaJqE6IipDv3ZNwUT-JErxFCm9w9QcgnXd0IPc8OI1Yp3BZUJ0LpfI0"",
                ""q"": ""1MJBiqH5Kf0_qFXNbMj_iAqC5kOsJDv1gBqz2Iqy35j7M11lU-A1MFz0c1e7ZEpj1fQyCDeHqVietO28S0s8_fR_CRF6aCi9f6RUhkW9uQez1o9jYOpPNMK8_iS-TRfrdIX4tpZdC9wpLB9TYGkELDyh-zOVvWnq1l2T5hxK9EU"",
                ""qi"": ""uPjpITyczdzAV97YUte_qjSFST1VyfC8Fx90IDjArziPFrkK6xpnK3noHSijYYiCPHVoNLBpjtaRa3ipnQCzUbLQ0X8YZ7WUx1D_PQrd8bTc1h8_ErF7fSSglTn3rk3Ed6mS-5r73llgdkReUvJNzYfMNC0-qp_MQ2TMK9jyvds""
            }";

        /// <summary>
        /// client_credentials flow jwt (這個專案的前端網頁沒用到)
        /// https://developers.line.biz/en/reference/messaging-api/#issue-channel-access-token-v2-1
        /// </summary>
        /// <returns></returns>
        public string genLineMessageAPIJWT()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.UtcNow.AddMinutes(30));
            long unixTimeSeconds = dateTimeOffset.ToUnixTimeSeconds();

            // 輸入欲簽署的 payload 和 headers
            var payload = new
            {
                iss = _configuration.GetValue<string>("LineLogin:ClientID"),



                sub = _configuration.GetValue<string>("LineLogin:ClientID"),
                aud = "https =//api.line.me/",
                exp = unixTimeSeconds,
                token_exp = 60 * 60 * 24 * 30,
            };

            var headers = new
            {
                alg = "RS256",
                typ = "JWT",
            };

            // 將 payload 與 header 轉成 JSON 字串
            string payloadJson = JsonSerializer.Serialize(payload);
            string headersJson = JsonSerializer.Serialize(headers);

            Console.WriteLine(GenerateJwt(payloadJson, headersJson));

            return GenerateJwt(payloadJson, headersJson);
        }


        // 產生 JWT 字串的方法
        private string GenerateJwt(string payloadJson, string headersJson)
        {
            // 定義 JWT 的 header
            var headers = JwtHeader.Deserialize(headersJson);
            var payload = JwtPayload.Deserialize(payloadJson);

            // 建立 JWT 的 security token
            var token = new JwtSecurityToken(headers, payload);
            // 簽發 JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                // Add any claims here, if needed
            });
            // 解析 JWK 私鑰
            JsonWebKey jwk = JsonWebKey.Create(privateKey);
            // 在 header 加上 kid
            jwk.KeyId = _configuration.GetValue<string>("LineLogin:Kid");
            var credentials = new SigningCredentials(jwk, SecurityAlgorithms.RsaSha256);


            // Create a JWT security token
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration.GetValue<string>("LineLogin:ClientID"),
                Audience = "https =//api.line.me/",
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = credentials
            };

            var encodedToken = tokenHandler.CreateEncodedJwt(tokenDescriptor);

            return encodedToken;
        }


        // Base64Url 編碼
        static string Base64UrlEncode(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Base64UrlEncode(bytes);
        }

        // Base64Url 編碼
        static string Base64UrlEncode(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            var base64Url = base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
            return base64Url;
        }

        public static string Decrypt(string data, string privateKeyXml)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKeyXml);
            byte[] cipherData = Convert.FromBase64String(data);
            byte[] plainData = rsa.Decrypt(cipherData, false);
            return Encoding.UTF8.GetString(plainData);
        }

        public static string Encrypt(string data, string publicKeyXml)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKeyXml);
            byte[] plainData = Encoding.UTF8.GetBytes(data);
            byte[] cipherData = rsa.Encrypt(plainData, false);
            return Convert.ToBase64String(cipherData);
        }

        #endregion
    }
}