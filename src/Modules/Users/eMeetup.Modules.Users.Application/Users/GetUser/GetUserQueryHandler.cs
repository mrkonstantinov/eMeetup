using System.Data;
using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

internal sealed class GetUserQueryHandler(IUserRepository userRepository, IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetUserQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        //var user = await userRepository.GetAsync(request.UserId, cancellationToken);
        //if (user is null)
        //{
        //    return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
        //}

        //UserResponse userResponse = new UserResponse(
        //    user.Id, user.Email, user.UserName, user.Gender, user.DateOfBirth, user.Bio, user.Interests, user.Location, user.ProfilePhotoUrl);


        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        string userSql = $"""
            SELECT
                u.id AS {nameof(UserResponse.Id)},
                u.email AS {nameof(UserResponse.Email)},
                u.user_name AS {nameof(UserResponse.UserName)},
                u.date_of_birth AS {nameof(UserResponse.DateOfBirth)},
                u.gender AS {nameof(UserResponse.Gender)},
                u.bio AS {nameof(UserResponse.Bio)},
                u.profile_picture_url AS {nameof(UserResponse.ProfilePictureUrl)},
                u.location_latitude AS {nameof(UserResponse.Latitude)},
                u.location_longitude AS {nameof(UserResponse.Longitude)},
                u.location_city AS {nameof(UserResponse.City)},
                u.location_country AS {nameof(UserResponse.Country)},
                u.created_at AS {nameof(UserResponse.CreatedAt)},
                u.updated_at AS {nameof(UserResponse.UpdatedAt)}
            FROM users.users u
            WHERE u.id = @UserId
            """;

        UserResponse? userResponse = await connection.QuerySingleOrDefaultAsync<UserResponse>(userSql, request);

        if (userResponse is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
        }

        string photosSql = $"""
            SELECT
                id AS {nameof(UserPhotoResponse.Id)},
                url AS {nameof(UserPhotoResponse.Url)},
                display_order AS {nameof(UserPhotoResponse.DisplayOrder)},
                is_primary AS {nameof(UserPhotoResponse.IsPrimary)}
            FROM users.user_photos
            WHERE user_id = @UserId
            ORDER BY display_order
            """;

        var photos = await connection.QueryAsync<UserPhotoResponse>(photosSql, new { UserId = request.UserId });

        var userWithPhotos = userResponse with { Photos = photos.AsList() };

        return userWithPhotos;
    }
}
