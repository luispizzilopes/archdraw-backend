using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Modules.Projects.Aggregates;

namespace Archdraw.Tests.Domain;

public class ProjectTests
{
    [Fact]
    public void Create_exige_proprietario_e_nome()
    {
        Assert.Throws<DomainException>(() => Project.Create(Guid.NewGuid(), Guid.Empty, "P", null, null, null));
        Assert.Throws<DomainException>(() => Project.Create(Guid.NewGuid(), Guid.NewGuid(), "  ", null, null, null));
        Assert.Throws<DomainException>(() => Project.Create(Guid.NewGuid(), Guid.NewGuid(), new string('x', 121), null, null, null));
    }

    [Fact]
    public void Estilos_sao_validados_deduplicados_e_podem_coexistir()
    {
        var p = Project.Create(Guid.NewGuid(), Guid.NewGuid(), "P", null, "modular-monolith", ["event-driven", "event-driven", "modular-monolith"]);
        Assert.Equal("modular-monolith", p.PrimaryStyleId);
        Assert.Equal(["event-driven"], p.SecondaryStyleIds);

        Assert.Throws<DomainException>(() => Project.Create(Guid.NewGuid(), Guid.NewGuid(), "P", null, "inexistente", null));
    }

    [Fact]
    public void Propriedade_e_verificada_pelo_dono()
    {
        var owner = Guid.NewGuid();
        var p = Project.Create(Guid.NewGuid(), owner, "P", null, null, null);
        Assert.True(p.IsOwnedBy(owner));
        Assert.False(p.IsOwnedBy(Guid.NewGuid()));
    }
}
