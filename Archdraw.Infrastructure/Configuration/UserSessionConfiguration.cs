using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Archdraw.Infrastructure.Configuration;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.RefreshTokenHash).HasMaxLength(128).IsRequired();
        builder.Property(s => s.DeviceName).HasMaxLength(120).IsRequired();
        builder.Property(s => s.UserAgent).HasMaxLength(512);
        builder.Property(s => s.IpAddress).HasMaxLength(64);
        builder.Property(s => s.RevocationReason).HasMaxLength(200);
        builder.HasIndex(s => s.RefreshTokenHash).IsUnique();
        builder.HasIndex(s => new { s.UserId, s.FamilyId });
        builder.HasOne<AppUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
