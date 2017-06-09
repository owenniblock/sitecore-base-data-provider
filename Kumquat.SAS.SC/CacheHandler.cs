using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kumquat.SAS.SC
{
    using Kumquat.SAS.SC.Interfaces;

    public class CacheHandler : ICacheHandler
    {
        #region Private Constant
        /// <summary>
        /// Non publish key
        /// </summary>
        private const string NonPublishKey = "DontPublish-{0}";

        /// <summary>
        /// The base sitekey
        /// </summary>
        private const string SiteKey = "{0}-{1}-SiteCache";

        /// <summary>
        /// The global key
        /// </summary>
        private const string GlobalKey = "Global";
        #endregion

        #region Private Variables

        /// <summary>
        /// The caching collection singleton to hold the caching references
        /// </summary>
        private readonly Dictionary<string, Sitecore.Caching.Cache> _cacheCollection = new Dictionary<string, Sitecore.Caching.Cache>();

        /// <summary>
        /// Are the error logs enabled, default is false
        /// </summary>
        private readonly bool _errorLogsEnabled = Sitecore.Configuration.Settings.GetBoolSetting("Caching.Error.Logs.Enabled", false);
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the base key used for the both sitecore caching and the HttpRuntime cache
        /// </summary>
        /// <param name="isGlobal"></param>
        /// <param name="siteName"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        private string GetBaseKey(bool isGlobal = false, string siteName = "default", string databaseName = "master")
        {
            // the defaults
            string cacheKey = "";

            // the site name can be overridden
            if (string.IsNullOrEmpty(siteName))
            {
                siteName = Sitecore.Context.Site == null ? "NoSite" : Sitecore.Context.Site.Name;
            }

            // the database can be overridden
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = Sitecore.Context.Database == null ? "NoName" : Sitecore.Context.Database.Name;
            }

            // are we on the global cache
            if (isGlobal)
            {
                // set the global cache key
                cacheKey = string.Format(SiteKey, GlobalKey, databaseName);
            }
            else
            {
                // get the site root site
                cacheKey = string.Format(SiteKey, siteName, databaseName);
            }

            return cacheKey;
        }

        /// <summary>
        /// The easy way to fetch the cache in a locked way
        /// </summary>
        /// <param name="isGlobal">Is the</param>
        /// <param name="siteName">Name of the site.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>Returns the sitecore cache instance</returns>
        private Sitecore.Caching.Cache SitecoreCache(bool isGlobal = false, string siteName = "default", string databaseName = "master")
        {
            // sets the default cache key
            string cacheKey = GetBaseKey(isGlobal, siteName, databaseName);

            // caching reference
            Sitecore.Caching.Cache cache = null;

            // data found
            if (_cacheCollection.ContainsKey(cacheKey))
            {
                // fetch the cache from the collection
                cache = _cacheCollection[cacheKey];
            }
            else
            {
                // fetches from the settings, but has a default size
                cache = Sitecore.Caching.Cache.GetNamedInstance(cacheKey, Sitecore.StringUtil.ParseSizeString(Sitecore.Configuration.Settings.GetSetting("Caching.CacheSize", "100MB")));

                // add new reference to the singleton
                _cacheCollection.Add(cacheKey, cache);
            }

            // return our object
            return cache;
        }
        #endregion

        #region GetCachedItem
        /// <summary>
        /// Loads the cache from the page caching
        /// </summary>
        /// <typeparam name="T">The type of parameter to cast the list to</typeparam>
        /// <param name="cacheKey">The cache key we are looking for</param>
        /// <param name="useSitecoreCache">Do you want to use Sitecore cache or the HttpRuntime cache object</param>
        /// <param name="globalCache">Do we use the global cache</param>
        /// <param name="siteName">The sitename</param>
        /// <param name="databaseName">The database name</param>
        /// <returns>Returns the cached data or null</returns>
        public T GetCachedItem<T>(string cacheKey,
            bool useSitecoreCache = true,
            bool globalCache = false,
            string siteName = "default",
            string databaseName = "master")
        {
            // no key so return error
            if (string.IsNullOrEmpty(cacheKey))
            {
                return default(T);
            }

            // setup the default object we will use
            T cachedItem = default(T);

            // produce the valid key we want to use
            string key = cacheKey.ToLower();

            // what type of cache are we using
            if (useSitecoreCache)
            {
                // get the cache we are looking for
                Sitecore.Caching.Cache cache = SitecoreCache(globalCache, siteName, databaseName);

                if (cache != null)
                {
                    // make ure the system has the key before doing anything
                    if (cache.ContainsKey(key))
                    {
                        // get the data from the sitecore cache
                        cachedItem = (T)cache[key];
                    }
                    else
                    {
                        // the item might be a non publish key
                        key = string.Format(NonPublishKey, key).ToLower();

                        // get the data if found
                        if (cache.ContainsKey(key))
                        {
                            // get the data from the sitecore cache
                            cachedItem = (T)cache[key];
                        }
                    }
                }
            }
            else
            {
                // set the cache key
                string globalBaseKey = GetBaseKey(globalCache, siteName, databaseName);
                string cacheStartKey = globalBaseKey + key;

                // get the cache item from the http runtime
                cachedItem = (T)System.Web.HttpRuntime.Cache.Get(cacheStartKey.ToLower());

                // is empty try a non-published item
                if (cachedItem == null)
                {
                    // the item might be a non publish key
                    cacheStartKey = (globalBaseKey + string.Format(NonPublishKey, key)).ToLower();

                    // get the non publish clearing item
                    cachedItem = (T)System.Web.HttpRuntime.Cache.Get(cacheStartKey);
                }
            }

            /*
             * Wanted to do a dynamic shadow clone (have to work out how to cast to correct list)
            // make sure we have data first
            if (cachedItem != null)
            {
                // get the cached item type
                Type tType = cachedItem.GetType();

                // is the item generic
                if (tType.IsGenericType)
                {
                    // are we dealing with a list
                    if (cachedItem.GetType().GetGenericTypeDefinition().Equals(typeof(List<>)))
                    {
                        // make sure we have some data
                        if (cachedItem.GetType().GetGenericArguments().Count() > 0)
                        {
                            Type listItemType = cachedItem.GetType().GetGenericArguments()[0];

                            System.Collections.IList list = cacheCheck;

                        }

                    }
                }
            }*/

            // return the data or null
            return cachedItem;
        }
        #endregion

        #region SaveCachedItem
        /// <summary>
        /// Saves the list to the cache
        /// </summary>
        /// <typeparam name="T">The type of parameter that will be saved</typeparam>
        /// <param name="cacheKey">The unique key to save</param>
        /// <param name="cachingData">The data to cache</param>
        /// <param name="cacheTimer">The time we want to cache this data</param>
        /// <param name="isNoSlidingExpiration">Is the cacheTimer an Absolute Expiration (default) or a sliding expiration</param>
        /// <param name="useSitecoreCache">Do you want to use Sitecore cache or the HttpRuntime cache object</param>
        /// <param name="cacheSize">The size of the cache, this will fetch the size dynamically if not provided</param>
        /// <param name="globalCache">Is the data to be stored in the global cache or site specific cache</param>
        /// <param name="removeOnPublish">Remove the contents on a publish, this is defaulted as true</param>
        /// <param name="siteName">Force set the site name, if this is run from a scheduled task this should be set</param>
        /// <param name="databaseName">Force the database if this is run from a scheduled tasks, this should be set</param>
        /// <param name="cacheDep">Any caching dependencies for the cache. NOTE: Not valid for Sitecore Caching</param>
        /// <param name="priority">The priority of the cache. NOTE: Not valid for Sitecore Caching</param>
        /// <param name="callback">The call-back function of the cache. NOTE: Not valid for Sitecore Caching</param>
        public void SaveCachedItem<T>(string cacheKey, object cachingData, object cacheTimer,
            bool isNoSlidingExpiration = true,
            bool useSitecoreCache = true,
            long cacheSize = 0,
            bool globalCache = false,
            bool removeOnPublish = true,
            string siteName = "default",
            string databaseName = "master",
            System.Web.Caching.CacheDependency cacheDep = null,
            System.Web.Caching.CacheItemPriority priority = System.Web.Caching.CacheItemPriority.Normal,
            System.Web.Caching.CacheItemRemovedCallback callback = null)
        {
            // make sure we have data
            if (!string.IsNullOrEmpty(cacheKey)
                && cachingData != null)
            {
                // set the key so we can override it
                string key = cacheKey.ToLower();

                if (!removeOnPublish)
                {
                    key = string.Format(NonPublishKey, key).ToLower();
                }

                // setup defaults for caching types
                TimeSpan slidingCache = System.Web.Caching.Cache.NoSlidingExpiration;
                DateTime absoluteCache = System.Web.Caching.Cache.NoAbsoluteExpiration;

                // set the cache type
                if (isNoSlidingExpiration)
                {
                    // make sure it's right
                    if (cacheTimer.GetType().Equals(typeof(DateTime)))
                    {
                        absoluteCache = (DateTime)cacheTimer;
                    }
                    else
                    {
                        // we have an issue fix up
                        TimeSpan timeSpanCheck = (TimeSpan)cacheTimer;
                        absoluteCache = DateTime.UtcNow.Add(timeSpanCheck);
                    }
                }
                else
                {
                    // make sure it's right
                    if (cacheTimer.GetType().Equals(typeof(TimeSpan)))
                    {
                        slidingCache = (TimeSpan)cacheTimer;
                    }
                    else
                    {
                        // we have an issue fix up
                        DateTime dateCheck = (DateTime)cacheTimer;
                        slidingCache = dateCheck.Subtract(DateTime.UtcNow);
                    }
                }

                // what type of cache are we using
                if (useSitecoreCache)
                {
                    #region Sitecore Cache
                    Sitecore.Caching.Cache cache = SitecoreCache(globalCache, siteName, databaseName);

                    if (cache.ContainsKey(key))
                    {
                        // remove the key
                        cache.Remove(key);
                    }

                    // do we have to work out the cache size
                    if (cacheSize == 0)
                    {
                        // we need to do a binary serialization to get the objects size
                        try
                        {
                            System.IO.MemoryStream mem = new System.IO.MemoryStream();
                            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            binFormatter.Serialize(mem, cachingData);
                            cacheSize = mem.Length + 500; // increase just in case
                        }
                        catch (System.Exception ex)
                        {
                            // log and setup defaults
                            cacheSize = 1500; // default size we have made it bigger than normal just in case

                            // do we display the serialization error
                            if (_errorLogsEnabled)
                            {
                                Sitecore.Diagnostics.Log.Warn(string.Format("Cache - Size Serialize: '{0}'", key), ex, typeof(CacheHandler));
                            }

                            // is the type an IEnumerable
                            // get the cached item type
                            Type tType = cachingData.GetType();

                            // the type is a collection
                            if (typeof(System.Collections.ICollection).IsAssignableFrom(tType)
                                || typeof(ICollection<>).IsAssignableFrom(tType))
                            {
                                // we want to try and see if the item is a collection
                                try
                                {
                                    // set the data as ICollection so we can get the data
                                    System.Collections.ICollection iEnum = (cachingData as System.Collections.ICollection);

                                    // make sure it casts correctly
                                    if (iEnum != null)
                                    {
                                        // we need to set this as it will cause issues
                                        long fakeCacheSize = cacheSize;
                                        cacheSize = iEnum.Count * fakeCacheSize;
                                    }
                                }
                                catch (System.Exception exer)
                                {
                                    cacheSize = 5000; // at least set it bigger just in case

                                    // do we display the logs
                                    if (_errorLogsEnabled)
                                    {
                                        Sitecore.Diagnostics.Log.Warn(string.Format("Cache - Collection Count: '{0}'", key), exer, typeof(CacheHandler));
                                    }
                                }
                            }
                        }
                    }

                    // use the sitecore cache
                    cache.Add(key.ToLower(), cachingData, cacheSize, slidingCache, absoluteCache);
                    #endregion
                }
                else
                {
                    #region HttpRuntime Cache
                    // set the cache key
                    string cacheStartKey = GetBaseKey(globalCache, siteName, databaseName) + key;

                    // confirm the cache sliding
                    System.Web.HttpRuntime.Cache.Add(cacheStartKey.ToLower(), cachingData, cacheDep, absoluteCache, slidingCache, priority, callback);
                    #endregion
                }
            }
        }
        #endregion

        #region RemoveCacheItem
        /// <summary>
        /// Removes the required item from the cache
        /// </summary>
        /// <typeparam name="T">The type of object to retrun from the cache</typeparam>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="useSitecoreCache">Do we want to use the cache</param>
        /// <param name="globalCache">Are we using the global cache</param>
        /// <param name="siteName">The sitename</param>
        /// <param name="databaseName">The database name</param>
        /// <returns>Returns true if the data was removed from the cache or false if it wasnt or there was an error</returns>
        public bool RemoveCacheItem<T>(string cacheKey,
            bool useSitecoreCache = true,
            bool globalCache = false,
            string siteName = "default",
            string databaseName = "master")
        {
            // no key so return error
            if (string.IsNullOrEmpty(cacheKey))
            {
                return false;
            }

            // produce the valid key we want to use
            string key = cacheKey.ToLower();

            // what type of cache are we using
            if (useSitecoreCache)
            {
                // get the cache we are looking for
                Sitecore.Caching.Cache cache = SitecoreCache(globalCache, siteName, databaseName);

                if (cache != null)
                {
                    // make ure the system has the key before doing anything
                    if (cache.ContainsKey(key))
                    {
                        // remove the cached object
                        cache.Remove(key);
                    }
                    else
                    {
                        // the item might be a non publish key
                        key = string.Format(NonPublishKey, key).ToLower();

                        // get the data if found
                        if (cache.ContainsKey(key))
                        {
                            // remove the cached object
                            cache.Remove(key);
                        }
                    }
                }
            }
            else
            {
                // set the cache key
                string globalBaseKey = GetBaseKey(globalCache, siteName, databaseName);
                string cacheStartKey = globalBaseKey + key;

                // removes the cache key
                System.Web.HttpRuntime.Cache.Remove(cacheStartKey.ToLower());

                // remove the non publish key
                System.Web.HttpRuntime.Cache.Remove((globalBaseKey + string.Format(NonPublishKey, key)).ToLower());
            }

            // data removed
            return true;
        }
        #endregion

        #region ClearCache
        /// <summary>
        /// Clears the cache based on the details provided
        /// </summary>
        /// <param name="siteName">The name of the site to clear it's cached data</param>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="useSitecoreCache">Clear the cache from sitecore or the httpruntime</param>
        /// <param name="globalCache">Clear the global cache as well</param>
        /// <param name="removeOnPublish">Remove the data which was indicated as not to be cleared when publishing</param>
        public void ClearCache(
            string siteName = "default",
            string databaseName = "master",
            bool useSitecoreCache = true,
            bool globalCache = false,
            bool removeOnPublish = false)
        {
            // use this to get the key
            string baseCacheKey = GetBaseKey(globalCache, siteName, databaseName);

            // what type of caching are we dealing with
            if (useSitecoreCache)
            {
                #region Sitecore Cache
                // get the cache from sitecore
                Sitecore.Caching.Cache cache = SitecoreCache(globalCache, siteName, databaseName);

                // make sure we have the data
                if (cache != null)
                {
                    // process the keys
                    foreach (object key in cache.GetCacheKeys())
                    {
                        // can we remove the item
                        if (removeOnPublish || !((string)key).Contains("DontPublish-"))
                        {
                            // remove the key
                            cache.Remove(key);
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region HttpRuntime
                /*// is empty try a non-published item
                if (cachedItem == null)
                {
                    // the item might be a non publish key
                    cacheStartKey = globalBaseKey + string.Format(NonPublishKey, key).ToLower();

                    // get the non publish clearing item
                    cachedItem = (T)System.Web.HttpRuntime.Cache.Get(cacheStartKey.ToLower());
                }
                System.Web.HttpRuntime.Cache*/
                #endregion
            }
        }
        #endregion
    }
}
