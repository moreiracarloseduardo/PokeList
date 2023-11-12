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

        if (!string.IsNullOrWhiteSpace(pokemon.color))
        {
            // Set background color based on species color
            backgroundImage.color = ConvertColor(pokemon.color);
        }
        else
        {
            Debug.LogWarning("Pokemon color is null or empty.");
            // Decida o que fazer neste caso. Por exemplo, você pode usar uma cor padrão.
            backgroundImage.color = Color.white; // ou qualquer outra cor padrão desejada
        }
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
    Color ConvertColor(string colorName)
    {
        if (string.IsNullOrEmpty(colorName))
        {
            Debug.LogWarning("Color name is null or empty.");
            return Color.white; // ou qualquer outra cor padrão desejada
        }

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