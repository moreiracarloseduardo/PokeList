using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;


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
                pokemon.url = url; 

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
[Serializable]
public class PokemonSpecies
{
    public NamedAPIResource color;
}

[Serializable]
public class NamedAPIResource
{
    public string name;
}
[Serializable]
public class Pokemon
{
    public string name;
    public int weight;
    public int order;
    public PokemonSprites sprites;
    public string url; 
    public string color = ""; 

    public int Id
    {
        get
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("URL is null or empty.");
                return -1;
            }

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
