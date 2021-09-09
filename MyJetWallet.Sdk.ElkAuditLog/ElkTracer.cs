using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using MyJetWallet.Sdk.Service;
using Nest;

namespace MyJetWallet.Sdk.ElkAuditLog
{
    public class ElkTracer : IElkTracer
    {
        private bool _isInit;
        private ElasticClient Client { get; set; }
        private string IndexName { get; set; }
        private string Source { get; set; }
        
        public void Init(LogElkSettings config, string name, string source)
        {
            var nodes = config.Urls.Values.Select(e => new Uri(e)).ToArray();
            var pool = new StaticConnectionPool(nodes);
            var settings = new ConnectionSettings(pool);

            settings.BasicAuthentication(config.User, config.Password);
            settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            settings.DefaultIndex(name);
            
            Client = new ElasticClient(settings);

            IndexName = name;
            Source = source;
            
            _isInit = true;
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
                } }
            };
            
            var x = new DocumentPath<object>(id).Index(index);
            
            Client.Update<object, object>(x, u => u
                .Doc(entityToLog).DocAsUpsert().Index(index)
            );
        }

        public object GetFromTodayIndex(string id)
        {
            var index = $"{IndexName}-{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month}-{DateTime.UtcNow.Day}";
            var x = new DocumentPath<object>(id).Index(index);
            
            var getResponse2 = Client.Get<object>(x);
            return getResponse2.Source;
        }
    }
}