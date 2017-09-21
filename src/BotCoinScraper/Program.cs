﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace BotCoinScraper
{
    class Program
    {

        private class Ticker
        {
            public decimal mid { get; set; }
            public decimal bid { get; set; }
            public decimal ask { get; set; }
            public decimal last_price { get; set; }
            public decimal low { get; set; }
            public decimal high { get; set; }
            public decimal volume { get; set; }
            public string timestamp { get; set; }
        }

        static void Main(string[] args)
        {

            while (true)
            {

                var client = new RestSharp.RestClient("https://api.bitfinex.com/v1");
                var request = new RestSharp.RestRequest("/pubticker/btcusd", RestSharp.Method.GET);
                var response = client.Execute<Ticker>(request);
                var data = response.Data;

                Console.WriteLine($"Price: {data.last_price}");

                var T = new BotCoinEvents.Events.TickEvent
                {
                    mid = data.mid,
                    bid = data.bid,
                    ask = data.ask,
                    last_price = data.last_price,
                    low = data.low,
                    high = data.high,
                    volume = data.volume
                };

                SaveEvent(T);

                for (var i = 30; i > 0; i--)
                {
                    if(i < 30 && i% 5 == 0) Console.WriteLine(i);
                    System.Threading.Thread.Sleep(1000);
                }

            }

        }

        private static void SaveEvent(BotCoinEvents.Events.IEvent e){

            var settings = ConnectionSettings.Create();
            settings.SetHeartbeatTimeout(TimeSpan.FromSeconds(60));

            using (var connection = EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, 1113)))
            {

                connection.ConnectAsync().Wait();

                var data = new EventData(
                    Guid.NewGuid(),
                    e.GetType().Name,
                    true,
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(
                            e, new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.None
                            }
                        )
                    ),
                    null
                );

                connection.AppendToStreamAsync("BotCoin-001001", ExpectedVersion.Any, data).Wait();

            }

        }

    }
}
