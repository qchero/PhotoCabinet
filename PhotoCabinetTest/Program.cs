using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoTest
{
    public class Program
    {
        public static void MainXX(string[] args)
        {
            Console.WriteLine("Hello World!");

            for (int i = 0; i < 10; i++)
            {
                TestImage.GenerateRandom($"TestImage_{i}.jpg");
            }

            {
                var image1 = Image.Load("TestImage.jpg");
                var md51 = GetMd5(image1);
            }

            {
                var image2 = Image.Load("TestImage2.jpg");
                var md52 = GetMd5(image2);

                var image3 = image2.Clone();
                var md53 = GetMd5(image3);

                image2.Mutate(ctx => ctx.Resize(image2.Width / 2, image2.Height / 2));
                using (var fileStream = File.Create("TestImage_Resize.jpe"))
                {
                    image2.SaveAsJpeg(fileStream);
                }
            }
        }

        public static string GetMd5(Image<Rgba32> image)
        {
            var md5 = MD5.Create();
            Stream s = new MemoryStream();
            image.SaveAsJpeg(s);
            return Md5ToString(md5.ComputeHash(s));
        }

        public static string Md5ToString(byte[] md5)
        {
            var sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            foreach (var t in md5)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
