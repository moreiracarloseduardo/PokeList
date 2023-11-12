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
    public GameObject loadingIndicator;

    private bool isLoading = false;
    private bool isScrollCooldownActive = false;

    private int limit = 20;
    private int start = 1;
    private PokemonAPIManager pokemonAPIManager;
    private LRUCache<string, Texture2D> imageCache = new LRUCache<string, Texture2D>(100);
    private RectTransform scrollRectRectTransform;

    private void Start()
    {
        pokemonAPIManager = GetComponent<PokemonAPIManager>();
        StartCoroutine(pokemonAPIManager.GetPokemonData(start, limit, InitializeCards));
        scrollRect.onValueChanged.AddListener(OnScroll);

        scrollRectRectTransform = scrollRect.GetComponent<RectTransform>();
    }

    private void OnScroll(Vector2 scrollPosition)
    {
        if (scrollRect.verticalScrollbar != null && scrollRect.verticalScrollbar.value <= 0.01f && !isLoading && !isScrollCooldownActive)
        {
            StartCoroutine(LoadMorePokemon());
        }
        UpdateCardVisibility();
    }
    private IEnumerator LoadImage(string url, RawImage imageComponent)
    {
        Texture2D texture = imageCache.Get(url);

        if (texture == null)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    imageCache.Add(url, texture);
                }
                else
                {
                    Debug.Log(www.error);
                }
            }
        }

        if (imageComponent != null && texture != null)
        {
            imageComponent.texture = texture;
        }
    }
    private void CreateCard(Pokemon pokemon)
    {
        GameObject card = Instantiate(cardPrefab, contentTransform);
        if (card == null)
        {
            Debug.LogError("Failed to instantiate cardPrefab.");
            return;
        }

        Card cardComponent = card.GetComponent<Card>();
        if (cardComponent == null)
        {
            Debug.LogError("Failed to get Card component.");
            return;
        }

        if (cardComponent.PokemonIcon == null)
        {
            Debug.LogError("PokemonIcon is null.");
            return;
        }

        StartCoroutine(LoadImage(pokemon.sprites.front_default, cardComponent.PokemonIcon));

        if (cardComponent.NameText == null)
        {
            Debug.LogError("NameText is null.");
            return;
        }
        cardComponent.NameText.text = pokemon.name;

        if (cardComponent.WeightText == null)
        {
            Debug.LogError("WeightText is null.");
            return;
        }
        cardComponent.WeightText.text = $"Weight: {pokemon.weight}";

        if (cardComponent.OrderText == null)
        {
            Debug.LogError("OrderText is null.");
            return;
        }
        cardComponent.OrderText.text = $"Order: {pokemon.order}";

        if (!string.IsNullOrWhiteSpace(pokemon.color))
        {
            backgroundImage.color = ConvertColor(pokemon.color);
        }
        else
        {
            Debug.LogWarning("Pokemon color is null or empty.");
            backgroundImage.color = Color.white;
        }
    }
    private IEnumerator LoadMorePokemon()
    {
        isLoading = true;
        loadingIndicator.SetActive(true);

        List<Pokemon> newPokemons = new List<Pokemon>();
        yield return StartCoroutine(pokemonAPIManager.GetPokemonData(start, limit, pokemons => newPokemons = pokemons));
        start += limit;

        foreach (Pokemon pokemon in newPokemons)
        {
            CreateCard(pokemon);
            yield return null;
        }

        isLoading = false;
        loadingIndicator.SetActive(false);
    }

    private void UpdateCardVisibility()
    {
        foreach (Transform child in contentTransform)
        {
            GameObject card = child.gameObject;
            Image cardImage = card.GetComponentInChildren<Image>();
            GameObject pokemonIcon = card.transform.Find("PokemonIcon").gameObject;

            if (IsVisible(card.GetComponent<RectTransform>(), scrollRect))
            {
                cardImage.enabled = true;
                pokemonIcon.SetActive(true);
            }
            else
            {
                cardImage.enabled = false;
                pokemonIcon.SetActive(false);
            }
        }
    }

    public bool IsVisible(RectTransform rectTransform, ScrollRect scrollRect)
    {
        Rect rect = scrollRectRectTransform.rect;
        return RectTransformUtility.RectangleContainsScreenPoint(scrollRectRectTransform, rectTransform.position, null) && rectTransform.rect.height < rect.height;
    }


    private void InitializeCards(List<Pokemon> pokemons)
    {
        foreach (Pokemon pokemon in pokemons)
        {
            CreateCard(pokemon);
        }
    }

    Color ConvertColor(string colorName)
    {
        if (string.IsNullOrEmpty(colorName))
        {
            Debug.LogWarning("Color name is null or empty.");
            return Color.white;
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
    public class LRUCache<TKey, TValue>
    {
        private readonly int capacity;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cache;
        private readonly LinkedList<CacheItem> lruList;

        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            this.cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
            this.lruList = new LinkedList<CacheItem>();
        }

        public TValue Get(TKey key)
        {
            if (cache.TryGetValue(key, out LinkedListNode<CacheItem> node))
            {
                TValue value = node.Value.Value;
                lruList.Remove(node);
                lruList.AddLast(node);
                return value;
            }
            return default(TValue);
        }

        public void Add(TKey key, TValue value)
        {
            if (cache.ContainsKey(key))
            {
                LinkedListNode<CacheItem> node = cache[key];
                node.Value.Value = value;
                lruList.Remove(node);
                lruList.AddLast(node);
            }
            else
            {
                if (cache.Count >= capacity)
                {
                    RemoveFirst();
                }

                CacheItem cacheItem = new CacheItem { Key = key, Value = value };
                LinkedListNode<CacheItem> node = new LinkedListNode<CacheItem>(cacheItem);
                lruList.AddLast(node);
                cache.Add(key, node);
            }
        }
        private void RemoveFirst()
        {
            LinkedListNode<CacheItem> node = lruList.First;
            lruList.RemoveFirst();
            cache.Remove(node.Value.Key);
        }

        private class CacheItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }
    }

}
