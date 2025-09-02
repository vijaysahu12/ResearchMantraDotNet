using RM.Database.ResearchMantraContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;
namespace RM.API.Data.EntityConfiguration
{
    public class MenusEntityConfiguration : EntityTypeConfiguration<Menu>
    {
        public MenusEntityConfiguration(EntityTypeBuilder<Menu> entityBuilder)
        {

            _ = entityBuilder.Property(e => e.CreatedBy)
                   .HasMaxLength(100)
                   .IsUnicode(false);

            _ = entityBuilder.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(GETUTCDATE())");

            _ = entityBuilder.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);

            _ = entityBuilder.Property(e => e.IsLhs).HasDefaultValueSql("((0))");

            _ = entityBuilder.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

            _ = entityBuilder.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");

            _ = entityBuilder.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entityBuilder.Property(e => e.ModifiedOn).HasColumnType("datetime");

            _ = entityBuilder.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entityBuilder.Property(e => e.ParentId).HasDefaultValueSql("((0))");

            _ = entityBuilder.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");

            _ = entityBuilder.Property(e => e.SortOrder).HasDefaultValueSql("((100))");

            _ = entityBuilder.Property(e => e.Url)
                .HasMaxLength(250)
                .IsUnicode(false);


        }
    }
}
