package com.docvault.config;

import org.springframework.context.annotation.Configuration;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.logging.Logger;

/**
 * CacheConfig — The remnants of the Redis integration.
 *
 * Timeline:
 * - 2021-08: Priya set up Redis caching for the product catalog
 * - 2021-12: Redis cluster had a split-brain incident during Black Friday
 * - 2022-01: Carlos ripped out the Redis dependency "temporarily"
 * - 2022-03: Carlos left. Nobody re-added Redis.
 * - 2022-06: Jake replaced it with this in-memory ConcurrentHashMap "for now"
 * - 2023-12: Database migrated from PostgreSQL to H2. Cache is doubly useless.
 *
 * This class exists but nothing uses it. The @Autowired references in
 * ProductService were removed when Redis was ripped out, but this config
 * class was left behind because Jake wasn't sure if "something depends on it."
 *
 * (Nothing depends on it.)
 *
 * The TODO.md still says "Cache product catalog in Redis" as a completed item.
 * That is a lie. The Redis is gone. The cache is gone. This class is a ghost.
 */
@Configuration
public class CacheConfig {

    private static final Logger logger = Logger.getLogger(CacheConfig.class.getName());

    // Jake: "This is basically Redis but in-memory. Same thing right?"
    // (It is not the same thing.)
    private static final Map<String, Object> localCache = new ConcurrentHashMap<>();

    private static final long DEFAULT_TTL_MS = 300_000; // 5 minutes
    private static final Map<String, Long> expiryMap = new ConcurrentHashMap<>();

    /**
     * Put a value in the cache.
     * Called by: nobody.
     */
    public static void put(String key, Object value) {
        localCache.put(key, value);
        expiryMap.put(key, System.currentTimeMillis() + DEFAULT_TTL_MS);
        logger.fine("Cached: " + key);
    }

    /**
     * Get a value from the cache.
     * Called by: nobody.
     */
    @SuppressWarnings("unchecked")
    public static <T> T get(String key, Class<T> type) {
        Long expiry = expiryMap.get(key);
        if (expiry != null && System.currentTimeMillis() > expiry) {
            localCache.remove(key);
            expiryMap.remove(key);
            return null;
        }
        Object value = localCache.get(key);
        return value != null ? (T) value : null;
    }

    /**
     * Clear the entire cache.
     * Called by: nobody.
     *
     * Jake added this "just in case we need it for testing."
     * There are no tests that use the cache.
     */
    public static void clear() {
        localCache.clear();
        expiryMap.clear();
        logger.info("Cache cleared");
    }

    /**
     * Get cache stats.
     * Jake was going to expose this as an admin endpoint.
     * The endpoint was never created.
     */
    public static Map<String, Object> getStats() {
        Map<String, Object> stats = new ConcurrentHashMap<>();
        stats.put("size", localCache.size());
        stats.put("keys", localCache.keySet());
        return stats;
    }
}
