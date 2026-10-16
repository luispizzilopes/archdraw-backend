namespace Archdraw.Application.Modules.Auth.Models;

public sealed record FileUpload(Stream Content, string FileName, string ContentType, long Length);
