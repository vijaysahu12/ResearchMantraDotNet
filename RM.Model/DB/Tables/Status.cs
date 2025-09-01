namespace RM.Model.DB.Tables
{
    //public class Status
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public string Category { get; set; }
    //    public string Description { get; set; }
    //}

    public enum StatusEnum
    {
        NotReceived,
        PartialReceived,
        Received,
        Rejected,
        Refunded
    }
}
