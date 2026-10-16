using Archdraw.Application.Behaviors;
using Archdraw.Application.Common;
using Archdraw.Domain.Exceptions;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Archdraw.Tests.Application;

public class ApplicationBehaviorTests
{
    private sealed record Ping(string Name) : IRequest<ErrorOr<string>>;

    private sealed class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator() => RuleFor(x => x.Name).NotEmpty().WithMessage("Nome obrigatório.");
    }

    [Fact]
    public async Task Validation_behavior_bloqueia_requisicoes_invalidas_antes_do_handler()
    {
        var behavior = new ValidationBehavior<Ping, ErrorOr<string>>(new PingValidator());
        var called = false;
        var result = await behavior.Handle(new Ping(""), _ => { called = true; return Task.FromResult<ErrorOr<string>>("ok"); }, default);

        Assert.False(called);
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal("Nome obrigatório.", result.FirstError.Description);
    }

    [Fact]
    public async Task Domain_exception_vira_erro_de_validacao_e_concorrencia_vira_conflito()
    {
        var behavior = new DomainExceptionBehavior<Ping, ErrorOr<string>>();

        var invariant = await behavior.Handle(new Ping("x"), _ => throw new DomainException("Regra violada."), default);
        Assert.Equal(ErrorType.Validation, invariant.FirstError.Type);
        Assert.Equal("Regra violada.", invariant.FirstError.Description);

        var concurrency = await behavior.Handle(new Ping("x"), _ => throw new ConcurrencyConflictException("c", new Exception()), default);
        Assert.Equal(ErrorType.Conflict, concurrency.FirstError.Type);
    }

    [Fact]
    public void Guid_deterministico_e_estavel_por_namespace_e_nome()
    {
        var ns = Guid.NewGuid();
        Assert.Equal(DeterministicGuid.Create(ns, "project:a"), DeterministicGuid.Create(ns, "project:a"));
        Assert.NotEqual(DeterministicGuid.Create(ns, "project:a"), DeterministicGuid.Create(ns, "project:b"));
        Assert.NotEqual(DeterministicGuid.Create(ns, "project:a"), DeterministicGuid.Create(Guid.NewGuid(), "project:a"));
    }

    [Fact]
    public void Guid_deterministico_segue_o_vetor_conhecido_do_uuid_v5()
    {
        var dns = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
        Assert.Equal(Guid.Parse("2ed6657d-e927-568b-95e1-2665a8aea6a2"), DeterministicGuid.Create(dns, "www.example.com"));
    }
}
