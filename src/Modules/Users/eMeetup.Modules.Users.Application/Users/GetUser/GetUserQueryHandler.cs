using System.Data;
using System.Data.Common;
using Dapper;
using eMeetup.Common.Application.Data;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Users.Application.Abstractions.Identity;
using eMeetup.Modules.Users.Domain.Interfaces.Repositories;
using eMeetup.Modules.Users.Domain.Users;

namespace eMeetup.Modules.Users.Application.Users.GetUser;

internal sealed class GetUserQueryHandler(IDbConnectionFactory dbConnectionFactory, IIdentityProviderService identityProviderService)
    : IQueryHandler<GetUserQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var t = identityProviderService.GetUserAsync(request.IdentityId).Result;

        // Use PostgreSQL JSON features for optimal data retrieval
        var user = await GetUserWithDetailsPostgresAsync(connection, request.UserId);
        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId.ToString()));
        }

        return Result.Success(user);
    }

    private async Task<UserResponse?> GetUserWithDetailsPostgresAsync(IDbConnection connection, Guid userId)
    {
        const string userSql = @"
        SELECT 
            u.id AS Id,
            u.email AS Email,
            u.user_name AS UserName,
            u.date_of_birth AS DateOfBirth,
            u.gender AS Gender,
            u.bio AS Bio,
            u.profile_picture_url AS ProfilePictureUrl,
            u.location_latitude AS Latitude,
            u.location_longitude AS Longitude,
            u.location_city AS City,
            u.location_country AS Country,
            u.created_at AS CreatedAt,
            u.updated_at AS UpdatedAt
        FROM users.users u
        WHERE u.id = @userId;
    ";

        const string photosSql = @"
        SELECT 
            id AS Id,
            url AS Url,
            display_order AS DisplayOrder,
            is_primary AS IsPrimary
        FROM users.user_photos
        WHERE user_id = @userId
        ORDER BY display_order;
    ";

        const string interestsSql = @"
        SELECT string_agg(t.name, ', ' ORDER BY t.name) AS Interests
        FROM users.user_interests ui
        INNER JOIN users.tags t ON t.id = ui.tag_id
        WHERE ui.user_id = @userId
        GROUP BY ui.user_id;
    ";

        // Получаем данные отдельными запросами
        using var multi = await connection.QueryMultipleAsync(
            $"{userSql}; {photosSql}; {interestsSql}",
            new { userId }
        );

        var user = await multi.ReadFirstOrDefaultAsync<UserResponse>();
        if (user == null) return null;

        var photos = await multi.ReadAsync<UserPhotoResponse>();
        var interests = await multi.ReadFirstOrDefaultAsync<string>();

        user.Photos = photos.AsList();
        user.Interests = interests;

        return user;
    }

}
