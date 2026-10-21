using System;
using System.Collections.Generic;
using System.Linq;

using LiteDB;
using OPMF.Entities;

namespace OPMF.Database
{
    public interface IMetadataDbCollection<TItem> : IDatabaseCollection<TItem> where TItem : Metadata
    {
        IEnumerable<TItem> GetToDownload();
        IEnumerable<TItem> GetNew(int skip, int pageSize);
        IEnumerable<TItem> GetToDownloadAndWait(int skip, int pageSize);
        IEnumerable<TItem> GetManyByWordInTitle(string wordInMetadataTitle, int skip, int pageSize);
        IEnumerable<TItem> GetManyByChannelSiteIdAndWordInTitle(IEnumerable<string> channelSiteIds, string wordInMetadataTitle, int skip, int pageSize);
        void InsertNew(IEnumerable<TItem> items);
        void UpdateStatus(IEnumerable<TItem> items);
        void UpdateDownloadQualityChoiceOverrides(IEnumerable<TItem> items);
        void UpdateIsBeingProcessed(IEnumerable<TItem> items, bool? isProcessedValue = null);
    }

    public class MetadataDbCollection<TItem> : DatabaseCollection<TItem>, IMetadataDbCollection<TItem> where TItem : Metadata
    {
        public MetadataDbCollection(LiteDatabase db, string collectionName) : base(db, collectionName) { }

        public IEnumerable<TItem> GetToDownload()
        {
            return _liteCollection.Find(i => i.Status == MetadataStatus.ToDownload);
        }

        public IEnumerable<TItem> GetToDownloadAndWait(int skip, int pageSize)
        {
            return _liteCollection.Find(i => i.Status == MetadataStatus.ToDownload || i.Status == MetadataStatus.Wait, skip, pageSize);
        }

        public IEnumerable<TItem> GetNew(int skip, int pageSize)
        {
            return _liteCollection.Find(i => i.Status == MetadataStatus.New, skip, pageSize);
        }

        public IEnumerable<TItem> GetManyByWordInTitle(string wordInMetadataTitle, int skip, int pageSize)
        {
            return _liteCollection.Query().Where(i => i.Title.Contains(wordInMetadataTitle)).OrderByDescending(i => i.PublishedAt).Skip(skip).Limit(pageSize).ToList();
        }

        public IEnumerable<TItem> GetManyByChannelSiteIdAndWordInTitle(IEnumerable<string> channelSiteIds, string wordInMetadataTitle, int skip, int pageSize)
        {
            BsonArray siteIdsBsonArray = new BsonArray();
            foreach (string channelSiteId in channelSiteIds)
            {
                siteIdsBsonArray.Add(channelSiteId);
            }
            BsonExpression whereCause = Query.In("ChannelSiteId", siteIdsBsonArray);
            if (!string.IsNullOrWhiteSpace(wordInMetadataTitle))
            {
                whereCause = Query.And(whereCause, Query.Contains("Title", wordInMetadataTitle));
            }
            return _liteCollection.Find(whereCause, skip, pageSize);
        }

        public void InsertNew(IEnumerable<TItem> items)
        {
            try
            {
                IEnumerable<TItem> toInsert = items.Where(i => _liteCollection.FindOne(Query.EQ(_idFieldName, i.Id)) == null);
                _liteCollection.InsertBulk(toInsert);
                _liteDatabase.Commit();
            }
            catch (Exception)
            {
                _liteDatabase.Rollback();
                throw;
            }
        }

        public void UpdateStatus(IEnumerable<TItem> items)
        {
            try
            {
                _updateStoredItemsMatchingTheGivenItems(items, (item, storedItem) => storedItem.Status = item.Status);
                _liteDatabase.Commit();
            }
            catch (Exception)
            {
                _liteDatabase.Rollback();
                throw;
            }
        }

        public void UpdateDownloadQualityChoiceOverrides(IEnumerable<TItem> items)
        {
            try
            {
                _updateStoredItemsMatchingTheGivenItems(items, (item, storedItem) =>
                {
                    storedItem.VideoCodecChoiceOverride = item.VideoCodecChoiceOverride;
                    storedItem.MaximumVideoResolutionChoiceOverride = item.MaximumVideoResolutionChoiceOverride;
                    storedItem.AudioQualityChoiceOverride = item.AudioQualityChoiceOverride;
                });
                _liteDatabase.Commit();
            }
            catch (Exception)
            {
                _liteDatabase.Rollback();
                throw;
            }
        }

        public void UpdateIsBeingProcessed(IEnumerable<TItem> items, bool? isProcessedValue = null)
        {
            try
            {
                _updateStoredItemsMatchingTheGivenItems(items, (item, storedItem) =>
                {
                    if (isProcessedValue.HasValue) // Need to update items as well
                    {
                        item.IsBeingDownloaded = isProcessedValue.Value;
                    }
                    storedItem.IsBeingDownloaded = item.IsBeingDownloaded;
                });
                _liteDatabase.Commit();
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
