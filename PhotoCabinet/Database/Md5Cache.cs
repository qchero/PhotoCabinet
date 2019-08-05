using LiteDB;
using PhotoCabinet.Model;
using System;

namespace PhotoCabinet.Database
{
    public class Md5Cache
    {
        private readonly LiteCollection<Md5CacheEntity> m_md5Cache;

        public Md5Cache(string dbPath)
        {
            var db = new LiteDatabase(dbPath);
            m_md5Cache = db.GetCollection<Md5CacheEntity>("Md5Cache");
        }

        public string CachedFunc(string fileName, Func<string> func)
        {
            var id = new BsonValue(fileName);
            var result = m_md5Cache.FindById(id);
            if (result != null)
            {
                return result.Md5;
            }

            var md5 = func();
            m_md5Cache.Insert(id, new Md5CacheEntity
            {
                Md5 = md5
            });
            return md5;
        }

        public void DeleteAll()
        {
            m_md5Cache.Delete(e => true);
        }

        private class Md5CacheEntity
        {
            public string Md5 { get; set; }
        }
    }
}
