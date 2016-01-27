﻿using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Targets
{
    public class KafkaClient : IDisposable
    {
        Producer _client;
        Consumer _consumer;
        public KafkaClient(List<Uri> kafkaOptions)
        {
            var options = new KafkaOptions(kafkaOptions.ToArray());
            var router = new BrokerRouter(options);
            var r = router.GetTopicMetadata(new string[] { "storm_trooper" });
            _client = new Producer(router);
            _client.BatchSize = 1;
            var coption = new ConsumerOptions("storm_trooper", router);
            coption.ConsumerBufferSize = 1;
            coption.MaxWaitTimeForMinimumBytes = TimeSpan.FromMilliseconds(10);
            _consumer = new Consumer(coption);
            
        }

        public KafkaClient()
            : this(new List<Uri> { 
                new Uri("http://10.96.23.75:9092"), 
                new Uri("http://10.96.23.76:9092"),
                new Uri("http://10.96.23.77:9092"),
                new Uri("http://10.96.23.78:9092")})
        { }

        public void Post(string message)
        {
            try
            {
                var queueMessage = new Message(message, Guid.NewGuid().ToString());
                var results = 
                    _client.SendMessageAsync("storm_trooper", new[] { queueMessage },1);
                results.Wait();
                //_client.BatchSize
            }
            catch (KafkaApplicationException ex)
            {
                var strErrorMessage = ex.Message;
            }
            catch(Exception ex)
            {
                var m = ex.Message;
            }
        }

        public IEnumerable<string> Recieve()
        {
            foreach (var message in _consumer.Consume())
            {
                yield return string.Format("Response: P{0},O{1} : {2}",
                    message.Meta.PartitionId, message.Meta.Offset, 
                    System.Text.Encoding.Default.GetString(message.Value));
            }
        }

        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}