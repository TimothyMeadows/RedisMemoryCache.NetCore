using System;
using System.Collections.Generic;
using System.Text;

namespace RedisMemoryCache.NetCore
{
    public delegate void TimeoutCallback(TimedMemoryCache.NetCore.TimedMemoryCache source, string key, dynamic value, long timeout, bool stale, bool removed);
}
