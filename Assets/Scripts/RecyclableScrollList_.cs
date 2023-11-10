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
    public Transform contentPanel;
    public RectTransform contentRectTransform;
    public GridLayoutGroup gridLayoutGroup;
    private int currentOffset = 0;
    private List<GameObject> cards = new List<GameObject>();
    private List<Pokemon_> pokemons = new List<Pokemon_>();
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private bool isLoading = false; // This flag will be true when data is being loaded
    private int firstVisibleIndex = 0;
    private int lastVisibleIndex = 0;

    public bool IsLoading()
    {
        return isLoading;
    }


    void Start()
    {
        StartCoroutine(LoadPokemonData(currentOffset));
    }
    void Update()
    {
        UpdateVisibleCards();
    }
    void UpdateVisibleCards()
    {
       Debug.Log("UpdateVisibleCards");
        float cardHeight = gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y;
        float visibleHeight = contentRectTransform.rect.height;

        int firstVisibleIndexNew = Mathf.Max(Mathf.FloorToInt(-contentRectTransform.anchoredPosition.y / cardHeight), 0);
        int lastVisibleIndexNew = Mathf.Min(firstVisibleIndexNew + Mathf.CeilToInt(visibleHeight / cardHeight) + 1, pokemons.Count - 1);

        // Recycle cards that are out of view
        while (firstVisibleIndex < firstVisibleIndexNew && firstVisibleIndex < cards.Count)
        {
            RecycleCard(cards[firstVisibleIndex]);
            firstVisibleIndex++;
        }
        while (lastVisibleIndex > lastVisibleIndexNew && lastVisibleIndex < cards.Count)
        {
            RecycleCard(cards[lastVisibleIndex]);
            lastVisibleIndex--;
        }

        // Create cards that are now visible
        while (firstVisibleIndex > firstVisibleIndexNew && firstVisibleIndex < pokemons.Count)
        {
            firstVisibleIndex--;
            CreateCard(pokemons[firstVisibleIndex]);
        }
        while (lastVisibleIndex < lastVisibleIndexNew && lastVisibleIndex < pokemons.Count)
        {
            lastVisibleIndex++;
            CreateCard(pokemons[lastVisibleIndex]);
        }
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
    public void CreateCard(Pokemon_ pokemon)
    {
        GameObject card;
        if (objectPool.Count > 0)
        {
            card = objectPool.Dequeue();
            card.SetActive(true);
        }
        else
        {
            card = Instantiate(cardPrefab, contentPanel);
        }

        Card_ cardComponent = card.GetComponent<Card_>();
        cardComponent.SetData(pokemon);

        cards.Add(card);
    }
    public void RecycleCard(GameObject card)
    {
        card.SetActive(false);
        objectPool.Enqueue(card);

        cards.Remove(card);
    }
    void UpdateContentSize()
    {

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
}
[System.Serializable]
public class PokemonList
{
    public Pokemon_[] results;
}