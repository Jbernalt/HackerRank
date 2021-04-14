using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HackerRank.Services
{
    public interface IImageService
    {
        Task<string> SaveImage(IFormFile file, bool isProfileImage);
        bool DeleteImage(string filename, bool isProfileImage);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public bool DeleteImage(string filename, bool isProfileImage)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            string filepath;

            if (isProfileImage)
                filepath = Path.Combine(_webHostEnvironment.WebRootPath, "profileImg", filename);
            else
                filepath = Path.Combine(_webHostEnvironment.WebRootPath, "achievementImg", filename);

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
                return true;
            }
            return false;
        }

        public async Task<string> SaveImage(IFormFile file, bool isProfileImage)
        {
            string uniqueFileName = null;

            if (file != null)
            {
                string uploadsFolder;

                if (isProfileImage)
                    uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "profileImg");
                else
                    uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "achievementImg");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fileStream);
            }
            return uniqueFileName;
        }
    }
}
