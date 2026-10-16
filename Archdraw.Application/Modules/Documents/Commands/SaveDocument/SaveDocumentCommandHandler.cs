using Archdraw.Application.Modules.Documents.Requests;
using Archdraw.Application.Modules.Documents.Responses;
using Archdraw.Application.Modules.Documents.UseCases.SaveDocument;
using Archdraw.Domain.Modules.Documents.Enums;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Documents.Commands.SaveDocument;

public class SaveDocumentCommandHandler : IRequestHandler<SaveDocumentCommand, ErrorOr<DocumentResponse>>
{
    private readonly SaveDocumentUseCase _saveDocumentUseCase;

    public SaveDocumentCommandHandler(SaveDocumentUseCase saveDocumentUseCase)
    {
        _saveDocumentUseCase = saveDocumentUseCase;
    }

    public async Task<ErrorOr<DocumentResponse>> Handle(SaveDocumentCommand command, CancellationToken cancellationToken)
        => await _saveDocumentUseCase.ExecuteAsync(command.UserId, command.Scope, command.OwnerId, command.Request, cancellationToken);
}
