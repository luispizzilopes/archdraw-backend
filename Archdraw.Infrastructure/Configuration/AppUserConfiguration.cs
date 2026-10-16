using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Archdraw.Infrastructure.Configuration;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.Name).HasMaxLength(120).IsRequired();
        builder.Property(u => u.ProfilePhotoKey).HasMaxLength(300);
        builder.Property(u => u.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.UpdatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
