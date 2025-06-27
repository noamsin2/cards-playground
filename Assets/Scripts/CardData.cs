using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CardEffect
{
    public string action;
    public string effect;
    public int value;
}

[System.Serializable]
public class CardData
{
    public string cardName;
    public string cardDescription;
    public Sprite cardImage;
    public List<CardEffect> effects;
    public CardData(string name, string description, Sprite img, List<CardEffect> effects)
    {
        cardName = name;
        cardDescription = description;
        cardImage = img;
        this.effects = effects;
    }
}
