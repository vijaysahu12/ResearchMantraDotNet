using System;

namespace RM.Database.ResearchMantraContext
{
    public class WatiTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateBody { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
