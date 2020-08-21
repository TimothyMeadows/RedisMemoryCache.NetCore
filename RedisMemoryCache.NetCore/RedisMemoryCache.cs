using System;
using System.Text.Json;
using StackExchange.Redis;
using TimedMemoryCache.NetCore;

namespace RedisMemoryCache.NetCore
{
    public class RedisMemoryCache : IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly TimedMemoryCache.NetCore.TimedMemoryCache _cache;
        private readonly IDatabase _database;
        private readonly bool _synchronize;

        public RedisMemoryCache(string connectionString, int seconds = 300, bool synchronize = true)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();

            _cache = new TimedMemoryCache.NetCore.TimedMemoryCache(seconds);
            _cache.OnTimeout += OnTimeout;
            _synchronize = synchronize;
        }

        public dynamic this[string key]
        {
            get => _cache[key];
            set
            {
                _cache[key] = value;
                if (!_synchronize)
                    return;

                var serializable = ((Type)value.GetType()).IsSerializable;
                if (!serializable) 
                    return;

                var json = JsonSerializer.Serialize(value);
                _database.StringSet(new RedisKey(key), new RedisValue(json), null, When.Always, CommandFlags.FireAndForget);
            }
        }

        public void Write(string key, dynamic value, bool synchronize = true)
        {
            _cache.Write(key, value);
            if (!_synchronize)
                return;

            var serializable = ((Type)value.GetType()).IsSerializable;
            if (!serializable) 
                return;

            var json = JsonSerializer.Serialize(value);
            _database.StringSet(new RedisKey(key), new RedisValue(json), null, When.Always, CommandFlags.FireAndForget);
        }

        public void Write(string key, dynamic value, long timeout, bool synchronize = true)
        {
            _cache.Write(key, value, timeout);
            if (!_synchronize)
                return;

            var serializable = ((Type)value.GetType()).IsSerializable;
            if (!serializable) 
                return;

            var json = JsonSerializer.Serialize(value);
            _database.StringSet(new RedisKey(key), new RedisValue(json), null, When.Always, CommandFlags.FireAndForget);
        }

        public T Write<T>(string key, T value, bool synchronize = true)
        {
            var entry = _cache.Write<T>(key, value);
            if (!_synchronize)
                return entry;

            var serializable = value.GetType().IsSerializable;
            if (!serializable) 
                return entry;

            var json = JsonSerializer.Serialize(value);
            _database.StringSet(new RedisKey(key), new RedisValue(json), null, When.Always, CommandFlags.FireAndForget);

            return entry;
        }

        public T Write<T>(string key, T value, long timeout, bool synchronize = true)
        {
            var entry = _cache.Write<T>(key, value, timeout);
            if (!_synchronize)
                return entry;

            var serializable = value.GetType().IsSerializable;
            if (!serializable) 
                return entry;

            var json = JsonSerializer.Serialize(value);
            _database.StringSet(new RedisKey(key), new RedisValue(json), null, When.Always, CommandFlags.FireAndForget);

            return entry;
        }

        public T Read<T>(string key)
        {
            return _cache.Read<T>(key);
        }

        public void Delete(string key, bool synchronize = true)
        {
            _cache.Delete(key);

            if (synchronize)
                _database.KeyDelete(key, CommandFlags.FireAndForget);
        }

        public T Delete<T>(string key, bool synchronize = true)
        {
            var entry = _cache.Delete<T>(key);

            if (synchronize)
                _database.KeyDelete(key, CommandFlags.FireAndForget);

            return entry;
        }

        public void Dispose()
        {
            _redis?.Dispose();
            _cache?.Dispose();
        }

        private void OnTimeout(TimedMemoryCache.NetCore.TimedMemoryCache source, string key, dynamic value, long timeout)
        {
            var json = _database.StringGet(key);
            if (!json.HasValue)
                return;

            var redisValue = JsonSerializer.Deserialize<dynamic>(json.ToString());
            source.Write(key, redisValue, timeout);

            Console.WriteLine($"Refreshing '{key}'");
        }
    }
}
