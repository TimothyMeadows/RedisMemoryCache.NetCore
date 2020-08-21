using System;
using System.Collections.Generic;
using System.IO;

namespace RedisMemoryCache.NetCore.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            // You can populate the cache at initialization time, it will inherit the "default" timeout, and the default "expires" set in the constructor.
            var connectionString = File.ReadAllText("secret.txt");
            var cache = new RedisMemoryCache(connectionString, 3, TimeSpan.FromSeconds(30))
            {
                ["caw"] = "caw caw caw", 
                ["caw2"] = new KeyValuePair<string, string>("caw", "caw caw caw")
            };

            // You can also access an entry directly by key. However you will need to cast from dynamic using this method.
            var caw1 = (string)cache["caw"];

            // You can set an entry directly. It will inherit the "default" timeout, and the default "expires" set in the constructor.
            cache["caw3"] = "third caw!";

            // You can still set non-serializable values in memory, however, they will NOT be stored in Redis, and will inherit the "default" timeout, and the default "expires" set in the constructor.
            // You can override the timeout by setting it to 0 in which case it will remain in memory until .Dispose
            cache.Write("caw4", new MemoryStream(new byte[] {32, 34}), 0);

            // You can set the timeout, and expires directly using the Write method.
            cache.Write("caw5", true, 10, TimeSpan.FromMinutes(2));


            // When cache entries expire they will first be checked in redis, if they exist, they will be re-added with the same timeout they were set with.
            // If they do not exist in redis they will be removed, finally if redis times out during a read, the previous value will be used but considered "stale"
            cache.OnTimeout += (source, key, value, timeout, stale, removed) =>
            {
                Console.WriteLine($"{key}: stale={stale} removed={removed}");
            };

            Console.ReadKey();
        }
    }
}
