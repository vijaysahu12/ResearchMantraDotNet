namespace RM.Model.RequestModel
{
    public class WhatsappTemplateMessage
    {
        public string? TemplateBody { get; set; }
        public string? MobileNumber { get; set; }
    }

    public class WhatsappLeadsTemplateMessage
    {
        public string? FullName { get; set; }
        public string? MobileNumber { get; set; }
        public string TemplateBody { get; set; }
    }

    public class CommonTemplateMessage
    {
        public string? MobileNumber { get; set; }
        public string TemplateBody { get; set; }
    }
}
