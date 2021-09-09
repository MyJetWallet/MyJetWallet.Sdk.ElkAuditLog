using System;
using System.Collections.Generic;
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

            
             // INITIALIZE
            var index = "testindex";
            logger.Init(new LogElkSettings()
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

            var result = logger.GetFromTodayIndex(id);
            Console.WriteLine(JsonConvert.SerializeObject(result));
            
            
            
            
            // SECOND QUERY
            var z = new
            {
                field3 = "y",
                field4 = 40
            };
            
            logger.Trace(id, z, "testPref");

            result = logger.GetFromTodayIndex(id);
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}