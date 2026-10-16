using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.GetProfile;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Queries.GetProfile;

public record GetProfileQuery(Guid UserId) : IRequest<ErrorOr<GetProfileResponse>>;
