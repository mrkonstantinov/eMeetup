using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eMeetup.Modules.Users.Domain.Users
{
    public class UserPhoto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }

        public virtual User User { get; set; }
    }
}
