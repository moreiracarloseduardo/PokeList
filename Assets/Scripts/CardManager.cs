using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform contentTransform;
    public ScrollRect scrollRect;
    public Image backgroundImage;
    private PokemonAPIManager pokemonAPIManager;
    private int limit = 12;
    private int start = 1;
    private LRUCache<string, Texture2D> imageCache = new LRUCache<string, Texture2D>(100); // Set your desired capacity

    private void Start()
    {
        pokemonAPIManager = GetComponent<PokemonAPIManager>();
        StartCoroutine(pokemonAPIManager.GetPokemonData(start, limit, InitializeCards));
        scrollRect.onValueChanged.AddListener(OnScroll);
    }
    private void OnScroll(Vector2 scrollPosition)
    {
        if (scrollRect.verticalNormalizedPosition < 0.1f)
        {
            start += limit;
            StartCoroutine(pokemonAPIManager.GetPokemonData(start, limit, AddCards));
        }
    }

    private void AddCards(List<Pokemon> newPokemons)
    {
        foreach (Pokemon pokemon in newPokemons)
        {
            CreateCard(pokemon);
        }
    }
    private void InitializeCards(List<Pokemon> pokemons)
    {
        foreach (Pokemon pokemon in pokemons)
        {
            CreateCard(pokemon);
        }
    }

    private void CreateCard(Pokemon pokemon)
    {
        GameObject card = Instantiate(cardPrefab, contentTransform);
        Transform pokemonIconTransform = card.transform.Find("PokemonIcon");
        RawImage pokemonIcon = pokemonIconTransform.GetComponent<RawImage>();
        TextMeshProUGUI nameText = pokemonIconTransform.Find("Name").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI weightText = pokemonIconTransform.Find("Weight").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI orderText = pokemonIconTransform.Find("Order").GetComponent<TextMeshProUGUI>();

        StartCoroutine(LoadImage(pokemon.sprites.front_default, pokemonIcon));

        nameText.text = pokemon.name;
        weightText.text = $"Weight: {pokemon.weight}";
        orderText.text = $"Order: {pokemon.order}";
    }

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