using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pokemon_
{
    public string name;
    public string url;
    public int weight;
    public int order;
    public Sprites sprites;
    public int Id
    {
        get
        {
            // Check if url is null
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("URL is null or empty.");
                return -1;
            }

            // Extract the ID from the URL
            string idStr = url.Replace("https://pokeapi.co/api/v2/pokemon/", "").TrimEnd('/');
            int id;
            if (int.TryParse(idStr, out id))
            {
                return id;
            }
            else
            {
                Debug.LogError("Failed to parse ID from URL: " + url);
                return -1;
            }
        }
    }
}
[System.Serializable]
public class Sprites
{
    public string front_shiny;
}