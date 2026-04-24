using System.Reflection;
using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Events.Domain.TagGroups;
using Microsoft.AspNetCore.Hosting;

namespace eMeetup.Modules.Events.Application.EventTags.GetTagGroups;


public sealed record PictureResponse(byte[] Content, string ContentType, DateTime LastModified);

public sealed record GetItemPictureByIdQuery(int Id) : IQuery<PictureResponse>;

internal sealed class GetItemPictureByIdQueryHandler(
    ITagGroupRepository tagGroupRepository,
    IWebHostEnvironment environment)
    : IQueryHandler<GetItemPictureByIdQuery, PictureResponse>
{

    private static readonly string _picsPath;

    static GetItemPictureByIdQueryHandler()
    {
        // Get the directory where the class library DLL is located
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        _picsPath = Path.Combine(assemblyDirectory!, "Pics");
    }


    public async Task<Result<PictureResponse>> Handle(GetItemPictureByIdQuery request, CancellationToken cancellationToken)
    {
        // Get just the picture file name (more efficient than loading entire entity)
        var pictureFileName = await tagGroupRepository.GetPictureFileNameByIdAsync(request.Id, cancellationToken);

        if (string.IsNullOrEmpty(pictureFileName))
        {
            return Result.Failure<PictureResponse>(TagGroupErrors.NotFound(request.Id));
        }

        var path = GetFullPath(_picsPath, pictureFileName);
        //var path = GetFullPath(environment.ContentRootPath, pictureFileName);

        if (!File.Exists(path))
        {
            return Result.Failure<PictureResponse>(TagGroupErrors.PictureNotFound(pictureFileName));
        }

        string imageFileExtension = Path.GetExtension(pictureFileName);
        string contentType = GetImageMimeTypeFromImageFileExtension(imageFileExtension);
        DateTime lastModified = File.GetLastWriteTimeUtc(path);

        byte[] content = await File.ReadAllBytesAsync(path, cancellationToken);

        return new PictureResponse(content, contentType, lastModified);
    }

    private static string GetFullPath(string contentRootPath, string pictureFileName)
    {
        return Path.Combine(contentRootPath, pictureFileName);
    }

    private static string GetImageMimeTypeFromImageFileExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".bmp" => "image/bmp",
            ".tiff" => "image/tiff",
            ".wmf" => "image/wmf",
            ".jp2" => "image/jp2",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
