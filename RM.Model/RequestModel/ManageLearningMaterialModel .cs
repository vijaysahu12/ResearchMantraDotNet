using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace RM.Model.RequestModel
{
    public class LearningMaterialModel
    {
        public class ManageLearningCategoryRequestModel
        {
            public int? Id { get; set; }
            public string Title { get; set; }
            public string? Description { get; set; }
            public IFormFile? ImageUrl { get; set; }
            public DateTime? CreatedOn { get; set; }
            public bool? IsActive { get; set; }
            public bool? IsDelete { get; set; }
            public Guid? ModifiedBy { get; set; }
            public DateTime? ModifiedOn { get; set; }
        }

        public class ManageLearningContentRequestModel
        {
            public int? Id { get; set; }
            public int MaterialId { get; set; }
            public string Title { get; set; }
            public string? Description { get; set; }
            public string? AttachmentTitle { get; set; }
            public string? AttachmentDescription { get; set; }
            public string? AttachmentUrl { get; set; }
            public IFormFile? ProductImage { get; set; }
            public IFormFile? FirstImage { get; set; }
            public IFormFile? SecondImage { get; set; }
            public List<string> ExampleTitleList { get; set; }
            public List<int> ExampleIdList { get; set; }
            public IFormFileCollection ExampleContentImageList { get; set; }
            public List<int> ExampleIdListToUpdate { get; set; }
            public Guid LoggedUser { get; set; }
        }

        public class UpdateLearningCategoryStatusRequest
        {
        }
    }
}