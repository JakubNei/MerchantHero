using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class PileManager : MonoBehaviour
{
    RectTransform playDeckArea => GetComponent<RectTransform>();
    [SerializeField] CardHandManager cardHandManager;
    public static PileManager I => FindAnyObjectByType<PileManager>(FindObjectsInactive.Include);

    public bool isInitialCard;

    public void OnPileClicked() // called inside the button
    {
        CardBattle cardBattle = CardBattle.I;
        if (!cardBattle.CanPlayerPlayCardInHand)
            return;

        if (cardBattle.playerSelectedCard && cardBattle.playerSelectedCard.isSelected && cardBattle.playerSelectedCard != GetLastPlayedCard())
            cardBattle.TryPlayCard(CardBattle.I.playerSelectedCard, CardHandManager.Player);
    }

    public List<CardInstance> GetCards()
    {
        return GetComponentsInChildren<CardInstance>().ToList();
    }
    public bool HasCards()
    {
        return transform.childCount > 0;
    }

    public bool IsCardOverPile(RectTransform card, Vector2 screenPosition, Camera camera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(playDeckArea, screenPosition, camera);
    }

    public void AddCard_WithAnimation(CardInstance card, bool isInitialCard = false)
    {
        float targetRotation = Random.Range(-5f, 5f);
        targetRotation = Mathf.Repeat(targetRotation, 360f);
        targetRotation = Mathf.Clamp(targetRotation, -10, 10);
        Vector2 targetPosition = GetRandomAnchorDesiredPosition();
        Sequence moveSequence = DOTween.Sequence();
        moveSequence.Join(card.Rect.DOAnchorPos(targetPosition, 0.3f).SetEase(Ease.OutQuad));
        moveSequence.Join(card.Rect.DORotate(Vector3.forward * targetRotation, 0.3f).SetEase(Ease.OutQuad));
        moveSequence.Join(card.Rect.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad));
        CardBattle.I.numCardAnimationsInProgress++;
        moveSequence.OnComplete(() =>
            {
                CardBattle.I.numCardAnimationsInProgress--;
                SnapCardToPile(card, isInitialCard);
            });
    }
    
    public CardInstance GetLastPlayedCard()
    {
        if (transform.childCount == 0)
            return null;
        return transform.GetChild(transform.childCount - 1).GetComponent<CardInstance>();
    }

    public Vector2 GetRandomAnchorDesiredPosition()
    {
        Vector2 deckPosition = playDeckArea.anchoredPosition;
   
        // return new Vector2(
        //             deckPosition.x + Random.Range(-30f, 30f),
        //             deckPosition.y + Random.Range(-3f, 3f) + transform.childCount * 2);

        return new Vector2(
                     deckPosition.x + Random.Range(-5f, 5f),
                     deckPosition.y + Random.Range(-1f, 1f) + transform.childCount * 1);

    }

    public void SnapCardToPile(CardInstance card, bool isInitialCard = false)
    {
        this.isInitialCard = isInitialCard;
        AddCardInternal(card);
    }

    // public CardInstance GetNonSpecialCardFromPile()
    // {
    //     foreach (var card in allCardsInPile)
    //     {
    //         if (card.Type != CardType.Kick && card.Type != CardType.Feint && card.Type != CardType.Dodge)
    //         {
    //             return card;
    //         }
    //     }

    //     return null; 
    // }

    private void AddCardInternal(CardInstance card)
    {
        card.Rect.SetParent(transform, true);
        card.Rect.SetAsLastSibling();
        card.ShowTypeAndColor(showDodgeAsAnyColor: false);
        card.DisableCardClick();
        card.AllowDragToPile(false);
        card.AllowHoverScaleUpEffect(false);
    }

}
