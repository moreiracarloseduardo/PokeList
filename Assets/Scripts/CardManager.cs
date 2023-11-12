using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform contentTransform;
    private PokemonAPIManager pokemonAPIManager;

    private void Start()
    {
        pokemonAPIManager = GetComponent<PokemonAPIManager>();
        StartCoroutine(pokemonAPIManager.GetPokemonData(10, InitializeCards));
    }

    private void InitializeCards(Pokemon[] pokemons)
    {
        foreach (Pokemon pokemon in pokemons)
        {
            GameObject card = Instantiate(cardPrefab, contentTransform);
            RawImage pokemonIcon = card.transform.Find("PokemonIcon").GetComponent<RawImage>();
            StartCoroutine(LoadImage(pokemon.sprites.front_default, pokemonIcon));
        }
    }

    private IEnumerator LoadImage(string url, RawImage image)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            image.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
}