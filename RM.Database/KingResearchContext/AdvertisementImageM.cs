using System;

namespace RM.Database.ResearchMantraContext
{
    public class AdvertisementImageM
    {

        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string Name { get; set; }
        public string ProductName { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public DateTime? ExpireOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }

    public class PromotionM
    {
        public int Id { get; set; }           
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MediaUrl { get; set; }             
        public string? MediaType { get; set; }
        public string? ButtonText { get; set; }
       // public string? SecondaryButtonText { get; set; }   
        public string? ActionUrl { get; set; }              
      //  public string? DownloadUrl { get; set; }        
      //  public bool? ShowDownloadButton { get; set; }        
        public DateTime? StartDate { get; set; }          
        public DateTime? EndDate { get; set; }            
        public bool? IsActive { get; set; }               
        public Guid? CreatedBy { get; set; }             
        public bool IsDelete { get; set; }                 
        public DateTime? CreatedOn { get; set; }        
        public Guid? ModifiedBy { get; set; }             
        public DateTime? ModifiedOn { get; set; }   
        public bool? ShouldDisplay { get; set; }
        public bool? IsNotification { get; set; }
        public DateTime? ScheduleDate { get; set; }

        // Newly Added Fields
        public int? MaxDisplayCount { get; set; }          
        public int? DisplayFrequency { get; set; }        
        public DateTime? LastShownAt { get; set; }      
        public bool? GlobalButtonAction { get; set; }      
        public string? Target { get; set; }               
        public string? ProductName { get; set; }          
        public int? ProductId { get; set; }                
    }


}
