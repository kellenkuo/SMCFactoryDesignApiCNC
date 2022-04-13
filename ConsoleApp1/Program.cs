using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Configuration;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[ INFO ] Program start ... [ OK ]");
            PingRedis();

            string globalIP = "192.168.10.201";
            DeltaCNC deltaCNC = new DeltaCNC(globalIP);
            //MitsubishiCNC mitsubishiCNC = new MitsubishiCNC(globalIP);
            FanucCNC fanucCNC = new FanucCNC("192.168.10.12");

            JObject deltaData = deltaCNC.Get();
            //JObject mitsubishiData = mitsubishiCNC.Get();
            JObject mitsubishiData = new JObject();
            JObject fanucData = fanucCNC.Get();

            PrettyPrint(deltaData, mitsubishiData, fanucData);

            Console.WriteLine("press any key to continue ...");
            Console.ReadKey();
        }

        public static bool PingRedis()
        {
            RedisConnectionFactory.Connection.GetDatabase();
            var connectionString = ConfigurationSettings.AppSettings["RedisConnection"];
            Console.WriteLine($"[ INFO ] {connectionString} Redis Cache connected ... [ OK ]");
            return true;
        }

        public static JObject GetCacheData(string key)
        {
            var redis = RedisConnectionFactory.Connection.GetDatabase();
            string data = "";
            if (redis.KeyExists(key))
                data = redis.StringGet(key);

            return JObject.Parse(data);
        }

        public static string SetCacheData(string key, JObject json)
        {
            var redis = RedisConnectionFactory.Connection.GetDatabase();
            var value = json.ToString(Newtonsoft.Json.Formatting.None);
            redis.StringSet(key, value);
            
            return "ok";
        }

        public static class RedisConnectionFactory
        {
            static RedisConnectionFactory()
            {
                var connectionString = ConfigurationSettings.AppSettings["RedisConnection"];
                var options = ConfigurationOptions.Parse(connectionString);
                RedisConnectionFactory.lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                {
                    try
                    {
                        return ConnectionMultiplexer.Connect(options);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ FAIL ] {connectionString} Redis Cache connection unreachable ... [ ERROR ]");
                        System.Environment.Exit(1);
                    }
                    return null;
                });
            }

            private static Lazy<ConnectionMultiplexer> lazyConnection;
            public static ConnectionMultiplexer Connection
            {
                get
                {
                    return lazyConnection.Value;
                }
            }
        }

        public static JObject PrettyPrint(JObject deltaData, JObject mitsubishiData, JObject fanucData)
        {
            JObject mergeData = new JObject();
            foreach (var item in deltaData)
            {
                if (!mergeData.ContainsKey(item.Key))
                    mergeData.Add(item.Key, new JObject());
                mergeData[item.Key]["delta"] = item.Value;
            }
            foreach (var item in mitsubishiData)
            {
                if (!mergeData.ContainsKey(item.Key))
                    mergeData.Add(item.Key, new JObject());
                mergeData[item.Key]["mitsubishi"] = item.Value;
            }
            foreach (var item in fanucData)
            {
                if (!mergeData.ContainsKey(item.Key))
                    mergeData.Add(item.Key, new JObject());
                mergeData[item.Key]["fanuc"] = item.Value;
            }

            Console.WriteLine("+--------------------+--------------------+--------------------+--------------------+");
            Console.WriteLine("|                    |        Delta       |     Mitsubishi     |        Fanuc       |");
            foreach (var item in mergeData)
            {
                string title = String.Format("{0,20}", item.Key).Substring(0, 20);
                string value1 = String.Format("{0,20}", item.Value["delta"]).Substring(0, 20);
                string value2 = String.Format("{0,20}", item.Value["mitsubishi"]).Substring(0, 20);
                string value3 = String.Format("{0,20}", item.Value["fanuc"]).Substring(0, 20);
                Console.WriteLine("+--------------------+--------------------+--------------------+--------------------+");
                Console.WriteLine($"|{title}|{value1}|{value2}|{value3}|");
            }
            Console.WriteLine("+--------------------+--------------------+--------------------+--------------------+");

            return mergeData;
        }
    }
}
