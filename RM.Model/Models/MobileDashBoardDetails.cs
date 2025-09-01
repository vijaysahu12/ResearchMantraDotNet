using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.Models
{
    public class MobileDashBoardDetails
    {
        public string? MobileUserCount { get; set; }
        public string? ProductPurchaseCount { get; set; }
        public string? ClientWithHighestRevenue { get; set; }
        public string? ProductWithHighestRevenue { get; set; }
        public string? FreeTrialReport { get; set; }
        public string? PaymentGatewayRevenue { get; set; }
        public string? ProductPurchaseRevenue { get; set; }
        public string? CouponUsageReport { get; set; }
        public string? ProductRevenueGraphreport { get; set; }
        public string? ProductCategoreies { get; set; }
        public string? ProductLikeReport { get; set; }
        public string? UsersReoport { get; set; }
        public string? RevenueByCityReport { get; set; }
        public string? NumberOfNewUsersOfTheDayReport { get; set; }
        public string? TotalProductUserCountReport { get; set; }
    }
    public class PreMarketReportModel
    {
        public string Nifty { get; set; }
        public string Bnf { get; set; }
        public string Vix { get; set; }
        public string HotNews { get; set; }
        public string Diis { get; set; }
        public string Fiis { get; set; }
        public string Oi { get; set; }
        public string ButtonText { get; set; }
        public string Id { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class PostMarketResponseModel
    {
        public string Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public PerformerModel BestPerformer { get; set; }
        public PerformerModel WorstPerformer { get; set; }
        public List<VolumeShockerModel> VolumeShocker { get; set; }
    }

    public class PerformerModel
    {
        public string Name { get; set; }
        public double Close { get; set; }
        public double ChangePercentage { get; set; }
    }

    public class VolumeShockerModel
    {
        public string Name { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }
    }
}
