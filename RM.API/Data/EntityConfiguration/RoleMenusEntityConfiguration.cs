using RM.Database.ResearchMantraContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace RM.API.Data.EntityConfiguration
{
    public class RoleMenusEntityConfiguration : EntityTypeConfiguration<RoleMenu>
    {
        private readonly EntityTypeBuilder<RoleMenu> entityTypeBuilder;

        public RoleMenusEntityConfiguration(EntityTypeBuilder<RoleMenu> entityBuilder)
        {

            _ = entityBuilder.Property(e => e.CreatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

            _ = entityBuilder.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(GETUTCDATE())");

            _ = entityBuilder.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

            _ = entityBuilder.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");

            _ = entityBuilder.Property(e => e.MenuKey)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entityBuilder.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entityBuilder.Property(e => e.ModifiedOn).HasColumnType("datetime");

            _ = entityBuilder.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");

            _ = entityBuilder.Property(e => e.RoleKey)
                .HasMaxLength(100)
                .IsUnicode(false);
        }

    }
}
