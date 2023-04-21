using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class NotifyData
    {
        [Key]
        public long DataId { get; set; }
        public string AccessTokenEncrypt { get; set; }

        public string AppUserSub { get; set; } // Add foreign key property
        public virtual AppUser UserSub { get; set; }
    }
}