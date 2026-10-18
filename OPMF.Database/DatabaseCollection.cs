using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OPMF.Database
{
    public interface IDatabaseCollection<TItem>
    {
        TItem GetBySiteId(string id);
    }

    public class DatabaseCollection<TItem> : IDatabaseCollection<TItem>
    {
        protected string _idFieldName { get; } = "_id";

        protected LiteDatabase _liteDatabase { get; }

        protected ILiteCollection<TItem> _liteCollection { get; }

        public DatabaseCollection(LiteDatabase liteDatabase, string collectionName)
        {
            _liteDatabase = liteDatabase;
            _liteCollection = _liteDatabase.GetCollection<TItem>(collectionName);
        }

        public TItem GetBySiteId(string id)
        {
            return _liteCollection.FindOne(Query.Contains("SiteId", id));
        }

        public void InsertBulk(IEnumerable<TItem> items)
        {
            _liteCollection.InsertBulk(items);
        }

        public IEnumerable<TItem> GetAll()
        {
            return _liteCollection.FindAll();
        }
    }
}