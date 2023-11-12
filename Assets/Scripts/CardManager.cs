using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform contentTransform;
    private PokemonAPIManager pokemonAPIManager;

    private void Start()
    {
        pokemonAPIManager = GetComponent<PokemonAPIManager>();
        StartCoroutine(pokemonAPIManager.GetPokemonData(30, InitializeCards));
    }

    private void InitializeCards(Pokemon[] pokemons)
    {
        foreach (Pokemon pokemon in pokemons)
        {
            GameObject card = Instantiate(cardPrefab, contentTransform);
            Transform pokemonIconTransform = card.transform.Find("PokemonIcon");
            RawImage pokemonIcon = pokemonIconTransform.GetComponent<RawImage>();
            TextMeshProUGUI nameText = pokemonIconTransform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI weightText = pokemonIconTransform.Find("Weight").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI orderText = pokemonIconTransform.Find("Order").GetComponent<TextMeshProUGUI>();

            StartCoroutine(LoadImage(pokemon.sprites.front_default, pokemonIcon));

            nameText.text = pokemon.name;
            weightText.text = "Weight: " + pokemon.weight.ToString();
            orderText.text = "Order: " + pokemon.order.ToString();
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