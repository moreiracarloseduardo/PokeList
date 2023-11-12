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
    public string url; // Add this field to store the URL
    public string color = ""; // Change to string to store color name


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

    public void SetColor(string newColor)
    {
        color = newColor;
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

                // Wait until GetPokemonSpecies has completed before adding the Pokemon to the list
                yield return StartCoroutine(GetPokemonSpecies(pokemon));

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

                // Set the color directly in the Pokemon object
                pokemon.SetColor(species.color.name);
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

}