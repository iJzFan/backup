using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;
using System.Linq;

namespace CHIS.Services
{
    public class MongoLogService
    {
        private readonly MongoLogContext _mongo;



        private static Dictionary<string, IEnumerable<string>> NlogLogLevel = new Dictionary<string, IEnumerable<string>>
        {
            { "fatal",  new string[]{"Fatal" } },
            { "error",  new string[]{"Fatal","Error" } },
            { "warn",   new string[]{ "Fatal", "Error", "Warn" } },
            { "info",   new string[]{"Fatal","Error","Warn","Info"}},
            { "debug",  new string[]{"Fatal","Error","Warn","Info","Debug"}},
            { "trace",  new string[]{ "Fatal", "Error", "Warn", "Info", "Debug", "Trace"}}
        };

        public MongoLogService(MongoLogContext mongo)
        {
            _mongo = mongo;
        }


        public (List<ChisLog> List, int Count) GetBy(string searchText = "", string levels = "", DateTime? from = null, DateTime? to = null, int index = 1, int pageSize = 20)
        {
            var root = _mongo.ChisLog.AsQueryable();

            if (from.HasValue && to.HasValue)
            {
                root = root?.Where(x => x.Date >= from && x.Date <= to);
            }

            if (!string.IsNullOrEmpty(levels))
            {
                var levelList = NlogLogLevel.GetValueOrDefault(levels.ToLower());

                root = root?.Where(x => levelList.Contains(x.Level));
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                root = root?.Where(x => x.Message.ToLower().Contains(searchText.ToLower()) ||
                                       x.RequstUrl.ToLower().Contains(searchText.ToLower()) ||
                                       x.User.ToLower().Contains(searchText.ToLower()));
            }

            var count = root?.Count();

            var list = root?.OrderByDescending(x => x.Date).Skip((index < 1 ? 0 : index - 1) * pageSize).Take(pageSize < 1 ? 20 : pageSize)
                .Select(x => new ChisLog { Id = x.Id, Date = x.Date, Level = x.Level, Message = x.Message, Logger = x.Logger, RequstIP = x.RequstIP, RequstUrl = x.RequstUrl, User = x.User })
                .ToList();

            return (list, count.Value);
        }


        public ChisLog Get(string id)
        {
            var model = _mongo.ChisLog.AsQueryable().FirstOrDefault(x => x.Id == id);

            return model;
        }
    }

    public class MongoLogContext
    {
        private readonly IMongoDatabase _database = null;

        private readonly MongoLog _conf;

        public MongoLogContext(IOptionsSnapshot<MongoLog> conf)
        {
            _conf = conf.Value;

            var client = new MongoClient(_conf.ConnectionString);

            if (client != null)
            {
                _database = client.GetDatabase(_conf.Database);
            }
        }

        public IMongoCollection<ChisLog> ChisLog
        {
            get
            {
                return _database.GetCollection<ChisLog>(_conf.Collection);
            }
        }
    }

    public class ChisLog
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }

        public string Level { get; set; }

        public string Logger { get; set; }

        public string Message { get; set; }

        public string RequstIP { get; set; }

        public string RequstUrl { get; set; }

        public string User { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public Dictionary<string, object> Exception { get; set; }
    }

    public class MongoLog
    {
        public string ConnectionString { get; set; }

        public string Database { get; set; }

        public string Collection { get; set; }
    }
}
