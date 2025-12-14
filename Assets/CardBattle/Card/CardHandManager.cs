using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

public class CardHandManager : MonoBehaviour
{
    [SerializeField] public bool isPlayer;
    [SerializeField] RectTransform cardPrefab;
    [SerializeField] float stackOffset = 20f;
    [SerializeField] float screenPadding = 20f;
    [SerializeField] float yOffsetAtTheBottom = 20f;
    [SerializeField] float animationDuration = 0.5f;
    [SerializeField] TextMeshProUGUI healthText;
    //Player _player;
    public static CardHandManager Player 
    {
        get
        {
            var f = FindObjectsByType<CardHandManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var fs in f)
            {
                if (fs.isPlayer == true)
                    return fs;
            }
            return null;
        }
    }
    public static CardHandManager Enemy
    {
        get
        {
            var f = FindObjectsByType<CardHandManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var fs in f)
            {
                if (fs.isPlayer == false)
                    return fs;
            }
            return null;
        }
    }

    public int Health { get; private set; }
    public int StartingHealth = 20;

    public int NumStartingCards = 5;

    // How many times did this hand die ?
    public int DeadCounter { get; private set; } = 0;
    public Action OnDead;
    public CardInstance lastlyDamagedByCard;

    // public void SetPlayer(Player player)
    // { 
    //    _player = player;
    // }

    void OnEnable()
    {
        SetHealth(StartingHealth);
    }
    public void AddDamage(int damage)
    {
        SetHealth(Health - damage);
    }
    public void AddHealth(int health)
    {
        SetHealth(Health + health);
    }


    public void SetHealth(int newHalth)
    {
        if (healthText)
        {
            if (Health != newHalth)
            {
                const float duration1 = 0.05f;
                const float duration2 = 0.1f;
                // scale up for a moment to signal received damage
                {
                    var rect = healthText.transform.parent.GetComponent<RectTransform>();
                    var originalScale = rect.localScale;
                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(rect.DOScale(originalScale * 1.4f, duration1).SetEase(Ease.InOutQuad));
                    sequence.Append(rect.DOScale(originalScale, duration2).SetEase(Ease.InOutQuad));
                }

                // blick text red
                {
                    var originalColor = healthText.color;
                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(healthText.DOColor(new Color(0, 0, 0, 0), duration1).SetEase(Ease.InOutQuad));
                    sequence.Append(healthText.DOColor(originalColor, duration2).SetEase(Ease.InOutQuad));
                }
            }

            if (newHalth < Health)
            {
                var audioSource = healthText.transform.parent.GetComponent<AudioSource>();
                if (audioSource)
                {
                    audioSource.PlayOneShot(audioSource.clip);
                }
                Debug.Log(gameObject.name + " lost " + (Health - newHalth) + " HP", this);
            }
        }

        Health = newHalth;
        healthText?.SetText((newHalth > 0 ? newHalth : 0).ToString());
        if (Health <= 0)
        {
            DeadCounter++;
            if (OnDead != null)
                OnDead();
        }

    }

    public void AssignStaticAccessors()
    {

    }

    void OnTransformChildrenChanged()
    {
        ReorganizeHand();
    }

    public List<CardInstance> GetCardsInHand()
    {
        return new List<CardInstance>(GetComponentsInChildren<CardInstance>());
    }

    public List<CardInstance> GetCardsInHandOfType(CardType type)
    {
        var cards = new List<CardInstance>();
        foreach (var card in GetComponentsInChildren<CardInstance>())
        {
            if (card.Type == type)
            {
                cards.Add(card);
            }
        }
        return cards;
    }

    public void PutRandomCardToDeck_WithAnimation()
    {
        var cards = GetCardsInHand();
        if (cards.Count > 0)
        {
            var randomCard = cards[Random.Range(0, cards.Count)];
            DeckManager.I.AddCard_WithAnimation(randomCard);
        }
    }

    public Tween AddCard_WithAnimation(CardInstance card, bool isStartDeal)
    {
        CardBattle.I.playerSelectedCard?.Deselect();

        AddCardInternal(card);

        Vector2 targetPosition = GetCardPositionInHand(transform.childCount);
        Vector2 midpoint = (card.Rect.anchoredPosition + targetPosition) / 2 + Vector2.up * 150f * (isPlayer ? 1 : -1);

        float duration = isStartDeal ? animationDuration : animationDuration / 2;
        Sequence cardAnimation = DOTween.Sequence();
        cardAnimation.Append(card.Rect.DOAnchorPos(midpoint, animationDuration / 2).SetEase(Ease.OutSine));
        cardAnimation.Join(card.Rect.DORotateQuaternion(Quaternion.Euler(0, 0, Random.Range(-4.0f, 4.0f)), duration).SetEase(Ease.InSine));
        cardAnimation.Join(card.RectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad));
        CardBattle.I.numCardAnimationsInProgress++;
        cardAnimation.OnComplete(() =>
        {
            CardBattle.I.numCardAnimationsInProgress--;
            ReorganizeHand();
            CardBattle.I.UpdatePlayerHandInteractableState();
        });

        return cardAnimation;
    }

    Vector2 GetCardPositionInHand(int index)
    {
        var canvasRect = transform as RectTransform;

        float screenWidth = canvasRect.rect.width - 2 * screenPadding;
        float cardWidth = cardPrefab.rect.width;

        var cardCount = transform.childCount;
        float totalWidth = cardCount * cardWidth;
        float spacing = totalWidth > screenWidth ? (screenWidth - cardWidth) / (cardCount - 1) : cardWidth;
        spacing = Mathf.Max(spacing, stackOffset);

        float handWidth = (cardCount - 1) * spacing + cardWidth;
        float startingX = -handWidth / 2f + cardWidth / 2f;
        float bottomY = isPlayer
              ? -canvasRect.rect.height / 2f + cardPrefab.rect.height / 2f + yOffsetAtTheBottom
              : canvasRect.rect.height / 2f - cardPrefab.rect.height / 2f - yOffsetAtTheBottom;

        return new Vector2(startingX + index * spacing, bottomY);
    }

    public bool HasPlayableCard()
    {
        foreach (var card in GetCardsInHand())
        {
            if (CardBattle.I.CanPlayCard(card))
            {
                return true;
            }
        }
        return false;
    }

    public void AddCard(CardInstance card)
    {
        AddCards(new List<CardInstance>
        {
            card
        });
    }

    public void AddCards(List<CardInstance> cards)
    {
        foreach (var card in cards)
        {
            AddCardInternal(card);
        }

        ReorganizeHand();
    }

    public void ReorganizeHand()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var card = transform.GetChild(i).GetComponent<CardInstance>();
            Vector2 targetPosition = GetCardPositionInHand(i);

            Sequence cardAnimation = DOTween.Sequence();
            cardAnimation.Join(card.Rect.DOAnchorPos(targetPosition, animationDuration / 2).SetEase(Ease.OutQuad));

            // Generate a random rotation within the range -2 to -4 or +2 to +4
            float randomRotation = Random.Range(0, 2) == 0
                ? Random.Range(-4f, -2f)
                : Random.Range(2f, 4f);

            var zRot = card.Rect.rotation.eulerAngles.z + randomRotation;

            if (Mathf.Abs(zRot) > 4)
            {
                zRot = Mathf.Clamp(zRot, -4, 4);
            }

            cardAnimation.Join(card.Rect.DORotateQuaternion(Quaternion.Euler(0, 0, zRot), animationDuration / 2).SetEase(Ease.InSine));
        }
    }


    void OnCardClick(CardInstance card)
    {
        Debug.Log($"Card clicked: {card}");
    }


    void AddCardInternal(CardInstance card)
    {
        card.Rect.SetParent(transform, true);
        card.Rect.SetAsLastSibling();

        if (isPlayer)
        {
            card.ShowTypeAndColor(showDodgeAsAnyColor: true);
            card.AllowDragToPile(true);
            card.OnClickDoOnly(() => OnCardClick(card));
        }
        else
        {
            card.ShowBackSide();
            card.AllowDragToPile(false);
            // card.AllowHoverScaleUpEffect(false);
            card.DisableCardClick();
        }
        card.AllowHoverScaleUpEffect(true); // libilo se mi vic, kdyz se zvetsuji vsechny
    }

}
