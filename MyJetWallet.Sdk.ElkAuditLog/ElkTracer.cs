using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.Tools;
using Nest;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MyJetWallet.Sdk.ElkAuditLog
{
    public class ElkTracer : IElkTracer, IDisposable
    {
        private MyTaskTimer _timer;
        
        private bool _isInit;
        private ElasticClient Client { get; set; }
        private string IndexName { get; set; }
        private string Source { get; set; }

        private ILogger _logger;

        private readonly List<TraceBufferItem> _traceBuffer = new();
        private readonly object _locker = new();
        
        public void Init(ILogger logger, LogElkSettings config, string indexName, string source)
        {
            _logger = logger;
            _timer = new MyTaskTimer(nameof(ElkTracer), TimeSpan.FromSeconds(1), _logger, DoTime);
            IndexName = indexName;
            Source = source;
            
            var nodes = config.Urls.Values.Select(e => new Uri(e)).ToArray();
            var pool = new StaticConnectionPool(nodes);
            var settings = new ConnectionSettings(pool);

            settings.BasicAuthentication(config.User, config.Password);
            settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            settings.DefaultIndex(indexName);
            Client = new ElasticClient(settings);
            
            _isInit = true;
            _timer.Start();
        }

        private async Task DoTime()
        {
            await SaveToElk();
        }

        private async Task SaveToElk()
        {
            if (!_isInit)
            {
                return;
            }
            try
            {
                var entitiesForBulk = new List<TraceBufferItem>();
                lock (_locker)
                {
                    entitiesForBulk.AddRange(_traceBuffer);
                    _traceBuffer.Clear();
                }
                foreach (var index in entitiesForBulk.Select(e => e.Index).Distinct())
                {
                    var entities = entitiesForBulk
                        .Where(e => e.Index == index)
                        .Select(e => e.Entity);

                    var bulkResponse = await Client.BulkAsync(b => b
                        .Index(index)
                        .UpdateMany(entities, (bu, d) => bu.Doc(d).DocAsUpsert()
                            .Id(d.TryGetValue("id", out var result) ? result.ToString() : string.Empty))
                    );
                    _logger.LogInformation($"SaveToElk has bulk response: {JsonConvert.SerializeObject(bulkResponse)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SaveToElk has exception: {ex.Message}");
            }
        }

        public void Trace(string id, object entity, string prefix = "_")
        {
            if (!_isInit)
            {
                throw new Exception($"{nameof(IElkTracer)} is not init.");
            }
            var index = $"{IndexName}-{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month}-{DateTime.UtcNow.Day}";
            var entityToLog = new Dictionary<string, object>()
            {
                {"id", id },
                {"timestamp", DateTime.UtcNow},
                {Source, new Dictionary<string, object>()
                {
                    {prefix, entity}
                }}
            };
            lock (_locker)
            {
                _traceBuffer.Add(new TraceBufferItem()
                {
                    Index = index,
                    Id = id,
                    Entity = entityToLog
                });
            }
        }

        public object GetFromTodayIndex(string id)
        {
            var index = $"{IndexName}-{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month}-{DateTime.UtcNow.Day}";
            var documentPath = new DocumentPath<object>(id).Index(index);
            
            var getResponse2 = Client.Get<object>(documentPath);
            return getResponse2.Source;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            SaveToElk().GetAwaiter().GetResult();
        }
    }
}