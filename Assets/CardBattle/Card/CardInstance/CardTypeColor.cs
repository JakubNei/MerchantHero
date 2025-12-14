using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CardTypeColor
{
    public CardType Type;
    public CardColor Color;
    public static readonly CardColor[] allColors = new[] { CardColor.Red, CardColor.Green, CardColor.Yellow, CardColor.Blue };
    public static readonly CardType[] allTypes = new[] { CardType.Feint, CardType.Card8, CardType.Card9, CardType.Card10, CardType.Card5, CardType.Dodge, CardType.Card6, CardType.Kick };

    public static CardTypeColor GetRandom()
    {
        return new CardTypeColor
        {
            Color = allColors[Random.Range(0, allColors.Length - 1)],
            Type = allTypes[Random.Range(0, allTypes.Length - 1)]
        };
    }
    // 32 cards where each card has unique color and type
    public static List<CardTypeColor> GenerateAllPossibleCards()
    {
        var result = new List<CardTypeColor>();
        foreach (var color in allColors)
        {
            foreach (var type in allTypes)
            {
                result.Add(new CardTypeColor
                {
                    Color = color,
                    Type = type
                });
            }
        }
        return result;
    }

    public override string ToString()
    {
        return $"{Color} {Type}";
    }
}
