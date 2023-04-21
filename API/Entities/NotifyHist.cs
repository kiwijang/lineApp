using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class NotifyHist
    {
        [Key]
        public long ContentId { get; set; }

        public DateTime CreateTime { get; set; }

        public string Content { get; set; }

        public virtual AppUser AuthorSub { get; set; }
    }
}