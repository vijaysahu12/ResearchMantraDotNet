using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RM.API.Helpers
{
    public class SaveProductListImage
    {
        public SaveProductListImage(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        public async Task<string> SaveListImage(IFormFile image)
        {
            if (image == null) { return null; }
            var mobileAssetsFolderPath = _configuration["Mobile:RootDirectory"];

            string assetsDirectory = Path.Combine(mobileAssetsFolderPath, "Assets", "Products");

            if (!Directory.Exists(assetsDirectory))
            {
                Directory.CreateDirectory(assetsDirectory);
            }

            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(image.FileName)}";

            string filePath = Path.Combine(assetsDirectory, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return fileName;
        }

        public async Task<string> SaveLandscapeImage(IFormFile image)
        {
            if (image == null) { return null; }
            var mobileAssetsFolderPath = _configuration["Mobile:RootDirectory"];

            string assetsDirectory = Path.Combine(mobileAssetsFolderPath, "Assets", "Products");

            if (!Directory.Exists(assetsDirectory))
            {
                Directory.CreateDirectory(assetsDirectory);
            }

            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(image.FileName)}";

            string filePath = Path.Combine(assetsDirectory, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return fileName;
        }
    }
}