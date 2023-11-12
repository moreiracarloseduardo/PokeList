using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Card : MonoBehaviour
{
    public RawImage PokemonIcon;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI WeightText;
    public TextMeshProUGUI OrderText;
    private void Awake()
    {
        PokemonIcon = transform.Find("PokemonIcon").GetComponent<RawImage>();
        NameText = PokemonIcon.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        WeightText = PokemonIcon.transform.Find("Weight").GetComponent<TextMeshProUGUI>();
        OrderText = PokemonIcon.transform.Find("Order").GetComponent<TextMeshProUGUI>();
    }

}
