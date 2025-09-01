using RM.Database.KingResearchContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace RM.API.Data.EntityConfiguration
{
    public class PincodeEntityConfiguration : EntityTypeConfiguration<Pincode>
    {
        public PincodeEntityConfiguration(EntityTypeBuilder<Pincode> entityBuilder)
        {
            _ = entityBuilder.Property(e => e.Area).HasMaxLength(150);

            _ = entityBuilder.Property(e => e.CreatedBy).HasMaxLength(100);

            _ = entityBuilder.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(GETUTCDATE())");

            _ = entityBuilder.Property(e => e.District).HasMaxLength(150);

            _ = entityBuilder.Property(e => e.Division).HasMaxLength(150);

            _ = entityBuilder.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

            _ = entityBuilder.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");

            _ = entityBuilder.Property(e => e.ModifiedBy).HasMaxLength(100);

            _ = entityBuilder.Property(e => e.ModifiedOn).HasColumnType("datetime");

            _ = entityBuilder.Property(e => e.Pincode1)
                .IsRequired()
                .HasMaxLength(50);

            _ = entityBuilder.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");

            _ = entityBuilder.Property(e => e.State).HasMaxLength(150);
        }
    }
}
