using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    public class AppUser
    {
        [Key]
        public string Sub { get; set; }
        public string UserName { get; set; }

        public bool isSubscribeNotify { get; set; }

        [InverseProperty("AuthorSub")]
        public virtual List<NotifyHist> AuthoredNotify { get; set; }

        public virtual NotifyData NotifyData { get; set; }
    }
}

// dotnet ef migrations add init
// dotnet ef database update