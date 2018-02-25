
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;

using KdQuoteLibrary.QuoteFlowHelper;

namespace KdQuoteLibrary.Services
{
    public class CacheServices
    {
        private static readonly CacheServices _cacheServices = new CacheServices();
        private ICacheManager _cacheManager;

        private CacheExpiration _expireEverySixtySecond = new CacheExpiration { Minute = "1" };
        private CacheExpiration _expireEveryThirtyMinute = new CacheExpiration { Minute = "30" };
        private CacheExpiration _expireEverySundayAtThree = new CacheExpiration { Hour = "3", DayOfWeek = "6" };
        private CacheExpiration _expireEveryDayAtSix = new CacheExpiration { Minute = "0", Hour = "6" };

        private CacheServices()
        {
            _cacheManager = CacheFactory.GetCacheManager();
        }

        public static CacheServices Instance
        {
            get
            {
                return _cacheServices;
            }
        }

        public CacheExpiration ExpireEverySixtySecond
        {
            get
            {
                return _expireEverySixtySecond;
            }
        }

        public CacheExpiration ExpireEveryThirtyMinute
        {
            get
            {
                return _expireEveryThirtyMinute;
            }
        }

        public CacheExpiration ExpireEverySundayAtThree
        {
            get
            {
                return _expireEverySundayAtThree;
            }
        }

        public CacheExpiration ExpireEveryDayAtSix
        {
            get
            {
                return _expireEveryDayAtSix;
            }
        }

        public void Add(String key, Object obj)
        {
            _cacheManager.Add(key, obj);
        }

        public void Add(String key, Object obj, CacheExpiration expiration)
        {
            _cacheManager.Add(key, obj, CacheItemPriority.Normal, null, new ExtendedFormatTime(expiration.ToString()));
        }

        public void Clear(String key)
        {
            _cacheManager.Remove(key);
        }

        public void ClearAll()
        {
            _cacheManager.Flush();
        }

        public Object GetData(String key)
        {
            return _cacheManager.GetData(key);
        }

        public Boolean Contains(String key)
        {
            return _cacheManager.Contains(key);
        }
    }
}
