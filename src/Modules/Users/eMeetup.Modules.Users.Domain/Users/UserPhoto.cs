using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Users
{
    public class UserPhoto
    {
        private UserPhoto() { } // Private constructor for EF

        internal UserPhoto(Guid userId, string url, int displayOrder, bool isPrimary = false)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Url = url;
            DisplayOrder = displayOrder;
            IsPrimary = isPrimary;
            UploadedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Url { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsPrimary { get; private set; }
        public DateTime UploadedAt { get; private set; }

        public virtual User User { get; private set; }

        // Factory method
        public static Result<UserPhoto> Create(Guid userId, string url, int displayOrder, bool isPrimary = false)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(url))
                return Result.Failure<UserPhoto>(UserErrors.EmptyPhoto);

            if (displayOrder < 0)
                return Result.Failure<UserPhoto>(UserErrors.InvalidDisplayOrder);

            var photo = new UserPhoto(userId, url, displayOrder, isPrimary);
            return photo;
        }

        // Domain methods with Result return type
        public Result UpdateDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0)
                return Result.Failure(UserErrors.InvalidDisplayOrder);

            DisplayOrder = displayOrder;
            return Result.Success();
        }

        public void SetAsPrimary()
        {
            IsPrimary = true;
        }

        public void SetAsSecondary()
        {
            IsPrimary = false;
        }

        public Result UpdateUrl(string newUrl)
        {
            if (string.IsNullOrWhiteSpace(newUrl))
                return Result.Failure(UserErrors.EmptyPhoto);

            Url = newUrl;
            return Result.Success();
        }
    }
}
