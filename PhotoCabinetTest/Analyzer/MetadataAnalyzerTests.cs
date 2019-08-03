using PhotoCabinet.Analyzer;
using PhotoCabinet.Utility;
using System;
using System.Collections.Generic;
using Xunit;

namespace PhotoCabinetTest
{
    public class MetadataAnalyzerTests
    {
        [Theory]
        [MemberData(nameof(InferDateTimeFromFileName_TestData))]
        public void InferDateTimeFromFileName(string[] knownDateFormats, string fileName, DateTime? expectedResult)
        {
            Assert.Equal(expectedResult, MetadataAnalyzer.InferDateTimeFromFileName(knownDateFormats, fileName));
        }

        public static IEnumerable<object[]> InferDateTimeFromFileName_TestData()
        {
            yield return new object[] { new string[] { "yyyyMMdd_HHmmss" }, @"20160511_152459.jpg", new DateTime(2016, 05, 11, 15, 24, 59) };
            yield return new object[] { new string[] { "yyyyMMdd_HHmmss" }, @"IMG_20160511_152459.jpg", new DateTime(2016, 05, 11, 15, 24, 59) };
        }
    }
}
