using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageLoader_ : MonoBehaviour
{

    private LRUCache<string, Texture2D> imageCache = new LRUCache<string, Texture2D>(100); // Set your desired capacity

    public IEnumerator LoadImage(string url, RawImage imageComponent)
    {
        Texture2D texture = imageCache.Get(url);

        if (texture == null)
        {
            // If the image is not in the cache, load it
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                    // Add the image to the cache
                    imageCache.Add(url, texture);
                }
            }
        }

        if (texture != null) // Check if texture is not null before setting the image
        {
            imageComponent.texture = texture;
        }
    }
}
public class LRUCache<TKey, TValue>
{
    private readonly int capacity;
    private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cache;
    private readonly LinkedList<CacheItem> lruList;

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
        this.cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
        this.lruList = new LinkedList<CacheItem>();
    }

    public TValue Get(TKey key)
    {
        if (cache.TryGetValue(key, out LinkedListNode<CacheItem> node))
        {
            TValue value = node.Value.Value;
            lruList.Remove(node);
            lruList.AddLast(node);
            return value;
        }
        return default(TValue);
    }

    public void Add(TKey key, TValue value)
    {
        if (cache.Count >= capacity)
        {
            RemoveFirst();
        }

        CacheItem cacheItem = new CacheItem { Key = key, Value = value };
        LinkedListNode<CacheItem> node = new LinkedListNode<CacheItem>(cacheItem);
        lruList.AddLast(node);
        cache.Add(key, node);
    }

    private void RemoveFirst()
    {
        LinkedListNode<CacheItem> node = lruList.First;
        lruList.RemoveFirst();
        cache.Remove(node.Value.Key);
    }

    private class CacheItem
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}