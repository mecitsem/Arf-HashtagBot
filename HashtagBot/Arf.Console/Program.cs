using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arf.Services;
using Microsoft.ProjectOxford.Vision.Contract;

namespace Arf.Console
{
    class Program
    {
        public static bool IsProcessing;
        public static void Main(string[] args)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("Please write image Url or local path");
            
            while (true)
            {
                if (IsProcessing) continue;
                System.Console.ResetColor();
                var imgPath = System.Console.ReadLine();
                Run(imgPath);
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.WriteLine("It takes a few time.Please wait!");
                System.Console.ResetColor();
            }
        }

        public static async void Run(string imgPath)
        {
            IsProcessing = true;
            var isUpload = imgPath != null && !imgPath.StartsWith("http");

            var service = new VisionService();
            var analysisResult = isUpload
                ? await service.UploadAndDescripteImage(imgPath)
                : await service.DescripteUrl(imgPath);

            System.Console.ForegroundColor = ConsoleColor.DarkGreen;
            System.Console.WriteLine(string.Join(" ", analysisResult.Description.Tags.Select(s => s = "#" + s)));
            System.Console.ResetColor();
            IsProcessing = false;
        }


    }
}
