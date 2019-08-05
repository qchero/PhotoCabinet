using PhotoCabinet.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace PhotoCabinetTest.Database
{
    public class Md5CacheTests
    {
        [Fact]
        public void CachedFuncTest()
        {
            var md5Cache = new Md5Cache("tmp.db");
            md5Cache.DeleteAll();

            var val1 = md5Cache.CachedFunc("key1", () => "value1");
            Assert.Equal("value1", val1);

            var val2 = md5Cache.CachedFunc("key1", () => "value2");
            Assert.Equal("value1", val2);

            var val3 = md5Cache.CachedFunc("key2", () => "value3");
            Assert.Equal("value3", val3);
        }
    }
}
