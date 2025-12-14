using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;

public class DeckManager : MonoBehaviour
{
    [SerializeField] RectTransform cardPrefab;
    [SerializeField] float rotationRange = 15f;

    [SerializeField] int minAmountOfCardsRequiredToBeInDeck = 10;

    public static DeckManager I => FindAnyObjectByType<DeckManager>(FindObjectsInactive.Include);

    Vector3 GetLastCardDesiredPosition()
    {
        var canvasRect = transform as RectTransform;
        float startingX = -canvasRect.rect.width / 2 + cardPrefab.rect.width + 15 / 2; // 15 is offset from the left
        float yPosition = transform.childCount - 1;
        return new Vector2(startingX, yPosition);
    }

    Quaternion GetCardRandomRotation()
    {
        float randomRotation = Random.Range(-rotationRange, rotationRange);
        return Quaternion.Euler(0, 0, randomRotation);
    }

    public IEnumerator AddCards_WithAnimation(List<CardInstance> cards)
    {
        bool instantDeal = false;
        float cardDealDelay = 0.1f;
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            AddCard_WithAnimation(card);

            if (!instantDeal)
                yield return new WaitForSeconds(cardDealDelay);
            //instantDeal = instantDeal || DialogueManager.SkipDialogWasPressedThisFrame;
        }
    }

    public Sequence AddCard_WithAnimation(CardInstance card)
    {
        AddCardInternal(card);
        Sequence moveSequence = DOTween.Sequence();
        moveSequence.Join(card.Rect.DOAnchorPos(GetLastCardDesiredPosition(), 0.3f).SetEase(Ease.OutQuad));
        // moveSequence.Join(card.Rect.DORotate(Vector3.forward *  GetCardRandomRotation(), 0.3f).SetEase(Ease.OutQuad));
        CardBattle.I.numCardAnimationsInProgress++;
        return moveSequence.OnComplete(() =>
             {
                 CardBattle.I.numCardAnimationsInProgress--;
                 UpdateInteractableState();
             });
    }

    public void AddCards_NoAnimation(List<CardInstance> cards)
    {
        DoSomethingWith11thCard(cards);

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            AddCardInternal(card);

            card.Rect.anchoredPosition = GetLastCardDesiredPosition();
            card.Rect.localRotation = GetCardRandomRotation();
        }

        UpdateInteractableState();
    }

    void DoSomethingWith11thCard(List<CardInstance> cards)
    {
        int targetIndex = CardHandManager.Enemy.NumStartingCards + CardHandManager.Player.NumStartingCards;
        if (targetIndex < cards.Count)
        {
            if (!cards[targetIndex].IsSpecialCard())
            {
                List<CardInstance> nonSpecialCards = cards.FindAll(card => !card.IsSpecialCard());
                if (nonSpecialCards.Count > 0)
                {
                    CardInstance nonSpecialCard = nonSpecialCards[Random.Range(0, nonSpecialCards.Count)];
                    int swapIndex = cards.IndexOf(nonSpecialCard);

                    // Swap cards in the list
                    (cards[targetIndex], cards[swapIndex]) = (cards[swapIndex], cards[targetIndex]);
                }
                else
                {
                    Debug.LogWarning("No non-special cards available to place at the 13th position.");
                }
            }
        }
    }


    void OnTransformChildrenChanged()
    {
        if (!HasCards())
        {
            TryFillCardsFromPileOrHands();
        }

        UpdateInteractableState();
    }

    public bool HasCards()
    {
        return transform.childCount > 0;
    }

    public CardInstance GetTopCard()
    {
        var card = transform.GetChild(transform.childCount - 1).GetComponent<CardInstance>();
        return card;
    }

    public void TryFillCardsFromPileOrHands()
    {
        var cards = PileManager.I.GetCards();
        cards.RemoveAt(cards.Count - 1); // leave last one there

        // Deck always has to have 10 cards
        // lets take from the hand with biggest amount of cards
        if (cards.Count < minAmountOfCardsRequiredToBeInDeck)
        {
            var enemyHand = CardHandManager.Player.GetCardsInHand();
            var playerHand = CardHandManager.Enemy.GetCardsInHand();
            while (cards.Count < minAmountOfCardsRequiredToBeInDeck)
            {
                var handToTakeFrom = enemyHand.Count > playerHand.Count ? enemyHand : playerHand;
                if (handToTakeFrom.Count == 0)
                {
                    Debug.LogError("WTF where are the cards then if not in pile and not in playor nor enemy hand ?");
                    break;
                }
                var cardToTake = handToTakeFrom[Random.Range(0, handToTakeFrom.Count)];
                handToTakeFrom.Remove(cardToTake);
                cards.Add(cardToTake);
            }
        }

        cards.Sort((CardInstance a, CardInstance b) => Random.Range(0, 100000)); // random order
        DoSomethingWith11thCard(cards);
        StartCoroutine(AddCards_WithAnimation(cards));
    }

    void OnTopCardClicked()
    {
        if (!CardBattle.I.CanPlayerDrawCardFromDeck)
            return;

        StartCoroutine(CardBattle.I.HandDrawCardFromDeck(CardHandManager.Player));
    }

    void UpdateInteractableState()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var rect = transform.GetChild(i);
            var card = rect.GetComponent<CardInstance>();

            bool isTopCard = i == transform.childCount - 1;
            if (isTopCard)
            {
                card.AllowHoverScaleUpEffect(true);
                card.OnClickDoOnly(OnTopCardClicked);
            }
            else
            {
                card.AllowHoverScaleUpEffect(false);
                card.DisableCardClick();
            }
        }
    }

    private void AddCardInternal(CardInstance card)
    {
        card.Rect.SetParent(transform, true);
        card.Rect.SetAsLastSibling();
        card.ShowBackSide();
        card.AllowDragToPile(false);
    }
}