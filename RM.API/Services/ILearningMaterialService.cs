using RM.API.Helpers;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.RequestModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface ILearningMaterialService
    {
        Task<ApiCommonResponseModel> GetLearningMaterialCategory();

        ApiCommonResponseModel GetLearningMaterialItemBasedOnCategoryId(int id);

        Task<ApiCommonResponseModel> ManageLearningContent(LearningMaterialModel.ManageLearningContentRequestModel request);

        Task<ApiCommonResponseModel> LearningMaterialContentActiveToggle(int learningMaterialContentId);

        Task<ApiCommonResponseModel> ManageLearningCategory(LearningMaterialModel.ManageLearningCategoryRequestModel request, Guid loginUser);

        Task<ApiCommonResponseModel> UpdateCategoryStatus(LearningMaterialModel.ManageLearningCategoryRequestModel request);
    }

    public class LearningMaterialService : ILearningMaterialService
    {
        private readonly ApiCommonResponseModel responseModel = new();
        private readonly KingResearchContext _dbContext;
        private readonly IConfiguration _configuration;
        private string ImageHtml => $"<a href=\"imageFileName\"><img src=\"{_configuration["Mobile:BaseUrl"]}api/Product/GetImage?imageName=imageFileName\"></a>";

        public LearningMaterialService(KingResearchContext context, IConfiguration configuration)
        {
            _dbContext = context;
            _configuration = configuration;
        }

        public async Task<ApiCommonResponseModel> GetLearningMaterialCategory()
        {
            responseModel.Data = _dbContext.LearningMaterialM.Select(item =>
            new
            {
                item.Id,
                item.Title,
                item.Description,
                item.ImageUrl,
                item.IsActive,
                item.CreatedOn,
                item.ModifiedOn
            }).OrderByDescending(item => item.ModifiedOn ?? item.CreatedOn).ToList();
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public ApiCommonResponseModel GetLearningMaterialItemBasedOnCategoryId(int id)
        {
            var results = _dbContext.LearningContentM.Where(lc => lc.MaterialId == id)
            .Select(lc => new
            {
                lc.Id,
                lc.Title,
                lc.Description,
                lc.ListImageUrl,
                lc.AttachmentTitle,
                lc.AttachmentDescription,
                lc.Attachment,
                lc.FirstImage,
                lc.SecondImage,
                lc.IsActive,
                ExampleContentJson = _dbContext.LearningContentExampleM
                    .Where(lcm => lcm.ContentId == lc.Id && lcm.IsActive == true && lcm.IsDelete == false)
                    .Select(lcm => new
                    {
                        id = lcm.Id,
                        title = lcm.Title,
                        imageName = lcm.ImageUrl
                    }).ToList(),
                lc.CreatedOn,
                lc.ModifiedOn
            })
            .AsEnumerable()
            .Select(lc => new
            {
                lc.Id,
                lc.Title,
                lc.Description,
                lc.ListImageUrl,
                lc.AttachmentTitle,
                lc.AttachmentDescription,
                lc.Attachment,
                lc.FirstImage,
                lc.SecondImage,
                lc.IsActive,
                ExampleContentJson = JsonSerializer.Serialize(lc.ExampleContentJson),
                lc.CreatedOn,
                lc.ModifiedOn
            })
            .OrderByDescending(lc => lc.ModifiedOn.GetValueOrDefault(lc.CreatedOn))
            .ToList();

            responseModel.Data = results;
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManageLearningCategory(LearningMaterialModel.ManageLearningCategoryRequestModel request, Guid loginUser)
        {
            var responseModel = new ApiCommonResponseModel();

            try
            {
                SaveProductListImage saveImage = new(_configuration);

                if (request.Id > 0)
                {
                    // Update existing record
                    var existingMaterial = await _dbContext.LearningMaterialM.FindAsync(request.Id);
                    if (existingMaterial != null)
                    {
                        existingMaterial.Title = request.Title;
                        existingMaterial.Description = request.Description;
                        existingMaterial.ImageUrl = request.ImageUrl is not null
                            ? await saveImage.SaveListImage(request.ImageUrl)
                            : existingMaterial.ImageUrl;
                        existingMaterial.ModifiedOn = DateTime.Now;
                        existingMaterial.ModifiedBy = loginUser;

                        _dbContext.LearningMaterialM.Update(existingMaterial);
                        responseModel.StatusCode = HttpStatusCode.OK;
                        responseModel.Message = "Learning material updated successfully.";
                    }
                    else
                    {
                        responseModel.StatusCode = HttpStatusCode.NotFound;
                        responseModel.Message = "Learning material not found.";
                    }
                }
                else
                {
                    LearningMaterialM learningMaterial = new()
                    {
                        Title = request.Title,
                        Description = request.Description,
                        ImageUrl = request.ImageUrl is not null ? await saveImage.SaveListImage(request.ImageUrl) : null,
                        CreatedOn = DateTime.Now,
                        CreatedBy = loginUser,
                        IsActive = false,
                        IsDelete = false
                    };

                    await _dbContext.LearningMaterialM.AddAsync(learningMaterial);
                    responseModel.StatusCode = HttpStatusCode.OK;
                    responseModel.Message = "Learning material created successfully.";
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = ex.Message;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> UpdateCategoryStatus(LearningMaterialModel.ManageLearningCategoryRequestModel request)
        {
            var responseModel = new ApiCommonResponseModel();

            try
            {
                var category = await _dbContext.LearningMaterialM.FindAsync(request.Id);

                if (category == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "Category not found.";
                    return responseModel;
                }

                category.IsActive = (bool)request.IsActive;
                category.ModifiedBy = request.ModifiedBy;
                category.ModifiedOn = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                var status = category.IsActive ? "enabled" : "disabled";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = $"Category '{category.Title}' has been {status} successfully.";
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = ex.Message;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManageLearningContent(LearningMaterialModel.ManageLearningContentRequestModel request)
        {
            SaveProductListImage saveImage = new(_configuration);

            // update learning content
            if (request.Id > 0)
            {
                string newHtmlString = string.Empty;

                LearningContentM learningContent = _dbContext.LearningContentM.Where(x => x.Id == request.Id).FirstOrDefault();
                if (learningContent is null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "Content Not Found.";
                    return responseModel;
                }
                learningContent.Title = request.Title;
                learningContent.AttachmentDescription = request.AttachmentDescription;
                learningContent.Attachment = request.AttachmentUrl;
                learningContent.AttachmentTitle = request.AttachmentTitle;
                learningContent.ModifiedOn = DateTime.Now;
                learningContent.ListImageUrl = request.ProductImage is not null ? await saveImage.SaveLandscapeImage(request.ProductImage) : learningContent.ListImageUrl;

                if (request.FirstImage is not null)
                {
                    learningContent.FirstImage = await saveImage.SaveListImage(request.FirstImage);
                    request.Description = ReplaceImageStringInHtml(request.Description, "[firstImage]", ReplaceHtmlImgWithImageName(learningContent.FirstImage));
                }
                else
                {
                    request.Description = ReplaceImageStringInHtml(request.Description, "[firstImage]", ReplaceHtmlImgWithImageName(learningContent.FirstImage));
                }

                if (request.SecondImage is not null)
                {
                    learningContent.SecondImage = await saveImage.SaveListImage(request.SecondImage);
                    request.Description = ReplaceImageStringInHtml(request.Description, "[secondImage]", ReplaceHtmlImgWithImageName(learningContent.SecondImage));
                }
                else
                {
                    request.Description = ReplaceImageStringInHtml(request.Description, "[firstImage]", ReplaceHtmlImgWithImageName(learningContent.SecondImage));
                }

                learningContent.Description = request.Description;

                // update the example
                var existingExamples = await _dbContext.LearningContentExampleM
                .Where(x => x.ContentId == learningContent.Id && x.IsActive == true && x.IsDelete == false)
                .ToListAsync();

                // Keep track of the number of existing examples
                int existingCount = existingExamples.Count;

                // Update existing examples and add new ones
                if (request.ExampleTitleList is not null)
                {
                    for (int i = 0; i < request.ExampleTitleList.Count; i++)
                    {
                        var exampleId = request.ExampleIdList[i];
                        var title = request.ExampleTitleList[i];
                        //var image = request.ExampleContentImageList.ElementAtOrDefault(i);

                        if (exampleId == 0)
                        {
                            // Add new example
                            int imageCount = 0;

                            var newExample = new LearningContentExampleM
                            {
                                ContentId = learningContent.Id,
                                Title = title,
                                IsActive = true,
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = request.LoggedUser
                            };

                            // Check if this is a new example (index greater than or equal to existing count)

                            newExample.ImageUrl = await saveImage.SaveLandscapeImage(request.ExampleContentImageList[imageCount]);

                            _dbContext.LearningContentExampleM.Add(newExample);
                        }
                        else
                        {
                            // Update existing example
                            var existingExample = existingExamples.FirstOrDefault(e => e.Id == exampleId);
                            if (existingExample != null)
                            {
                                existingExample.Title = title;

                                if (request.ExampleIdListToUpdate is not null && request.ExampleIdListToUpdate.Contains(exampleId))
                                {
                                    int imageCount = 0;
                                    existingExample.ImageUrl = await saveImage.SaveLandscapeImage(request.ExampleContentImageList[imageCount]);
                                    imageCount++;
                                }

                                // Ensure the example is marked as active and not deleted
                                existingExample.IsActive = true;
                                existingExample.IsDelete = false;
                            }
                        }
                    }
                    // Remove examples that are not in the request
                    var exampleIdsToKeep = request.ExampleIdList.Where(id => id != 0).ToList();
                    foreach (var example in existingExamples)
                    {
                        if (!exampleIdsToKeep.Contains(example.Id))
                        {
                            example.IsActive = false;
                            example.IsDelete = true;
                        }
                    }
                }
                else
                {
                    // now examples came empty so delete every example
                    existingExamples.ForEach(item =>
                    {
                        item.IsActive = false;
                        item.IsDelete = true;
                    });
                }

                await _dbContext.SaveChangesAsync();

                responseModel.Message = "Update successfull.";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            // insert learning content
            else
            {
                string newHtmlString = string.Empty;

                LearningContentM learningContentM = new()
                {
                    Title = request.Title,
                    MaterialId = request.MaterialId,
                    AttachmentDescription = request.AttachmentDescription,
                    Attachment = request.AttachmentUrl,
                    AttachmentTitle = request.AttachmentTitle,
                    ListImageUrl = request.ProductImage is not null ? await saveImage.SaveListImage(request.ProductImage) : null,
                    CreatedOn = System.DateTime.Now
                };

                if (request.FirstImage is not null)
                {
                    learningContentM.FirstImage = await saveImage.SaveListImage(request.FirstImage);
                    request.Description = ReplaceImageStringInHtml(request.Description, "[firstImage]", ReplaceHtmlImgWithImageName(learningContentM.FirstImage));
                }

                if (request.SecondImage is not null)
                {
                    learningContentM.SecondImage = await saveImage.SaveListImage(request.SecondImage);
                    request.Description = ReplaceImageStringInHtml(request.Description, "[secondImage]", ReplaceHtmlImgWithImageName(learningContentM.SecondImage));
                }

                learningContentM.Description = request.Description;

                _dbContext.LearningContentM.Add(learningContentM);
                await _dbContext.SaveChangesAsync();

                List<LearningContentExampleM> examples = new();
                if (request.ExampleTitleList != null)
                {
                    for (int i = 0; i < request.ExampleTitleList.Count; i++)
                    {
                        LearningContentExampleM example = new()
                        {
                            ContentId = learningContentM.Id,
                            CreatedOn = DateTime.Now,
                            ImageUrl = request.ExampleContentImageList is not null ? await saveImage.SaveLandscapeImage(request.ExampleContentImageList[i]) : null,
                            CreatedBy = request.LoggedUser,
                            IsActive = true,
                            IsDelete = false,
                            Title = request.ExampleTitleList[i],
                        };

                        examples.Add(example);
                    }
                }

                _dbContext.LearningContentExampleM.AddRange(examples);

                await _dbContext.SaveChangesAsync();

                responseModel.Message = "Added Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
            }


            return responseModel;
        }

        public async Task<ApiCommonResponseModel> LearningMaterialContentActiveToggle(int learningMaterialContentId)
        {
            LearningContentM learningContent = _dbContext.LearningContentM.Where(x => x.Id == learningMaterialContentId).FirstOrDefault();
            if (learningContent is null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "Content Not Found.";
                return responseModel;
            }
            learningContent.IsActive = !learningContent.IsActive;

            await _dbContext.SaveChangesAsync();

            responseModel.Message = "Update successfull.";
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        private string ReplaceHtmlImgWithImageName(string ImageNameToReplace)
        {
            string newImageHtml = ImageHtml.Replace("imageFileName", ImageNameToReplace);

            return newImageHtml;
        }

        private string ReplaceImageStringInHtml(string htmlString, string stringToReplace, string newHtmlString)
        {
            string newImage = htmlString.Replace(stringToReplace, newHtmlString);
            return newImage;
        }
    }
}