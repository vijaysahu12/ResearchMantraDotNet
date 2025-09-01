using RM.Database.KingResearchContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Entity.ModelConfiguration;

namespace RM.API.Data.EntityConfiguration
{
    public class CallPerformanceEntityConfiguration : EntityTypeConfiguration<CallPerformance>
    {
        public CallPerformanceEntityConfiguration(EntityTypeBuilder<CallPerformance> entity)
        {
            //entity.Property(e => e.ActualReturn).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.CallByKey)
                .HasMaxLength(50)
                .IsUnicode(false);

            _ = entity.Property(e => e.CallDate).HasColumnType("datetime");

            _ = entity.Property(e => e.CallStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("('Open')");

            _ = entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(GETUTCDATE())");

            //entity.Property(e => e.ExpectedReturn).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.ExpiryKey)
                .HasMaxLength(50)
                .IsUnicode(false);

            _ = entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");

            _ = entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");

            _ = entity.Property(e => e.IsIntraday)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("('Yes')");

            _ = entity.Property(e => e.IsPublic).HasDefaultValueSql("((0))");

            _ = entity.Property(e => e.LotSize).HasDefaultValueSql("((1))");

            _ = entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            _ = entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

            //entity.Property(e => e.NetResult).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");

            _ = entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .IsUnicode(false);

            _ = entity.Property(e => e.ResultHigh).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.ResultTypeKey)
                .HasMaxLength(50)
                .IsUnicode(false);

            _ = entity.Property(e => e.SegmentKey)
                .HasMaxLength(50)
                .IsUnicode(false);

            //entity.Property(e => e.CallServiceKey)
            //    .HasMaxLength(50)
            //    .IsUnicode(false);

            _ = entity.Property(e => e.StockKey)
                .HasMaxLength(50)
                .IsUnicode(false);

            _ = entity.Property(e => e.OptionValue)
              .HasMaxLength(100)
              .IsUnicode(false);

            _ = entity.Property(e => e.StopLossPrice).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.StrategyKey)
                .HasMaxLength(50)
                .IsUnicode(false);

            _ = entity.Property(e => e.ExitPrice).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.EntryPrice).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.Target1Price).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.Target2Price).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.Target3Price).HasColumnType("decimal(12, 2)");

            _ = entity.Property(e => e.TradeType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("('Buy')");
        }
    }
}
