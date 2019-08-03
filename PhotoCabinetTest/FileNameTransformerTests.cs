using PhotoCabinet;
using System;
using System.Linq;
using Xunit;

namespace PhotoCabinetTest
{
    public class FileNameTransformerTests
    {
        [Theory]
        [InlineData(@"1.jpg", @"IMG_[date]_[time]", @"", new string[] {}, "IMG_20190802_201005.jpg")] // normal
        [InlineData(@"1.png", @"IMG_[date]_[time]", @"", new string[] {}, "IMG_20190802_201005.png")] // suffix
        [InlineData(@"1.jpg", @"IMG_[date]_[time]", @"_[num]", new string[] { "IMG_20190802_201005.jpg" }, "IMG_20190802_201005_001.jpg")] // dedup
        [InlineData(@"1.jpg", @"IMG_[date]_[time]", @"_[num]", new string[] { "IMG_20190802_201005.jpg", "IMG_20190802_201005_001.jpg", "IMG_20190802_201005_002.jpg" }, "IMG_20190802_201005_003.jpg")] // multi dedup
        public void Transform_ShouldProduceCorrectName(string filePath, string format, string dedupFormat, string[] dedupSet, string expectedName)
        {
            var fileNameTransformer = new FileNameTransformer();
            var metadata = new Metadata
            {
                TimeInferredFromFileName = new DateTime(2019, 08, 02, 20, 10, 05),
                FilePath = filePath
            };

            Assert.Equal(expectedName, fileNameTransformer.Transform(metadata, format, dedupFormat, dedupSet.ToHashSet()));
        }
    }
}
