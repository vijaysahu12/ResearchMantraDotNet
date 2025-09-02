namespace RM.Database.ResearchMantraContext
{
    public class Baskets
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsFree { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public int? SortOrder { get; set; }
    }
}