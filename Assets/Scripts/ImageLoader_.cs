using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageLoader_ : MonoBehaviour
{
    private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
    public IEnumerator LoadImage(string url, RawImage imageComponent)
    {
        Texture2D texture = null;

        if (imageCache.ContainsKey(url))
        {
            // If the image is in the cache, use it
            texture = imageCache[url];
        }
        else
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
                    imageCache[url] = texture;
                }
            }
        }

        if (texture != null) // Check if texture is not null before setting the image
        {
            imageComponent.texture = texture;
        }
    }
}
