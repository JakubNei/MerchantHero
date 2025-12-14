
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;


public class CardInstance : MonoBehaviour
{
    public CardTypeColor TypeColor;
    public CardType Type => TypeColor.Type;
    public CardColor Color => TypeColor.Color;

    public RectTransform Rect => rectTransform;
    public RectTransform RectTransform => rectTransform;

    public bool isSelected = false;

    public CardHandManager LastPlayedBy;

    RectTransform rectTransform;
    CardDragToPile cardDrag;
    CardHoverEffect cardHover;
    Image cardImage;
    Button cardButton;

    [SerializeField]
    CardSpritesConfig cardSpritesConfig;

    [SerializeField]
    TextMeshProUGUI damageNumberYellow;
    [SerializeField]
    TextMeshProUGUI damageNumberLightRedWhite;
    [SerializeField]
    TextMeshProUGUI damageNumberDarkTed;

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardImage = GetComponent<Image>();
        cardButton = GetComponent<Button>();
        cardDrag = GetComponent<CardDragToPile>();
        cardHover = GetComponent<CardHoverEffect>();
    }

    public int GetDamage()
    {
        switch (Type)
        {
            case CardType.Dodge:
                return 0;
            case CardType.Kick:
                return 1;
            case CardType.Card5:
                return 5;
            case CardType.Card6:
                return 6;
            case CardType.Feint:
                return 7;
            case CardType.Card8:
                return 8;
            case CardType.Card9:
                return 9;
            case CardType.Card10:
                return 10;
        }

        return 0;
    }

    TextMeshProUGUI GetDamageNumberText()
    {
        switch (Type)
        {
            case CardType.Dodge:
                return null; // no number
            case CardType.Kick:
                return damageNumberYellow; // dark brown font
            case CardType.Card5:
                return damageNumberLightRedWhite; // rest white font
            case CardType.Card6:
                return damageNumberLightRedWhite;
            case CardType.Feint:
                return damageNumberLightRedWhite;
            case CardType.Card8:
                return damageNumberLightRedWhite;
            case CardType.Card9:
                return damageNumberLightRedWhite;
            case CardType.Card10:
                return damageNumberLightRedWhite;
        }

        return null;
    }


    public bool IsSpecialCard()
    {
        return Type == CardType.Dodge || Type == CardType.Kick || Type == CardType.Feint;
    }


    public void ToggleSelection()
    {
        isSelected = !isSelected;
        cardHover.SetSelected(isSelected);

        if (isSelected)
        {
            CardBattle.I.SetPlayerSelectedCard(this);
        }
        else
        {
            CardBattle.I.ClearPlayerSelectedCard(this);
        }
    }

    public void Deselect()
    {
        cardHover.SetSelected(false);
        isSelected = false;
    }

    public void ShowBackSide()
    {
        var sprite = cardSpritesConfig.GetBackSideSprite();
        cardImage.sprite = sprite;

        damageNumberYellow.enabled = false;
        damageNumberLightRedWhite.enabled = false;
        damageNumberDarkTed.enabled = false;
    }

    public void ShowTypeAndColor(CardColor temporarilyShowDifferentColor)
    {
        Sprite sprite = cardSpritesConfig.GetSprite(Type, temporarilyShowDifferentColor);
        SetSprite(sprite);
    }

    public void ShowTypeAndColor(bool showDodgeAsAnyColor)
    {
        Sprite sprite = null;
        if (Type == CardType.Dodge && showDodgeAsAnyColor)
        {
            sprite = cardSpritesConfig.GetAllColorsUpperSprite();
        }
        else
        {
            sprite = cardSpritesConfig.GetSprite(Type, Color);
        }
        SetSprite(sprite);
    }

    void SetSprite(Sprite sprite)
    {
        cardImage.sprite = sprite;

        var damageNumber = GetDamageNumberText();
        if (damageNumber)
        {
            damageNumber.enabled = true;
            damageNumber.SetText(GetDamage().ToString());
            SetNumberAlpha(1);
        }
    }

    void SetNumberAlpha(float alpha)
    {
        var damageNumber = GetDamageNumberText();
        if (damageNumber && damageNumber.enabled)
        {
            var c = damageNumber.color;
            damageNumber.color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    public void OnClickDoOnly(UnityAction action)
    {
        cardButton.enabled = true;
        cardButton.interactable = true;
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(action);
        SetNumberAlpha(1);
    }

    public void DisableCardClick(bool showTransparent = false)
    {
        cardButton.onClick.RemoveAllListeners();
        if (showTransparent)
        {
            cardButton.enabled = true;
            cardButton.interactable = false;
            SetNumberAlpha(0.5f);
        }
        else
        {
            cardButton.enabled = false;
            cardButton.interactable = true;
            SetNumberAlpha(1);
        }
    }

    public void AllowDragToPile(bool allow)
    {
        cardDrag.enabled = allow;
    }

    public void AllowHoverScaleUpEffect(bool allow)
    {
        cardHover.enabled = allow;
    }



    public override string ToString()
    {
        return TypeColor.ToString();
    }
}

public enum CardColor
{
    Red,
    Green,
    Blue,
    Yellow,
}

public enum CardType
{
    Dodge, // Upper, Svrsek, 0
    Kick, // Ace, Eso, 1
    Card5, // Under, Spodek, 5
    Card6, // King, Kral, 6
    Feint, // 7 
    Card8,
    Card9,
    Card10,
}
