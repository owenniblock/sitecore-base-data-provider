namespace Kumquat.SAS.SC.Interfaces
{
    public interface ICacheHandler
    {
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
        T GetCachedItem<T>(string cacheKey,
          bool useSitecoreCache = true,
          bool globalCache = false,
          string siteName = "default",
          string databaseName = "master");

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
        void SaveCachedItem<T>(string cacheKey, object cachingData, object cacheTimer,
          bool isNoSlidingExpiration = true,
          bool useSitecoreCache = true,
          long cacheSize = 0,
          bool globalCache = false,
          bool removeOnPublish = true,
          string siteName = "default",
          string databaseName = "master",
          System.Web.Caching.CacheDependency cacheDep = null,
          System.Web.Caching.CacheItemPriority priority = System.Web.Caching.CacheItemPriority.Normal,
          System.Web.Caching.CacheItemRemovedCallback callback = null);

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
        bool RemoveCacheItem<T>(string cacheKey,
          bool useSitecoreCache = true,
          bool globalCache = false,
          string siteName = "default",
          string databaseName = "master");

        /// <summary>
        /// Clears the cache based on the details provided
        /// </summary>
        /// <param name="siteName">The name of the site to clear it's cached data</param>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="useSitecoreCache">Clear the cache from sitecore or the httpruntime</param>
        /// <param name="globalCache">Clear the global cache as well</param>
        /// <param name="removeOnPublish">Remove the data which was indicated as not to be cleared when publishing</param>
        void ClearCache(
          string siteName = "default",
          string databaseName = "master",
          bool useSitecoreCache = true,
          bool globalCache = false,
          bool removeOnPublish = false);
    }
}