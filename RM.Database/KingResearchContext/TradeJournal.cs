using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Database.KingResearchContext
{
    public class TradeJournal
    {
        public int? Id { get; set; }
        public DateTime StartDate { get; set; }
        public Guid MobileUserKey { get; set; }
        public string Symbol { get; set; }
        public bool BuySellButton { get; set; } // true = Buy, false = Sell

        [Column(TypeName = "decimal(18,2)")]
        public decimal CapitalAmount { get; set; }
        public decimal RiskPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RiskAmount { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal StopLoss { get; set; }
        public decimal? Target1 { get; set; }
        public decimal? Target2 { get; set; }
        public decimal? PositionSize { get; set; }
        public decimal? ActualExitPrice { get; set; }

        public string? ProfitLoss { get; set; }
        public string? RiskReward { get; set; }
        public string? Notes { get; set; }

        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class TradeJournalDto
    {
        public int? Id { get; set; }
        public string StartDate { get; set; }
        public Guid MobileUserKey { get; set; }
        public string Symbol { get; set; }
        public bool BuySellButton { get; set; } // true = Buy, false = Sell
        public decimal CapitalAmount { get; set; }
        public decimal RiskPercentage { get; set; }
        public decimal RiskAmount { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal StopLoss { get; set; }

        public decimal? Target1 { get; set; }
        public decimal? Target2 { get; set; }
        public decimal? PositionSize { get; set; }
        public decimal? ActualExitPrice { get; set; }
        public string? ProfitLoss { get; set; }
        public string? RiskReward { get; set; }
        public string? Notes { get; set; }

        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
