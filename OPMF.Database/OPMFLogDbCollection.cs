using LiteDB;
using OPMF.Entities;
using System;
using System.Collections.Generic;

namespace OPMF.Database
{
    public class OPMFLogDbCollection : DatabaseCollection<OPMFLog>
    {
        private static readonly string __collectionName = "OPMFLog";
        public OPMFLogDbCollection(LiteDatabase db) : base(db, __collectionName) { }

        public List<OPMFLog> GetWarnings(DateTime startDateTime, DateTime endDateTime)
        {
            return _liteCollection.Query().Where(i => i.Type == OPMFLogType.Warning
                && i.DateCreated > startDateTime && i.DateCreated < endDateTime)
                    .OrderByDescending(i => i.DateCreated).Select(i => new OPMFLog
                    {
                        Id = i.Id,
                        DateCreated = i.DateCreated,
                        ExceptionObject = i.ExceptionObject,
                        Message = i.Message,
                        Type = i.Type,
                        Variables = i.Variables,
                    }).ToList();
        }

        public List<OPMFLog> GetErrors(DateTime startDateTime, DateTime endDateTime)
        {
            return _liteCollection.Query().Where(i => i.Type == OPMFLogType.Error
                && i.DateCreated > startDateTime && i.DateCreated < endDateTime)
                    .OrderByDescending(i => i.DateCreated).Select(i => new OPMFLog
                    {
                        Id = i.Id,
                        DateCreated = i.DateCreated,
                        ExceptionObject = i.ExceptionObject,
                        Message = i.Message,
                        Type = i.Type,
                        Variables = i.Variables,
                    }).ToList();
        }
    }
}
