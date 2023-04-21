// https://notify-bot.line.me/doc/en/
namespace API.Entities
{
    public class OAuthCode
    {
        /// <summary>
        /// 	A code for acquiring access tokens
        /// </summary>
        /// <value></value>
        public string code { get; set; }

        /// <summary>
        /// Directly sends the assigned state parameter
        /// </summary>
        /// <value></value>
        public string state { get; set; }
    }
}