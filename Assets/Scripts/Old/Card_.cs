using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;

public class Card_ : MonoBehaviour
{
    public GameObject front;
    public RawImage pokemonImage; // Changed from Image to RawImage
    public ImageLoader_ imageLoader; // This will load the images
    public Pokemon_ Pokemon { get; private set; } // This will be set by the RecyclableScrollList_ class
    public Image cardBackground;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI weightText;
    private TextMeshProUGUI orderText;
    private void Awake()
    {
        nameText = front.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        weightText = front.transform.Find("Weight").GetComponent<TextMeshProUGUI>();
        orderText = front.transform.Find("Order").GetComponent<TextMeshProUGUI>();
    }

    public void SetData(Pokemon_ pokemon)
    {
        // ... existing checks ...

        nameText.text = pokemon.name;

        StringBuilder sb = new StringBuilder();
        sb.Append("Weight: ").Append(pokemon.weight);
        weightText.text = sb.ToString();
        sb.Clear();

        sb.Append("Order: ").Append(pokemon.order);
        orderText.text = sb.ToString();
        sb.Clear();

        StartCoroutine(imageLoader.LoadImage(pokemon.sprites.front_shiny, pokemonImage));
        this.Pokemon = pokemon;
        if (pokemon != null)
        {
            StartCoroutine(GetPokemonSpecies(pokemon));
        }
        else
        {
            Debug.LogError("Pokemon object is null.");
        }
    }
    IEnumerator GetPokemonSpecies(Pokemon_ pokemon)
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
                cardBackground.color = ConvertColor(species.color.name); // Set the background color
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
[System.Serializable]
public class PokemonSpecies
{
    public NamedAPIResource color;
}

[System.Serializable]
public class NamedAPIResource
{
    public string name;
}
