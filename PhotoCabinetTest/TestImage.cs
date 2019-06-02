using System.IO;
using AutoFixture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PhotoTest
{
    public static class TestImage
    {
        public static void GenerateRandom(string filePath)
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new RandomNumericSequenceGenerator(100, 500));

            Generate(fixture.Create<int>(), fixture.Create<int>(), filePath);
        }

        public static void Generate(int width, int height, string filePath)
        {
            var image = Image.Load("TestImage.jpg");
            image.Mutate(ctx => ctx.Resize(width, height));

            using (var fileStream = File.Create(filePath))
            {
                image.SaveAsJpeg(fileStream);
            }
        }
    }
}
