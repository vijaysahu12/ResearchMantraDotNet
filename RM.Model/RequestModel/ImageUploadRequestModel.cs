using Microsoft.AspNetCore.Http;

public class ImageUploadRequestModel
{
    public IFormFile Image { get; set; }
}