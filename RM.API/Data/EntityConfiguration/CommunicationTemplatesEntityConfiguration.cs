using RM.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace RM.API.Data.EntityConfiguration
{
    public class CommunicationTemplatesEntityConfiguration : EntityTypeConfiguration<CommunicationTemplates>
    {
        public CommunicationTemplatesEntityConfiguration(EntityTypeBuilder<CommunicationTemplates> entity)
        {
            _ = entity.Property(e => e.CreatedBy)
                     .HasMaxLength(100)
                     .IsUnicode(false);

            _ = entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(GETUTCDATE())");

            _ = entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

            _ = entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");

            _ = entity.Property(e => e.ModeKey)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

            _ = entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        }
    }
}
