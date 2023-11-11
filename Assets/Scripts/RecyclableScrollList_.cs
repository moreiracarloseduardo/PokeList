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
        // Subscribe to the BecameVisible event
        Card_.BecameVisible += OnCardBecameVisible;
        StartCoroutine(LoadPokemonData(currentOffset));
    }
    void OnCardBecameVisible(int id)
    {
        // Calculate the index of the card in the list
        int index = pokemons.FindIndex(p => p.Id == id);

        // If the card is near the end of the list, load the next page
        int cardsLeft = pokemons.Count - index;
        if (cardsLeft <= 5 && !IsLoading())
        {
            LoadNextPage();
        }
    }
    private void OnDestroy()
    {
        // Unsubscribe from the BecameVisible event
        Card_.BecameVisible -= OnCardBecameVisible;
    }

    void Update()
    {
        UpdateVisibleCards();
    }
    void UpdateVisibleCards()
    {
        float cardHeight = gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y;
        float visibleHeight = contentRectTransform.rect.height;

        int firstVisibleIndexNew = Mathf.Max(Mathf.FloorToInt(-contentRectTransform.anchoredPosition.y / cardHeight), 0);
        int lastVisibleIndexNew = Mathf.Min(firstVisibleIndexNew + Mathf.CeilToInt(visibleHeight / cardHeight) + 1, pokemons.Count - 1);

        // If the range of visible cards has changed, update the cards
        if (firstVisibleIndexNew != firstVisibleIndex || lastVisibleIndexNew != lastVisibleIndex)
        {
            Debug.Log("Updating visible cards in batch...");
            // Recycle all currently visible cards
            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                if (i < cards.Count)
                {
                    RecycleCard(cards[i]);
                }
            }

            // Create new cards for the new range
            for (int i = firstVisibleIndexNew; i <= lastVisibleIndexNew; i++)
            {
                if (i < pokemons.Count)
                {
                    CreateCard(pokemons[i]);
                }
            }

            firstVisibleIndex = firstVisibleIndexNew;
            lastVisibleIndex = lastVisibleIndexNew;
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