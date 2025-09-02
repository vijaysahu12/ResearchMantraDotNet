using System;

namespace RM.Database.ResearchMantraContext;

public partial class CustomerService
{
    public long Id { get; set; }

    public string CustomerKey { get; set; }

    public string ServiceKey { get; set; }

    public string PaymentModeKey { get; set; }

    public decimal? ActualCost { get; set; }

    public decimal? AmountPaid { get; set; }

    public decimal? AmountOutstanding { get; set; }

    public decimal? Discount { get; set; }

    public byte? IsWon { get; set; }

    public string OrderId { get; set; }

    public string Remarks { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }
}
