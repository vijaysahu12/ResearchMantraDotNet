using RM.Database.ResearchMantraContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace RM.API.Data.EntityConfiguration
{
    public class StocksEntityConfiguration : EntityTypeConfiguration<Stock>
    {
        public StocksEntityConfiguration(EntityTypeBuilder<Stock> entity)
        {
            _ = entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);

            _ = entity.Property(e => e.Id).ValueGeneratedOnAdd();



            _ = entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");

            _ = entity.Property(e => e.LotSize).HasDefaultValueSql("((1))");



            _ = entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

            _ = entity.Property(e => e.Symbol)
                .HasMaxLength(100)
                .IsUnicode(false);
        }
    }
}
