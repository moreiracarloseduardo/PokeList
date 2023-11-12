using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RecyclableScrollList_ : MonoBehaviour
{
    public GameObject cardPrefab;
    public int cardsPerPage = 20;
    public Transform contentPanel;
    public int currentOffset = 0;
    private List<Pokemon_> pokemons = new List<Pokemon_>();
    private bool isLoading = false;

    public bool IsLoading()
    {
        return isLoading;
    }

    void Start()
    {
        StartCoroutine(LoadPokemonData(currentOffset));
    }

    public IEnumerator LoadPokemonData(int offset)
    {
        isLoading = true;
        using (UnityWebRequest www = UnityWebRequest.Get("https://pokeapi.co/api/v2/pokemon?limit=" + cardsPerPage + "&offset=" + offset))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                PokemonList pokemonList = JsonUtility.FromJson<PokemonList>(www.downloadHandler.text);
                for (int i = 0; i < pokemonList.results.Length; i++)
                {
                    Pokemon_ pokemon = pokemonList.results[i];
                    // Make an additional API call to get the detailed Pokemon data
                    using (UnityWebRequest www2 = UnityWebRequest.Get(pokemon.url))
                    {
                        yield return www2.SendWebRequest();

                        if (www2.result != UnityWebRequest.Result.Success)
                        {
                            Debug.Log(www2.error);
                        }
                        else
                        {
                            Pokemon_ detailedPokemon = JsonUtility.FromJson<Pokemon_>(www2.downloadHandler.text);
                            detailedPokemon.url = pokemon.url; // Ensure the url property is set
                            CreateCard(detailedPokemon);
                        }
                    }
                }
                currentOffset += cardsPerPage; // Update the offset for the next page of data
            }
        }
        isLoading = false;
    }

    public void CreateCard(Pokemon_ pokemon)
    {
        GameObject card = Instantiate(cardPrefab, contentPanel);
        Card_ cardComponent = card.GetComponent<Card_>();
        cardComponent.SetData(pokemon);
    }
}

[System.Serializable]
public class PokemonList
{
    public Pokemon_[] results;
}