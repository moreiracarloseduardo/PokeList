using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RecyclableScrollList_ : MonoBehaviour
{
    public GameObject cardPrefab;
    public int cardsPerPage = 20;
    public int numberOfCards;
    private int currentOffset = 0;
    private List<GameObject> cards = new List<GameObject>();
    private List<Pokemon_> pokemons = new List<Pokemon_>();
    private bool isLoading = false; // This flag will be true when data is being loaded

    public bool IsLoading()
    {
        return isLoading;
    }


    void Start()
    {
        StartCoroutine(LoadPokemonData(currentOffset));
        RectTransform contentRectTransform = GetComponent<RectTransform>();
        GridLayoutGroup gridLayoutGroup = GetComponent<GridLayoutGroup>();

        float totalCardHeight = gridLayoutGroup.cellSize.y; // Use the cell size of the GridLayoutGroup to get the card height
        float spacing = gridLayoutGroup.spacing.y;
        int numberOfColumns = gridLayoutGroup.constraintCount;

        int numberOfRows = Mathf.CeilToInt((float)numberOfCards / numberOfColumns);
        float totalHeight = numberOfRows * totalCardHeight + (numberOfRows - 1) * spacing;

        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);
    }
    IEnumerator LoadPokemonData(int offset)
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
                UpdateContentSize();
                currentOffset += cardsPerPage; // Update the offset for the next page of data
            }
        }
        isLoading = false;
    }
    void CreateCard(Pokemon_ pokemon)
    {
        GameObject cardObject = Instantiate(cardPrefab) as GameObject;
        cardObject.transform.SetParent(transform, false);
        cards.Add(cardObject);

        Card_ card = cardObject.GetComponent<Card_>();
        card.SetData(pokemon);
    }
    void UpdateContentSize()
    {
        RectTransform contentRectTransform = GetComponent<RectTransform>();
        GridLayoutGroup gridLayoutGroup = GetComponent<GridLayoutGroup>();

        float totalCardHeight = gridLayoutGroup.cellSize.y; // Use the cell size of the GridLayoutGroup to get the card height
        float spacing = gridLayoutGroup.spacing.y;
        int numberOfColumns = gridLayoutGroup.constraintCount;

        int numberOfRows = Mathf.CeilToInt((float)cards.Count / numberOfColumns);
        float totalHeight = numberOfRows * totalCardHeight + (numberOfRows - 1) * spacing;

        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);
    }
    public void LoadNextPage()
    {
        if (!isLoading) // Only start loading the next page if data is not currently being loaded
        {
            StartCoroutine(LoadPokemonData(currentOffset));
        }
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