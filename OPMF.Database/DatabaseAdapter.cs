﻿using LiteDB;
using System;

namespace OPMF.Database
{
    public class DatabaseAdapter : IDisposable
    {
        // --- Static object ---
        public static void AccessDbAdapter(Action<DatabaseAdapter> DbAction)
        {
            using (DatabaseAdapter databaseAdapter = new DatabaseAdapter(Settings.ReadonlySettings.GetDatabasePath()))
            {
                DbAction(databaseAdapter);
            }
        }

        // --- Dynamics objects ---
        private string __dbPath;
        private LiteDatabase __db;

        // Collections
        private YoutubeMetadataDbCollection __youtubeMetadataDbCollection = null;
        public YoutubeMetadataDbCollection YoutubeMetadataDbCollection
        {
            get
            {
                return __youtubeMetadataDbCollection;
            }
        }
        private YoutubeChannelDbCollection __youtubeChannelDbCollection = null;
        public YoutubeChannelDbCollection YoutubeChannelDbCollection
        {
            get
            {
                return __youtubeChannelDbCollection;
            }
        }

        public DatabaseAdapter(string dbPath)
        {
            __dbPath = dbPath;
            __db = new LiteDatabase(__dbPath);

            __youtubeMetadataDbCollection = new YoutubeMetadataDbCollection(__db);
            __youtubeChannelDbCollection = new YoutubeChannelDbCollection(__db);
        }

        public void MigrateData()
        {
            DatabaseAdapter newDatabaseAdapter = new DatabaseAdapter(Settings.ReadonlySettings.GetDatabasePath() + ".new");
            newDatabaseAdapter.YoutubeMetadataDbCollection.InsertBulk(__youtubeMetadataDbCollection.GetAll());
            newDatabaseAdapter.YoutubeChannelDbCollection.InsertBulk(__youtubeChannelDbCollection.GetAll());
        }

        public void Dispose()
        {
            __db.Dispose();
        }
    }
}
