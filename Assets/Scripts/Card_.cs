using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Card_ : MonoBehaviour
{
    public GameObject front;
    public RawImage pokemonImage; // Changed from Image to RawImage
    public ImageLoader_ imageLoader; // This will load the images
    public Pokemon_ Pokemon { get; private set; } // This will be set by the RecyclableScrollList_ class
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
        if (pokemon == null || pokemon.sprites == null || string.IsNullOrEmpty(pokemon.sprites.front_default))
        {
            Debug.LogError("Pokemon object or sprites are null or front_default is empty.");
            return;
        }
        if (pokemon.sprites == null || string.IsNullOrEmpty(pokemon.sprites.front_default))
        {
            Debug.LogError("Pokemon sprites are null or front_default is empty.");
            return;
        }

        nameText.text = pokemon.name;
        weightText.text = "Weight: " + pokemon.weight.ToString();
        orderText.text = "Order: " + pokemon.order.ToString();

        StartCoroutine(imageLoader.LoadImage(pokemon.sprites.front_default, pokemonImage));
        this.Pokemon = pokemon;
    }
}
