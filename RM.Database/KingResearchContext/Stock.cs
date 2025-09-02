using System;

namespace RM.Database.ResearchMantraContext;

public partial class Stock
{
    public int Id { get; set; }
    //public string Name { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int? LotSize { get; set; }
    public string Logo { get; set; }
    public byte? IsDisabled { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? Exchange { get; set; }
}



public partial class Contracts
{
    public int Id { get; set; }
    public string Symbol { get; set; }
    public string TradingSymbol { get; set; }
    public double StrikePrice { get; set; }
    public string LotSize { get; set; }
    public string Token { get; set; }
    public string Exch { get; set; }
    public string OptionType { get; set; }
    public DateTime ContractDate { get; set; }
    public string Month { get; set; }
}