using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;


[Serializable]
public class Pokemon
{
    public string name;
    public int weight;
    public int order;
    public PokemonSprites sprites;
    public Color color;
    public string url; // Add this field to store the URL

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

[Serializable]
public class PokemonSprites
{
    public string front_default;
}

public class PokemonAPIManager : MonoBehaviour
{
    public CardManager cardManager;
    private string baseURL = "https://pokeapi.co/api/v2/pokemon/";

    public IEnumerator GetPokemonData(int start, int limit, Action<List<Pokemon>> callback)
    {
        List<Pokemon> pokemons = new List<Pokemon>();
        for (int i = start; i < start + limit && i <= 898; i++)
        {
            string url = baseURL + i;
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Pokemon pokemon = JsonUtility.FromJson<Pokemon>(www.downloadHandler.text);
                pokemon.url = url; // Set the URL manually
                StartCoroutine(GetPokemonSpecies(pokemon));
                pokemons.Add(pokemon);
            }
        }

        callback(pokemons);
    }
    IEnumerator GetPokemonSpecies(Pokemon pokemon)
    {
        if (pokemon != null)
        {
            string url = $"https://pokeapi.co/api/v2/pokemon-species/{pokemon.Id}/";
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                PokemonSpecies species = JsonUtility.FromJson<PokemonSpecies>(json);
                cardManager.backgroundImage.color = ConvertColor(species.color.name); // Set the background color
            }
            else
            {
                Debug.LogError("Failed to fetch species data: " + request.error);
            }
        }
        else
        {
            Debug.LogError("Pokemon object is null.");
        }
    }
    Color ConvertColor(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "black":
                return new Color(0.4f, 0.4f, 0.4f); // Lighter black
            case "blue":
                return new Color(0.6f, 0.6f, 1.0f); // Pastel blue
            case "brown":
                return new Color(0.76f, 0.6f, 0.42f); // Pastel brown
            case "gray":
                return new Color(0.8f, 0.8f, 0.8f); // Lighter gray
            case "green":
                return new Color(0.6f, 1.0f, 0.6f); // Pastel green
            case "pink":
                return new Color(1.0f, 0.85f, 0.9f); // Pastel pink
            case "purple":
                return new Color(0.8f, 0.6f, 1.0f); // Pastel purple
            case "red":
                return new Color(1.0f, 0.6f, 0.6f); // Pastel red
            case "white":
                return Color.white;
            case "yellow":
                return new Color(1.0f, 1.0f, 0.6f); // Pastel yellow
            default:
                Debug.LogWarning("Unknown color name: " + colorName);
                return Color.white; // Default color
        }
    }
}