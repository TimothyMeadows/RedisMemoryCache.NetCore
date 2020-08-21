using System;
using System.Collections.Generic;
using System.IO;

namespace RedisMemoryCache.NetCore.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = File.ReadAllText("secret.txt");

            var cache = new RedisMemoryCache(connectionString, 3)
            {
                ["caw"] = "caw caw caw", 
                ["caw2"] = new KeyValuePair<string, string>("caw", "caw caw caw")
            };

            Console.ReadKey();
        }
    }
}
