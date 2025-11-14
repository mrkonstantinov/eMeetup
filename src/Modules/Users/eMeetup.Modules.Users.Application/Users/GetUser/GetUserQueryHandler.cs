using System.Data;
using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
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

        const string sql =
            $"""
             SELECT
                 users.id AS {nameof(UserResponse.Id)},
                 users.email AS {nameof(UserResponse.Email)},
                 users.user_name AS {nameof(UserResponse.UserName)},
                 users.gender AS {nameof(UserResponse.Gender)},
                 users.date_of_birth AS {nameof(UserResponse.DateOfBirth)},
                 users.bio AS {nameof(UserResponse.Bio)},
                 users.interests AS {nameof(UserResponse.Interests)},
                 ST_AsText(users.location) AS {nameof(UserResponse.Location)},
                 
                 users.user_images.static_path AS {nameof(UserResponse.ProfilePictureUrl)}
             FROM users.users
             LEFT JOIN users.user_images ON users.users.id = users.user_images.user_id
             WHERE users.id = @UserId
             """;


        UserResponse? userResponse = await connection.QuerySingleOrDefaultAsync<UserResponse>(sql, request);

        if (userResponse is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
        }

        return userResponse;
    }
}
