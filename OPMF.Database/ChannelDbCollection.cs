using System;
using System.Collections.Generic;
using System.Linq;

using LiteDB;

namespace OPMF.Database
{
    public interface IChannelDbCollection<TItem> : IDatabaseCollection<TItem> where TItem : Entities.Channel
    {
        List<TItem> GetManyBySiteIds(IEnumerable<string> ids);
        IEnumerable<TItem> GetAll();
        IEnumerable<TItem> GetNotBacklisted();
        IEnumerable<TItem> GetManyByWordInName(string wordInChannelName);
        void InsertOrUpdate(IEnumerable<TItem> items);
        void UpdateLastCheckedOutAndActivity(IEnumerable<TItem> items);
        void UpdateBlackListStatus(IEnumerable<TItem> items);
    }

    public class ChannelDbCollection<TItem> : DatabaseCollection<TItem>, IChannelDbCollection<TItem> where TItem : Entities.Channel
    {
        public ChannelDbCollection(LiteDatabase db, string collectionName) : base(db, collectionName) { }

        public List<TItem> GetManyBySiteIds(IEnumerable<string> ids)
        {
            return _liteCollection.Query().Where(c => ids.Contains(c.SiteId)).ToList();
        }

        public IEnumerable<TItem> GetNotBacklisted()
        {
            return _liteCollection.Find(i => i.Blacklisted == false).ToList();
        }

        public IEnumerable<TItem> GetManyByWordInName(string wordInChannelName)
        {
            return _liteCollection.Find(Query.Contains("Name", wordInChannelName));
        }

        public void InsertOrUpdate(IEnumerable<TItem> items)
        {
            try
            {
                IEnumerable<TItem> storedItemsToUpdate = _updateStoredItemsMatchingTheGivenItems(items, (item, storedItem) =>
                {
                    storedItem.Name = item.Name;
                    storedItem.Url = item.Url;
                    storedItem.Thumbnail.Url = item.Thumbnail.Url;
                    storedItem.Thumbnail.Width = item.Thumbnail.Width;
                    storedItem.Thumbnail.Height = item.Thumbnail.Height;
                    if (item.Description != null)
                    {
                        storedItem.Description = item.Description;
                    }
                });

                IEnumerable<TItem> toInsert = items.Where(i => !storedItemsToUpdate.Any(j => j.Id == i.Id));
                _liteCollection.InsertBulk(toInsert);

                _liteDatabase.Commit();
            }
            catch (Exception)
            {
                _liteDatabase.Rollback();
                throw;
            }
        }

        public void UpdateLastCheckedOutAndActivity(IEnumerable<TItem> items)
        {
            try
            {
                _updateStoredItemsMatchingTheGivenItems(items, (item, storedItem) =>
                {
                    storedItem.LastCheckedOut = item.LastCheckedOut;
                    storedItem.LastActivityDate = item.LastActivityDate;
                });

                _liteDatabase.Commit();
            }
            catch (Exception)
            {
                _liteDatabase.Rollback();
                throw;
            }
        }

        public void UpdateBlackListStatus(IEnumerable<TItem> items)
        {
            try
            {
                _updateStoredItemsMatchingTheGivenItems(items, (item, storedItem) => storedItem.Blacklisted = item.Blacklisted);
            }
            catch (Exception)
            {
                _liteDatabase.Rollback();
                throw;
            }
        }

        protected IEnumerable<TItem> _updateStoredItemsMatchingTheGivenItems(IEnumerable<TItem> items, Action<TItem, TItem> copyGivenItemOntoStoredItem)
        {
            IEnumerable<string> itemIds = items.Select(i => i.Id);
            List<TItem> storedItemsToUpdate = _liteCollection.Find(i => itemIds.Contains(i.SiteId)).ToList(); // storedItemsToUpdate must be list or it won't update in foreach loop
            foreach (TItem storedItem in storedItemsToUpdate)
            {
                TItem item = items.First(i => i.Id == storedItem.Id);
                copyGivenItemOntoStoredItem(item, storedItem);
            }
            _liteCollection.Update(storedItemsToUpdate);

            return storedItemsToUpdate;
        }
    }
}
