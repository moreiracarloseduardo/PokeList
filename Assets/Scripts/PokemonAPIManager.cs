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
}

[Serializable]
public class PokemonSprites
{
    public string front_default;
}

public class PokemonAPIManager : MonoBehaviour
{
    private string baseURL = "https://pokeapi.co/api/v2/pokemon/";

    public IEnumerator GetPokemonData(int start, int limit, Action<List<Pokemon>> callback)
    {
        List<Pokemon> pokemons = new List<Pokemon>();
        for (int i = start; i < start + limit && i <= 898; i++)
        {
            UnityWebRequest www = UnityWebRequest.Get(baseURL + i);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Pokemon pokemon = JsonUtility.FromJson<Pokemon>(www.downloadHandler.text);
                pokemons.Add(pokemon);
            }
        }

        callback(pokemons);
    }
}