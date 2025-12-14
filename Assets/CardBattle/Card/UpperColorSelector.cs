using System;
using DG.Tweening;
using UnityEngine;

public class UpperColorSelector : MonoBehaviour
{
    [SerializeField] CardInstance colorSelectionCard;

    CardColor colorAfterColorChosen;
    RectTransform rectTra => GetComponent<RectTransform>();

    public static UpperColorSelector i;
    [HideInInspector] public bool colorPicked = false;

    private void Awake()
    {
        i = this;
    }

    public void OnUpperCardPlayedByPlayer()
    {
        CardBattle.I.currentState = State.Player_PickColor;
        SpawnColorSelectionCards();
    }

    public void OnUpperCardPlayedByEnemy()
    {        
        // pick random color
        CardColor colorToChangeTo = (CardColor)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(CardColor)).Length);

        // pick color that enemy has most of in his deck
        {
            Func<CardColor, int> countColors = (CardColor c) =>
            {
                int result = 0;
                foreach (var card in CardHandManager.Enemy.GetCardsInHand())
                {
                    if (card.Color == c && card.Type != CardType.Dodge)
                        result++;
                }
                return result;
            };
            foreach (CardColor color in System.Enum.GetValues(typeof(CardColor)))
            {
                if (countColors(color) > countColors(colorToChangeTo))
                    colorToChangeTo = color;
            }
        }

        HandleColorPicked(colorToChangeTo);
    }

    void SpawnColorSelectionCards()
    {
        colorPicked = false;
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        float screenWidth = rectTra.rect.width;
        float cardWidth = colorSelectionCard.GetComponent<RectTransform>().rect.width;
        float spacing = cardWidth + 20f;
        int totalColors = System.Enum.GetValues(typeof(CardColor)).Length;

        float totalWidth = (totalColors - 1) * spacing + cardWidth;
        float startX = -totalWidth / 2f + cardWidth / 2f;

        int index = 0;
        foreach (CardColor color in System.Enum.GetValues(typeof(CardColor)))
        {
            CardInstance colorCard = Instantiate(colorSelectionCard, transform);
            colorCard.TypeColor = new CardTypeColor { Type = CardType.Dodge, Color = color };
            colorCard.ShowTypeAndColor(showDodgeAsAnyColor: false);
            colorCard.AllowHoverScaleUpEffect(true);

            RectTransform cardRect = colorCard.GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(startX + index * spacing, 0);

            colorCard.OnClickDoOnly(() =>
            {
                HandleColorPicked(color);
                Destroy(colorCard.gameObject);
            });

            index++;
        }
    }

    void HandleColorPicked(CardColor selectedColor)
    {
        var lastPlayedCard = PileManager.I.GetLastPlayedCard();

        if (lastPlayedCard != null && lastPlayedCard.Type == CardType.Dodge)
        {
            Sequence colorChangeSequence = DOTween.Sequence();
            colorChangeSequence.Append(lastPlayedCard.RectTransform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutQuad));
            colorChangeSequence.AppendCallback(() =>
            {
                lastPlayedCard.ShowTypeAndColor(selectedColor);
            });
            colorChangeSequence.Append(lastPlayedCard.RectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InQuad));
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        SetPickedColor(selectedColor);

        CardBattle.I.SwapTurns();
    }

    public void SetPickedColor(CardColor color)
    {
        colorAfterColorChosen = color;
        colorPicked = true;
    }


    public CardColor GetPickedColor() => colorAfterColorChosen;
}
