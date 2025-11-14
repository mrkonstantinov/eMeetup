using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

public sealed record UserResponse(Guid Id, string Email, string UserName, int? Gender, DateTime DateOfBirth, string? Bio, string? Interests, Location Location, string? ProfilePictureUrl);



public class PointJson
{
    public string Type { get; set; } = "Point";
    public double[] Coordinates { get; set; }

    //public PointJson(Point point)
    //{
    //    Coordinates = new double[] { point.X, point.Y };
    //}
}
