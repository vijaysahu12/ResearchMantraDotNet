using RM.Database.KingResearchContext.Tables;
using RM.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace RM.Database.KingResearchContext;

public partial class KingResearchContext : DbContext
{
    public KingResearchContext()
    {
    }

    public KingResearchContext(DbContextOptions<KingResearchContext> options)
        : base(options)
    {
    }


    public virtual DbSet<LeadActivity> LeadActivity { get; set; }
   // public virtual DbSet<ActivityType> ActivityTypes { get; set; }
    public virtual DbSet<AdvertisementImageM> AdvertisementImageM { get; set; }
    public virtual DbSet<Baskets> BasketsM { get; set; }
    public virtual DbSet<CallPerformance> CallPerformances { get; set; }

    public virtual DbSet<CommunicationMode> CommunicationModes { get; set; }
    public virtual DbSet<Complaints> Complaints { get; set; }
    public virtual DbSet<CompanyTypeM> CompanyTypeM { get; set; }
    public virtual DbSet<CompanyDetailM> CompanyDetailM { get; set; }
    public virtual DbSet<CompanyDetailMessageM> CompanyDetailMessageM { get; set; }
    public virtual DbSet<CodelineContactInfo> CodelineContactInfo { get; set; }
    public virtual DbSet<PromotionM> PromotionM { get; set; }

    //public virtual DbSet<BlogComments> BlogComments { get; set; }
    public virtual DbSet<CommunicationTemplate> CommunicationTemplates { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerKyc> CustomerKycs { get; set; }
    public virtual DbSet<KingResearchCouponsM> KingResearchCouponsM { get; set; }
    public virtual DbSet<CouponsM> CouponsM { get; set; }
    public virtual DbSet<CouponUserMappingM> CouponUserMappingM { get; set; }
    public virtual DbSet<CouponProductMappingM> CouponProductMappingM { get; set; }

    public virtual DbSet<CustomerService> CustomerServices { get; set; }

    public virtual DbSet<CustomerTag> CustomerTags { get; set; }

    public virtual DbSet<CustomerType> CustomerTypes { get; set; }
    public virtual DbSet<DeleteStatementM> DeleteStatementM { get; set; }

    public virtual DbSet<Enquiry> Enquiries { get; set; }

    public virtual DbSet<Expiry> Expirys { get; set; }

    public virtual DbSet<Lead> Leads { get; set; }
    public virtual DbSet<LeadsPullLimit> LeadPullLimit { get; set; }

    public virtual DbSet<LeadSource> LeadSources { get; set; }

    public virtual DbSet<LeadType> LeadTypes { get; set; }
    public virtual DbSet<LearningMaterialM> LearningMaterialM { get; set; }
    //public virtual DbSet<LearningProductM> LearningProductM { get; set; }
    public virtual DbSet<LearningContentExampleM> LearningContentExampleM { get; set; }
    public virtual DbSet<LearningContentM> LearningContentM { get; set; }

    public virtual DbSet<LeadsShadow> LeadsShadows { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MobileUser> MobileUsers { get; set; }

    public virtual DbSet<LastTenYearSalesM> LastTenYearSalesM { get; set; }
    public virtual DbSet<LearningContentLikesM> LearningContentLikesM { get; set; }
    public virtual DbSet<LastOneYearMonthlyPriceM> LastOneYearMonthlyPriceM { get; set; }
    //public virtual DbSet<PromotersHoldingInPercentM> PromotersInPercentM { get; set; }
    public virtual DbSet<LikeTypesM> LikeTypesM { get; set; }
    public virtual DbSet<LeadsImport> LeadsImport { get; set; }

    public virtual DbSet<PartnerAccount> PartnerAccounts { get; set; }
    public virtual DbSet<PartnerAccountDetails> PartnerAccountDetails { get; set; }

    public virtual DbSet<PartnerAccountActivity> PartnerAccountActivities { get; set; }

    public virtual DbSet<PartnerComment> PartnerComments { get; set; }


    public virtual DbSet<PaymentMode> PaymentModes { get; set; }
    public virtual DbSet<PaymentRequestStatusM> PaymentRequestStatusM { get; set; }

    public virtual DbSet<PerformanceResult> PerformanceResults { get; set; }

    public virtual DbSet<Pincode> Pincodes { get; set; }
    public virtual DbSet<PartnerAccountsM> PartnerAccountsM { get; set; }
    public virtual DbSet<ProductLikesM> ProductLikesM { get; set; }
    public virtual DbSet<ProductsRatingM> ProductsRatingM { get; set; }
    public virtual DbSet<PushNotificationsM> PushNotificationsM { get; set; }
    public virtual DbSet<ScheduledNotification> ScheduledNotificationM { get; set; }
    public virtual DbSet<PaymentResponseStatusM> PaymentResponseStatusM { get; set; }
    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public virtual DbSet<PreMarketData> PreMarketData { get; set; }
    public virtual DbSet<PartnerNamesM> PartnerNamesM { get; set; }
    public virtual DbSet<PurchaseOrderM> PurchaseOrdersM { get; set; }
    public virtual DbSet<ProductsM> ProductsM { get; set; }
    public virtual DbSet<ProductsContentM> ProductsContentM { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleMenu> RoleMenus { get; set; }

    public virtual DbSet<Segment> Segments { get; set; }
    public virtual DbSet<Setting> Settings { get; set; }
    public virtual DbSet<ScannerPerformanceM> ScannerPerformanceM { get; set; }
    public virtual DbSet<Subscriptions.SubscriptionDurationM> SubscriptionDurationM { get; set; }
    public virtual DbSet<Subscriptions.SubscriptionMappingM> SubscriptionMappingM { get; set; }
    public virtual DbSet<Subscriptions.SubscriptionPlanM> SubscriptionPlanM { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<Status> Status { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<StockImports> StockImport { get; set; }

    public virtual DbSet<Contracts> Contracts { get; set; }

    public virtual DbSet<Strategy> Strategies { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<TicketCommentsM> TicketCommentsM { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserActivity> UserActivity { get; set; }
    public virtual DbSet<UserMappings> UserMappings { get; set; }
    public virtual DbSet<UserRefreshTokenM> UserRefreshTokenM { get; set; }
    public virtual DbSet<UserType> UserTypes { get; set; }
    public virtual DbSet<WebSocketConnection> WebSocketConnections { get; set; }

    public virtual DbSet<XlPartner> XlPartners { get; set; }
    public virtual DbSet<WatiSentMessageLog> WatiSentMessageLogs { get; set; }
    public virtual DbSet<WatiTemplate> WatiTemplates { get; set; }

    public virtual DbSet<XlStockList> XlStockLists { get; set; }

    public virtual DbSet<ExceptionLog> ExceptionLogs { get; set; }

    public virtual DbSet<FollowUpReminder> FollowUpReminders { get; set; }
    public virtual DbSet<PushNotification> PushNotifications { get; set; }

    public virtual DbSet<InstaMojo> InstaMojos { get; set; }
    public virtual DbSet<InstaMojosMarketManthan> InstaMojosMarketManthan { get; set; }
    public virtual DbSet<OctEventRegistration> OctEventRegistration { get; set; }
    //public virtual DbSet<Log> Logs { get; set; }
    public virtual DbSet<ProductCategoriesM> ProductCategoriesM { get; set; }
    //public virtual DbSet<CommunityPost> CommunityPosts { get; set; }
    //public virtual DbSet<CommunityPostReply> CommunityPostReplies { get; set; }
    //public virtual DbSet<PostReactions> PostReaction { get; set; }
    public virtual DbSet<Groups> Groups { get; set; }
    public virtual DbSet<MyBucketM> MyBucketM { get; set; }
    public virtual DbSet<ProductCommunityMapping> ProductCommunityMappingM { get; set; }
    public virtual DbSet<TicketM> TicketM { get; set; }
    public virtual DbSet<ScreenerTables.ScreenerM> ScreenerM { get; set; }
    public virtual DbSet<ScreenerTables.ScreenerCategoryM> ScreenerCategoryM { get; set; }
    public virtual DbSet<ScreenerTables.ScreenerStockDataM> ScreenerStockDataM { get; set; }
    public virtual DbSet<ReasonChangeLog> ReasonChangeLogs { get; set; }
    public virtual DbSet<ReasonChangePurchase> ReasonChangePurchase { get; set; }
    public virtual DbSet<LeadFreeTrial> LeadFreeTrials { get; set; }
    public virtual DbSet<DashboardServiceM> DashboardServiceM { get; set; }

    public virtual DbSet<ProductBonusMappingM> ProductBonusMappingM { get; set; }
    //public virtual DbSet<SchedulerTask> SchedulerTaskM { get; set; }
    public virtual DbSet<LeadFreeTrailReasonLog> LeadFreeTrailReasonLog { get; set; }

    public virtual DbSet<Chapter> Chapter { get; set; }
    public virtual DbSet<SubChapter> SubChapter { get; set; }
    public virtual DbSet<QueryFormM> QueryFormM { get; set; }
    public virtual DbSet<QueryFormRemarks> QueryFormRemarks { get; set; }
    public virtual DbSet<TradeJournal> TradeJounal { get; set; }
    public virtual DbSet<RpfForm> RpfForms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CallPerformance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CallPerf__3214EC07DD757399");

            entity.ToTable("CallPerformance");

            //entity.Property(e => e.ActualReturn).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CallByKey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CallDate).HasColumnType("datetime");
            //entity.Property(e => e.CallServiceKey)
            //    .HasMaxLength(50)
            //    .IsUnicode(false);
            entity.Property(e => e.CallStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("('Open')");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EntryPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ExitPrice).HasColumnType("decimal(12, 2)");
            //entity.Property(e => e.ExpectedReturn).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ExpiryKey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsIntraday)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("('Yes')");
            entity.Property(e => e.IsPublic).HasDefaultValueSql("((0))");
            entity.Property(e => e.LotSize).HasDefaultValueSql("((1))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            //entity.Property(e => e.NetResult).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.OptionValue)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ResultHigh).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ResultTypeKey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SegmentKey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StockKey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StopLossPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.StrategyKey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Target1Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Target2Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Target3Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TradeType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("('Buy')");
        });

        modelBuilder.Entity<CommunicationMode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Communic__3214EC07D2B47B5D");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<CommunicationTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Communic__3214EC0754ED3745");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModeKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC075EF347D8");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PurchaseOrderKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.LeadKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Remarks)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.TotalPurchases).HasColumnType("decimal(12, 2)");
            //entity.Property(e => e.PurchaseOrderKey);
        });

        modelBuilder.Entity<CustomerKyc>(entity =>
        {
            entity.ToTable("CustomerKYC");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.JsonData).IsRequired();
            entity.Property(e => e.LeadKey)
                .IsRequired()
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Pan)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("PAN");
            entity.Property(e => e.Panurl)
                .HasMaxLength(50)
                .HasColumnName("PANURL");
            entity.Property(e => e.Remarks).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CustomerService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC0780ED3532");

            entity.Property(e => e.ActualCost)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.AmountOutstanding)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.AmountPaid)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Discount)
                .HasDefaultValueSql("((0))")
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.OrderId)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.PaymentModeKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ServiceKey)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CustomerTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC079C2ABFF9");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerKey).HasMaxLength(100);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.TagKey).HasMaxLength(100);
        });

        modelBuilder.Entity<CustomerType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC077A392DF2");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<Enquiry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Enquirie__3214EC073A7A21E8");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Details)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.IsAdmin).HasDefaultValueSql("((1))");
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsLead).HasDefaultValueSql("((1))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ReferenceKey)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Expiry>(entity =>
        {
            entity.HasKey(e => e.PublicKey).HasName("PK__Expirys__37A9983B2713FDA0");

            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Leads__3214EC07DF7B12F4");

            entity.Property(e => e.AssignedTo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("('BB74D26F-AA28-EB11-BEE5-00155D53687A')");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EmailId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsSpam).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsWon).HasDefaultValueSql("((0))");
            entity.Property(e => e.LeadSourceKey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("('EB82ED26-6708-EB11-BFA7-00155D53687A')");
            entity.Property(e => e.LeadTypeKey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("('4C98BD0B-6F08-EB11-BFA7-00155D53687A')");
            entity.Property(e => e.MobileNumber)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.PinCode)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PriorityStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Remarks)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.ServiceKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PurchaseOrderKey);
        });

        modelBuilder.Entity<LeadSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadSour__3214EC075449608A");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<LeadType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeadType__3214EC075A042400");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<LeadsShadow>(entity =>
        {
            entity.HasKey(e => e.AuditId);

            entity.ToTable("Leads_shadow");

            entity.Property(e => e.AssignedTo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AuditAction)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.AuditApp)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasDefaultValueSql("(('App=('+rtrim(isnull(app_name(),'')))+') ')");
            entity.Property(e => e.AuditDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.AuditUser)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.EmailId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.LeadSourceKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LeadTypeKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Remarks)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.ServiceKey)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Menus__3214EC0779A678BB");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsLhs).HasColumnName("IsLHS");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ParentId).HasDefaultValueSql("((0))");
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.SortOrder).HasDefaultValueSql("((100))");
            entity.Property(e => e.Url)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MobileUser>(entity =>
        {
            entity.Property(e => e.About)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DeviceType)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Imei)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("IMEI");
            entity.Property(e => e.MobileToken)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.OneTimePassword)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.StockNature)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PartnerAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartnerA__3214EC0705BC839F");

            //entity.Property(e => e.AliceBlueCid)
            //    .HasMaxLength(100)
            //    .IsUnicode(false)
            //    .HasColumnName("AliceBlueCId");
            //entity.Property(e => e.AngelCid)
            //    .HasMaxLength(100)
            //    .IsUnicode(false)
            //    .HasColumnName("AngelCId");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedIpAddress)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            //entity.Property(e => e.EdelweissCid)
            //    .HasMaxLength(100)
            //    .IsUnicode(false)
            //    .HasColumnName("EdelweissCId");
            entity.Property(e => e.EmailId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .IsUnicode(false);
            //entity.Property(e => e.FyersCid)
            //    .HasMaxLength(100)
            //    .IsUnicode(false)
            //    .HasColumnName("FyersCId");
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.LeadKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            //entity.Property(e => e.MotilalCid)
            //    .HasMaxLength(100)
            //    .IsUnicode(false)
            //    .HasColumnName("MotilalCId");
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Remarks)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Source)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TelegramId)
                .HasMaxLength(500)
                .IsUnicode(false);
            //entity.Property(e => e.ZerodhaCid)
            //    .HasMaxLength(100)
            //    .IsUnicode(false)
            //    .HasColumnName("ZerodhaCId");
        });

        modelBuilder.Entity<PartnerAccountActivity>(entity =>
        {
            entity.Property(e => e.Comments).IsRequired();
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<PartnerComment>(entity =>
        {
            entity.Property(e => e.Comments).IsRequired();
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<PaymentMode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentM__3214EC07902610C3");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<PerformanceResult>(entity =>
        {
            entity.HasKey(e => e.PublicKey).HasName("PK__Performa__37A9983BC21C209A");

            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Pincode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pincodes__3214EC07894ABE9E");

            entity.Property(e => e.Area)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.District)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Division)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Pincode1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Pincode");
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.State)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Purchase__3214EC0739F97C3D");

            entity.Property(e => e.BankName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ClientName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Dob)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DOB");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Mobile)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.NetAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Pan)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.Remark)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Service)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TransactionRecipt)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TransasctionReference)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Anonymouse)
            .HasDefaultValue(false)         // Tell EF the DB default
            .ValueGeneratedOnAdd();         // Important: allows DB to generate it if you don't set it
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC071AA21746");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<RoleMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RoleMenu__3214EC07CB36E2FE");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.MenuKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.RoleKey)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Segment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Segments__3214EC072C6E9881");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Services__3214EC07CFBFDC19");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.ServiceCategoryKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ServiceCost).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ServiceTypeKey)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceC__3214EC07FD15E8AE");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceT__3214EC075888BCBA");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Status__3214EC0785E72F27");

            entity.ToTable("Status");

            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Code)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.LotSize).HasDefaultValueSql("((1))");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Symbol)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Strategy>(entity =>
        {
            entity.HasKey(e => e.PublicKey).HasName("PK__Strategi__37A9983B1BBF5FE6");

            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tags__3214EC0710B8FC74");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07D9F948CF");

            entity.Property(e => e.Address)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Doj)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DOJ");
            entity.Property(e => e.EmailId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Gender)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsDelete).HasDefaultValueSql("((0))");
            entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PasswordKey)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordSalt).HasMaxLength(300);
            entity.Property(e => e.PublicKey).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.RoleKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserImage)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.IsUpdateAllow).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<XlPartner>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("xl_Partners");

            entity.Property(e => e.BranchCodeNew)
                .HasMaxLength(255)
                .HasColumnName("Branch Code New");
            entity.Property(e => e.ClientId).HasColumnName("Client Id");
            entity.Property(e => e.DayOfDerivativeFirstTradeDate)
                .HasMaxLength(255)
                .HasColumnName("Day of Derivative first trade date");
            entity.Property(e => e.DayOfDerivativeLastTradeDate)
                .HasMaxLength(255)
                .HasColumnName("Day of Derivative Last trade date");
            entity.Property(e => e.DayOfDerivativeUccResponseDate)
                .HasMaxLength(255)
                .HasColumnName("Day of Derivative UCC Response Date");
            entity.Property(e => e.DayOfFirstTradeDate)
                .HasMaxLength(255)
                .HasColumnName("Day of First Trade Date");
            entity.Property(e => e.DayOfLastTradeDate)
                .HasMaxLength(255)
                .HasColumnName("Day of Last trade date");
            entity.Property(e => e.DayOfUccResponseDate)
                .HasMaxLength(255)
                .HasColumnName("Day of UCC Response Date");
            entity.Property(e => e.Dd).HasColumnName("dd");
            entity.Property(e => e.Margin).HasMaxLength(255);
            entity.Property(e => e.RemeshireGroup)
                .HasMaxLength(255)
                .HasColumnName("Remeshire Group");
        });

        modelBuilder.Entity<XlStockList>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("xl_StockList");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<ReasonChangeLog>()
       .ToTable("ReasonChangeLog", "dbo");

        modelBuilder.Entity<LeadFreeTrial>()
      .ToTable("LeadFreeTrial", "dbo");

        modelBuilder.Entity<ScheduledNotification>()
     .ToTable("ScheduledNotificationM", "dbo");

        modelBuilder.Entity<Chapter>()
      .ToTable("Chapters", "dbo");
        modelBuilder.Entity<SubChapter>()
      .ToTable("SubChapters", "dbo");
        modelBuilder.Entity<QueryFormM>()
     .ToTable("QueryFormM", "dbo");
        modelBuilder.Entity<QueryFormRemarks>()
     .ToTable("QueryFormRemarks", "dbo");
        modelBuilder.Entity<TradeJournal>()
    .ToTable("TradingJournal", "dbo");
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}