using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using LiteDB;
using Universal.Edge.Models;

namespace Universal.Edge.Repos
{
    public class FileStorageRepo
    {/*
        public IEnumerable<DiagramDto> Save(tempfile dto)
        {
            using (var db =
                new LiteDatabase(Path.Combine(ApplicationData.Current.LocalFolder.Path, "DB") + "Database.db"))
            {
                var collection = db.GetCollection<tempfile>(nameof(tempfile));

                var saved = collection.Upsert(dto);

                return collection.FindAll();
            }
        }*/
        public IEnumerable<tempfile> Save(tempfile dto)
        {
            var directoryPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DB");
            if (Directory.Exists(directoryPath) == false)
            {
                Directory.CreateDirectory(directoryPath);
            }

            var dbPath = Path.Combine(directoryPath, "Database.db");
            var cs = $"FileName=\"{dbPath}\";utc=true";
            using (var db =
                new LiteDatabase(cs))
            {
                var collection = db.GetCollection<tempfile>(nameof(tempfile));

                var saved = collection.Upsert(dto);

                return collection.FindAll();
            }
        }
    }

    public class tempfile
    {
        //public long _dt { get; set; }
        public long Id { get; set; }
        //[BsonIgnore]
        public DateTime DT
        {
            get;
            set;
            /*
            get { return DateTime.FromBinary(_dt); }
            set { _dt = value.ToBinary(); }*/
        }
    }
}