using RM.Database.ResearchMantraContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace RM.API.Data.EntityConfiguration
{
    public class PerformanceResultsEntityConfiguration : EntityTypeConfiguration<PerformanceResult>
    {
        public PerformanceResultsEntityConfiguration(EntityTypeBuilder<PerformanceResult> entity)
        {
            _ = entity.HasKey(e => e.PublicKey)
                    .HasName("PK__Performa__37A9983B025D5595");

            _ = entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");

            _ = entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(GETUTCDATE())");

            _ = entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);

            _ = entity.Property(e => e.Id).ValueGeneratedOnAdd();

            _ = entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

            _ = entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");

            _ = entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

            _ = entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        }
    }
}
