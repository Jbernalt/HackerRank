using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HackerRank.Services
{
    public interface IImageService
    {
        Task<string> SaveImage(IFormFile file, bool isProfileImage);
        bool DeleteImage(string filename, bool isProfileImage);
        Task Compressimage(string filename, IFormFile file);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task Compressimage(string filepath, IFormFile file)
        {
            try
            {
                float maxHeight = 500.0f;
                float maxWidth = 500.0f;
                int newWidth;
                int newHeight;

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                using var image  = Image.FromStream(memoryStream);

                string extension = image.RawFormat.ToString().ToLower();
                Bitmap originalBMP = new(image);
                int originalWidth = originalBMP.Width;
                int originalHeight = originalBMP.Height;

                if (originalWidth > maxWidth || originalHeight > maxHeight)
                {
                    // To preserve the aspect ratio  
                    float ratioX = (float)maxWidth / originalWidth;
                    float ratioY = (float)maxHeight / originalHeight;
                    float ratio = Math.Min(ratioX, ratioY);
                    newWidth = (int)(originalWidth * ratio);
                    newHeight = (int)(originalHeight * ratio);
                }
                else
                {
                    newWidth = originalWidth;
                    newHeight = originalHeight;
                }

                Bitmap bitMAP1 = new(originalBMP, newWidth, newHeight);
                Graphics imgGraph = Graphics.FromImage(bitMAP1);

                if (extension == "png")
                {
                    imgGraph.SmoothingMode = SmoothingMode.AntiAlias;
                    imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    imgGraph.DrawImage(originalBMP, 0, 0, newWidth, newHeight);

                    bitMAP1.Save(filepath);

                    bitMAP1.Dispose();
                    imgGraph.Dispose();
                    originalBMP.Dispose();
                }
                else if (extension == "jpg" || extension == "jpeg")
                {
                    imgGraph.SmoothingMode = SmoothingMode.AntiAlias;
                    imgGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    imgGraph.DrawImage(originalBMP, 0, 0, newWidth, newHeight);

                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    Encoder myEncoder = Encoder.Quality;
                    EncoderParameters myEncoderParameters = new(1);
                    EncoderParameter myEncoderParameter = new(myEncoder, 75L);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    bitMAP1.Save(filepath, jpgEncoder, myEncoderParameters);

                    bitMAP1.Dispose();
                    imgGraph.Dispose();
                    originalBMP.Dispose();
                }
                else if (extension == "gif")
                {
                    using var fileStream = new FileStream(filepath, FileMode.Create);
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
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

                await Compressimage(filePath, file);
            }
            return uniqueFileName;
        }
    }
}
