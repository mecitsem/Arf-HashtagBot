using System;
using System.IO;
using System.Threading.Tasks;
using Arf.Core;
using Arf.Services.Common;
using Arf.Services.Helpers;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace Arf.Services
{
    public class VisionService : BaseService
    {
        #region Constructors
        public VisionService()
        {
            var subsKey = VisionServiceHelper.GetSubscriptionKey();

            if (string.IsNullOrEmpty(subsKey))
                throw new ArgumentNullException($"Vision Service SubscriptionKey is null");

            SubscriptionKey = subsKey;
        }
        #endregion

        #region Methods
        public async Task<AnalysisResult> UploadAndDescripteImage(string imageFilePath)
        {
            // Create Project Oxford Vision API Service client
            var visionServiceClient = new VisionServiceClient(SubscriptionKey);

            using (Stream imageFileStriStream = File.OpenRead(imageFilePath))
            {
                // Upload and image and request three descriptions
                var analysisResult = await visionServiceClient.DescribeAsync(imageFileStriStream, 3);
                return analysisResult;
            }
        }

        public async Task<AnalysisResult> UploadAndDescripteImage(byte[] content)
        {
            // Create Project Oxford Vision API Service client
            var visionServiceClient = new VisionServiceClient(SubscriptionKey);

            using (Stream imageFileStriStream = new MemoryStream(content))
            {
                // Upload and image and request three descriptions
                var analysisResult = await visionServiceClient.DescribeAsync(imageFileStriStream, 3);
                return analysisResult;
            }
        }

        public async Task<AnalysisResult> DescripteUrl(string imageUrl)
        {
            var visionServiceClient = new VisionServiceClient(SubscriptionKey);
            var analysisResult = await visionServiceClient.DescribeAsync(imageUrl, 3);
            return analysisResult;
        }

        public async Task<AnalysisResult> DoWork(Uri imageUri, bool upload)
        {
            AnalysisResult analysisResult;

            if (upload)
            {
                analysisResult = await UploadAndDescripteImage(imageUri.LocalPath);
            }
            else
            {
                analysisResult = await DescripteUrl(imageUri.AbsolutePath);
            }

            return analysisResult;
        }
        #endregion
    }
}
