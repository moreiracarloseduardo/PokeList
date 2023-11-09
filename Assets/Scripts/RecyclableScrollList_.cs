using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RecyclableScrollList_ : MonoBehaviour
{
    public GameObject cardPrefab;
    public int numberOfCards;
    private List<GameObject> cards = new List<GameObject>();
    private List<Pokemon_> pokemons = new List<Pokemon_>();


    void Start()
    {
        StartCoroutine(LoadPokemonData());
        RectTransform contentRectTransform = GetComponent<RectTransform>();
        GridLayoutGroup gridLayoutGroup = GetComponent<GridLayoutGroup>();

        float totalCardHeight = gridLayoutGroup.cellSize.y; // Use the cell size of the GridLayoutGroup to get the card height
        float spacing = gridLayoutGroup.spacing.y;
        int numberOfColumns = gridLayoutGroup.constraintCount;

        int numberOfRows = Mathf.CeilToInt((float)numberOfCards / numberOfColumns);
        float totalHeight = numberOfRows * totalCardHeight + (numberOfRows - 1) * spacing;

        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);
        for (int i = 0; i < numberOfCards; i++)
        {
            GameObject card = Instantiate(cardPrefab) as GameObject;
            card.transform.SetParent(transform);
            cards.Add(card);
        }
    }
    IEnumerator LoadPokemonData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://pokeapi.co/api/v2/pokemon?limit=" + numberOfCards))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                PokemonList pokemonList = JsonUtility.FromJson<PokemonList>(www.downloadHandler.text);
                foreach (Pokemon_ pokemon in pokemonList.results)
                {
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
                            CreateCard(detailedPokemon);
                        }
                    }
                }
            }
        }
    }
    void CreateCard(Pokemon_ pokemon)
    {
        GameObject cardObject = Instantiate(cardPrefab) as GameObject;
        cardObject.transform.SetParent(transform);
        cards.Add(cardObject);

        Card_ card = cardObject.GetComponent<Card_>();
        card.SetData(pokemon);
    }

    void Update()
    {
        foreach (GameObject card in cards)
        {
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(card.transform.position);
            if (viewportPosition.y > 1 || viewportPosition.y < 0)
            {
                // The card is out of view so we can recycle it
                RecycleCard(card);
            }
        }
    }

    void RecycleCard(GameObject card)
    {
        // Move the card to the end of the list
        card.transform.SetAsLastSibling();

        // Update the position of the card
        card.transform.position = new Vector3(card.transform.position.x, GetLastCardPositionY() - card.GetComponent<RectTransform>().rect.height, card.transform.position.z);
    }

    float GetLastCardPositionY()
    {
        if (cards.Count == 0)
        {
            return transform.position.y;
        }
        else
        {
            return cards[cards.Count - 1].transform.position.y;
        }
    }
}
[System.Serializable]
public class PokemonList
{
    public Pokemon_[] results;
}