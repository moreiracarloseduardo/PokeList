using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class Card_ : MonoBehaviour
{
    public static event Action<int> BecameVisible = delegate { };
    public GameObject front;
    public RawImage pokemonImage; // Changed from Image to RawImage
    public ImageLoader_ imageLoader; // This will load the images
    public Pokemon_ Pokemon { get; private set; } // This will be set by the RecyclableScrollList_ class
    public Image cardBackground;

    public void SetData(Pokemon_ pokemon)
    {
        if (pokemon == null)
        {
            Debug.LogError("Pokemon object is null.");
            return;
        }
        if (imageLoader == null || pokemonImage == null)
        {
            Debug.LogError("'imageLoader' or 'pokemonImage' is null.");
            return;
        }
        if (front == null)
        {
            Debug.LogError("Front GameObject is not assigned in the Unity editor.");
            return;
        }

        Transform nameTransform = front.transform.Find("Name");
        if (nameTransform == null)
        {
            Debug.LogError("Front GameObject does not have a child GameObject named 'Name'.");
            return;
        }

        TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
        if (nameText == null)
        {
            Debug.LogError("'Name' GameObject does not have a TextMeshProUGUI component.");
            return;
        }

        Transform weightTransform = front.transform.Find("Weight");
        if (weightTransform == null)
        {
            Debug.LogError("Front GameObject does not have a child GameObject named 'Weight'.");
            return;
        }

        TextMeshProUGUI weightText = weightTransform.GetComponent<TextMeshProUGUI>();
        if (weightText == null)
        {
            Debug.LogError("'Weight' GameObject does not have a TextMeshProUGUI component.");
            return;
        }

        Transform orderTransform = front.transform.Find("Order");
        if (orderTransform == null)
        {
            Debug.LogError("Front GameObject does not have a child GameObject named 'Order'.");
            return;
        }

        TextMeshProUGUI orderText = orderTransform.GetComponent<TextMeshProUGUI>();
        if (orderText == null)
        {
            Debug.LogError("'Order' GameObject does not have a TextMeshProUGUI component.");
            return;
        }

        // pokemonImage = front.transform.Find("Image").GetComponent<Image>();
        if (pokemonImage == null)
        {
            Debug.LogError("'Image' GameObject does not have an Image component.");
            return;
        }

        // Check if 'pokemon' or 'pokemon.sprites' is null
        if (pokemon == null || pokemon.sprites == null || string.IsNullOrEmpty(pokemon.sprites.front_shiny))
        {
            Debug.LogError("Pokemon object or sprites are null or front_shiny is empty.");
            return;
        }
        if (pokemon.sprites == null || string.IsNullOrEmpty(pokemon.sprites.front_shiny))
        {
            Debug.LogError("Pokemon sprites are null or front_shiny is empty.");
            return;
        }

        nameText.text = pokemon.name;
        weightText.text = "Weight: " + pokemon.weight.ToString();
        orderText.text = "Order: " + pokemon.order.ToString();

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
    // This method is called when the renderer became visible by any camera
    private void OnBecameVisible()
    {
        // Trigger the BecameVisible event and pass the index of this card
        BecameVisible(Pokemon.Id);
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
