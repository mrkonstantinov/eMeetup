using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace eMeetup.Modules.Users.Domain.Photos;

public record FileUploadResult(string Url, string FileName, long Size);
