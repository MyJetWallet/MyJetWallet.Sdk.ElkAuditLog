using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MyJetWallet.Sdk.ElkAuditLog.Test
{
    public class Tests
    {
        [Test]
        public void Test1()
        {
            var logger = new ElkTracer();
            var _loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));
            var _logger = _loggerFactory.CreateLogger<Tests>();
            
             // INITIALIZE
            var index = "testindex";
            logger.Init(_logger, new LogElkSettings()
            {
                Urls = new Dictionary<string, string>()
                {
                    {"1", "https://52.166.232.97:9201"},
                    {"2", "https://52.166.232.97:9202"},
                    {"3", "https://52.166.232.97:9203"}
                }, 
                Password = "yXALKCBr8p4Eq7qj",
                User = "spot"
            }, index, "testApp");

            
            
            
            // FIRST QUERY
            var id = DateTime.UtcNow.Millisecond.ToString();
            
            var y = new
            {
                field1 = "x",
                field2 = 10
            };

            logger.Trace(id, y);

            Thread.Sleep(2000);
            var result = logger.GetFromTodayIndex(id);
            Console.WriteLine(JsonConvert.SerializeObject(result));
            
            
            
            
            // SECOND QUERY
            var z = new
            {
                field3 = "y",
                field4 = 40
            };
            
            logger.Trace(id, z, "testPref");

            Thread.Sleep(2000);
            result = logger.GetFromTodayIndex(id);
            Console.WriteLine(JsonConvert.SerializeObject(result));
            
            
            
            
            // OTHER APP 
            logger.Init(_logger, new LogElkSettings()
            {
                Urls = new Dictionary<string, string>()
                {
                    {"1", "https://52.166.232.97:9201"},
                    {"2", "https://52.166.232.97:9202"},
                    {"3", "https://52.166.232.97:9203"}
                }, 
                Password = "yXALKCBr8p4Eq7qj",
                User = "spot"
            }, index, "otherApp");
            
            // THIRD QUERY
            var m = new
            {
                field5 = "u",
                field6 = 70
            };
            
            logger.Trace(id, m);

            Thread.Sleep(2000);
            result = logger.GetFromTodayIndex(id);
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}