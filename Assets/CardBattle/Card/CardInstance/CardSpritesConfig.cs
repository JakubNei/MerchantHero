using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSpritesConfig", menuName = "Scriptable Objects/CardSpritesConfig")]
public class CardSpritesConfig : ScriptableObject
{
    [System.Serializable]
    public struct Card
    {
        public CardColor color;
        public CardType type;
        public Sprite sprite;
    }

    [SerializeField]
    List<Card> cards = new();

    [SerializeField]
    Sprite backSide;

    [SerializeField]
    Sprite allColorsUpper;

    public Sprite GetBackSideSprite() => backSide;

    public Sprite GetAllColorsUpperSprite() => allColorsUpper;

    Dictionary<Tuple<CardColor, CardType>, Sprite> cache = new();
    public Sprite GetSprite(CardType type, CardColor color)
    {
        var key = Tuple.Create(color, type);

        Sprite sprite;
        if (cache.TryGetValue(key, out sprite))
            return sprite;

        var index = cards.FindIndex(x => x.type == type && x.color == color);
        if (index == -1)
        {
            Debug.LogError($"Could not find sprite for {type} {color}");
            return null;
        }

        sprite = cards[index].sprite;
        cache.Add(key, sprite);
        return sprite;
   }
}
