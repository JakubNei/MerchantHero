using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public enum State
{
    DealingFirstCards,
    Player_PickColor,
    Player_DrawOrPlayCard,
    Player_ReturningFaint,
    Player_ReturningKick,
    Enemy_DrawOrPlayCard
}

public class CardBattle : MonoBehaviour
{
    [SerializeField] CardInstance configCardPrefab;

    //public static CharacterAnimationBase PlayerCharacterAnimation => Player.CharacterAnimation;
    //public static CharacterAnimationBase EnemyCharacterAnimation => Enemy.CharacterAnimation;
    public State currentState = State.Player_DrawOrPlayCard;

    // Stop player breaking game when state changed but animation did not finish yet
    public bool isCardMoveAnimationInProgress => numCardAnimationsInProgress > 0;
    public int numCardAnimationsInProgress = 0;
    public int numCharacterAnimationsInProgress = 0;
    public bool CanPlayerPlayCardInHand => !stopGame && !isCardMoveAnimationInProgress && (
        currentState == State.Player_DrawOrPlayCard ||
        currentState == State.Player_ReturningFaint ||
        currentState == State.Player_ReturningKick);
    public bool CanPlayerDrawCardFromDeck => !stopGame && !isCardMoveAnimationInProgress &&
        currentState == State.Player_DrawOrPlayCard;

    bool stopGame = false;       

    public static CardBattle I => Instance;
    public static CardBattle Instance => FindAnyObjectByType<CardBattle>(FindObjectsInactive.Include);
    public List<CardInstance> allCards = new();

    // card currently picked by player
    public CardInstance playerSelectedCard;

    CardHandManager player => CardHandManager.Player;
    CardHandManager enemy => CardHandManager.Enemy;
    DeckManager deck => DeckManager.I;
    PileManager pile => PileManager.I;

    float delayAfterAnimation = 0.5f;
    bool isDealingFirstCards => currentState == State.DealingFirstCards;

    bool showDebugUI = false;
    UInt16 debugAmount = 5;

    public bool IsPlaying => transform.parent.gameObject.activeSelf && !stopGame;

    public void StartBattle()
    {
        stopGame = false;
        transform.parent.gameObject.SetActive(true);
    }
    public void StopBattle()
    {
        stopGame = true;
    }
    public void HideBattle()
    {
        transform.parent.gameObject.SetActive(false);
        //PlayerCharacterAnimation?.ShowBasic();
        //EnemyCharacterAnimation?.ShowBasic();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            showDebugUI = !showDebugUI;
        }
    }
    void OnGUI()
    {
        GUILayout.Label("D = debug");
        if (showDebugUI)
        {
            GUILayout.Space(10);

            UInt16.TryParse(GUILayout.TextField(debugAmount.ToString()), out debugAmount);
            if (GUILayout.Button(debugAmount + " damage to enemy"))
            {
                CardHandManager.Enemy.AddDamage(debugAmount);
            }
            if (GUILayout.Button(debugAmount + " damage to player"))
            {
                CardHandManager.Player.AddDamage(debugAmount);
            }
            if (GUILayout.Button(debugAmount + " health to enemy"))
            {
                CardHandManager.Enemy.AddHealth(debugAmount);
            }
            if (GUILayout.Button(debugAmount + " health to player"))
            {
                CardHandManager.Player.AddHealth(debugAmount);
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Restart"))
            {
                HideBattle();
                StartBattle();
            }
            if (GUILayout.Button("--->>> WIN <<<---"))
            {
                CardHandManager.Enemy.AddDamage(CardHandManager.Enemy.Health);
            }
        }
    }


    void OnEnable()
    {
        foreach (var c in GetComponentsInChildren<CardHandManager>())
        {
            c.AssignStaticAccessors();
        }

        foreach (var card in allCards)
        {
            Destroy(card.gameObject);
        }
        allCards.Clear();

        StartCoroutine(Loop());

        //PlayerCharacterAnimation?.ShowBasic();
        //EnemyCharacterAnimation?.ShowBasic();
    }

    IEnumerator Loop()
    {
        currentState = State.DealingFirstCards;

        if (playerSelectedCard != null)
        {
            playerSelectedCard.Deselect();
            ClearPlayerSelectedCard(playerSelectedCard);
        }

        List<CardInstance> cardsDeck = new();
        foreach (var typeColor in CardTypeColor.GenerateAllPossibleCards())
        {
            cardsDeck.Add(CreateCard(typeColor, true));
        }

        cardsDeck.Sort((CardInstance a, CardInstance b) => Random.Range(0, 100000)); // random order

        deck.AddCards_NoAnimation(cardsDeck);

        bool instantDeal = false;
        float cardDealDelay = 0.2f;

        if (!instantDeal)
            yield return new WaitForSeconds(cardDealDelay);
        //instantDeal = instantDeal || DialogueManager.SkipDialogWasPressedThisFrame;

        for (int i = 0; i < player.NumStartingCards; i++)
        {
            player.AddCard_WithAnimation(deck.GetTopCard(), true);
            if (!instantDeal)
                yield return new WaitForSeconds(cardDealDelay);
            //instantDeal = instantDeal || DialogueManager.SkipDialogWasPressedThisFrame;
        }

        for (int i = 0; i < enemy.NumStartingCards; i++)
        {
            enemy.AddCard_WithAnimation(deck.GetTopCard(), true);
            if (!instantDeal)
                yield return new WaitForSeconds(cardDealDelay);
            //instantDeal = instantDeal || DialogueManager.SkipDialogWasPressedThisFrame;
        }

        pile.AddCard_WithAnimation(deck.GetTopCard(), true);

        currentState = State.Player_DrawOrPlayCard;

        yield return null;
    }


    public void TryPlayCard(CardInstance cardToPlay, CardHandManager hand)
    {
        float randomRotation = Random.Range(0, 2) == 0
            ? Random.Range(-45f, -20f)
            : Random.Range(20f, 45f);
        randomRotation = Mathf.Clamp(randomRotation, -10, 10);

        StartCoroutine(TryPlayCard(cardToPlay, hand, randomRotation));
    }
    public IEnumerator TryPlayCard(CardInstance cardToPlay, CardHandManager hand, float targetRotation)
    {
        if (!CanPlayCard(cardToPlay))
        {
            Debug.LogError($"Why cant {hand.name} play card {cardToPlay} ???");
        }

        Debug.Log(hand.name + " played " + cardToPlay);

        cardToPlay.ShowTypeAndColor(showDodgeAsAnyColor: true);

        Sequence moveSequence = DOTween.Sequence();
        moveSequence.Join(cardToPlay.RectTransform.DORotate(Vector3.forward * targetRotation, 0.3f).SetEase(Ease.InSine));
        moveSequence.Join(cardToPlay.RectTransform.DOAnchorPos(pile.GetRandomAnchorDesiredPosition(), 0.2f));
        moveSequence.Join(cardToPlay.RectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad));
        numCardAnimationsInProgress++;
        moveSequence.OnComplete(() =>
        {
            numCardAnimationsInProgress--;
            cardToPlay.LastPlayedBy = hand;
            cardToPlay.RectTransform.rotation = Quaternion.Euler(0, 0, targetRotation);
            pile.SnapCardToPile(cardToPlay);
            StartCoroutine(SolveStateAfterCardIsPlayed(cardToPlay));
        });



        if (!pile.isInitialCard && !pile.GetLastPlayedCard().IsSpecialCard())
        {
            if (hand.isPlayer)
            {
                //if (EnemyCharacterAnimation?.IsAttackPrepared ?? false)
                {
                    //PlayerCharacterAnimation?.ShowBlock();
                    //EnemyCharacterAnimation?.ShowMyAttackWasBlocked();
                    numCharacterAnimationsInProgress++;
                    yield return new WaitForSeconds(delayAfterAnimation);
                    numCharacterAnimationsInProgress--;
                }
            }
            else
            {
                //if (PlayerCharacterAnimation?.IsAttackPrepared ?? false)
                {
                    //PlayerCharacterAnimation?.ShowMyAttackWasBlocked();
                    //EnemyCharacterAnimation?.ShowBlock();
                    numCharacterAnimationsInProgress++;
                    yield return new WaitForSeconds(delayAfterAnimation);
                    numCharacterAnimationsInProgress--;
                }
            }
        }

        if (hand.isPlayer)
        {
            //PlayerCharacterAnimation?.ShowPrepareAttack(cardToPlay.Type);
            //EnemyCharacterAnimation?.ShowBasic();
        }
        else
        {
            //PlayerCharacterAnimation?.ShowBasic();
            //EnemyCharacterAnimation?.ShowPrepareAttack(cardToPlay.Type);
        }
        numCharacterAnimationsInProgress++;
        yield return new WaitForSeconds(delayAfterAnimation);
        numCharacterAnimationsInProgress--;
    }

    public IEnumerator SolveStateAfterCardIsPlayed(CardInstance justPlayedCard)
    {
        while (numCharacterAnimationsInProgress != 0 || numCardAnimationsInProgress != 0)
            yield return new WaitForEndOfFrame();

        if (currentState == State.Player_DrawOrPlayCard ||
            currentState == State.Player_ReturningKick ||
            currentState == State.Player_ReturningFaint)
        {
            if (justPlayedCard.Type == CardType.Dodge)
            {
                UpperColorSelector.i.OnUpperCardPlayedByPlayer();
                //PlayerCharacterAnimation?.ShowBasic();
                //EnemyCharacterAnimation?.ShowBasic();
                yield return new WaitForSeconds(delayAfterAnimation);
            }
            else if (justPlayedCard.Type == CardType.Kick)
            {
                List<CardInstance> enemyKicks = enemy.GetCardsInHandOfType(CardType.Kick);
                if (enemyKicks.Count > 0)
                {
                    // Enemy counters with a Kick
                    CardInstance enemyKickCard = enemyKicks[Random.Range(0, enemyKicks.Count)];
                    currentState = State.Enemy_DrawOrPlayCard;
                    //PlayerCharacterAnimation?.ShowMyAttackWasBlocked();
                    //EnemyCharacterAnimation?.ShowBlock();
                    yield return new WaitForSeconds(delayAfterAnimation);
                    TryPlayCard(enemyKickCard, enemy);
                }
                else
                {
                    // Enemy skips a turn, player plays again
                    // succesfull kick
                    enemy.PutRandomCardToDeck_WithAnimation();
                    enemy.lastlyDamagedByCard = pile.GetLastPlayedCard();
                    enemy.AddDamage(1);
                    //PlayerCharacterAnimation?.ShowMyAttackDidHit();
                    //EnemyCharacterAnimation?.ShowHurt();
                    if (!IsPlaying)
                        yield break;
                    yield return new WaitForSeconds(delayAfterAnimation);
                    currentState = State.Player_DrawOrPlayCard;
                }
            }
            else if (justPlayedCard.Type == CardType.Feint)
            {
                // Check if the enemy has a Feint card
                List<CardInstance> enemyFeintCards = enemy.GetCardsInHandOfType(CardType.Feint);
                if (enemyFeintCards.Count > 0)
                {
                    // Enemy reacts with a Feint card
                    CardInstance enemyFeintCard = enemyFeintCards[Random.Range(0, enemyFeintCards.Count)];
                    currentState = State.Enemy_DrawOrPlayCard;
                    //PlayerCharacterAnimation?.ShowMyAttackWasBlocked();
                    //EnemyCharacterAnimation?.ShowBlock();
                    yield return new WaitForSeconds(delayAfterAnimation);
                    TryPlayCard(enemyFeintCard, enemy);
                }
                else
                {
                    // Enemy doesn't react, draw a card as penalty
                    enemy.lastlyDamagedByCard = justPlayedCard;
                    enemy.AddDamage(justPlayedCard.GetDamage());
                    //PlayerCharacterAnimation?.ShowMyAttackDidHit();
                    //EnemyCharacterAnimation?.ShowHurt();
                    if (!IsPlaying)
                        yield break;
                    yield return new WaitForSeconds(delayAfterAnimation);
                    currentState = State.Player_DrawOrPlayCard;
                }
            }
            else
            {
                SwapTurns();
            }
            UpdatePlayerHandInteractableState();
        }
        else if (currentState == State.Enemy_DrawOrPlayCard)
        {
            if (justPlayedCard.Type == CardType.Dodge)
            {
                UpperColorSelector.i.OnUpperCardPlayedByEnemy();
                //PlayerCharacterAnimation?.ShowBasic();
                //EnemyCharacterAnimation?.ShowBasic();
            }
            else if (justPlayedCard.Type == CardType.Kick)
            {
                List<CardInstance> playerKicks = player.GetCardsInHandOfType(CardType.Kick);
                if (playerKicks.Count > 0)
                {
                    currentState = State.Player_ReturningKick;
                }
                else
                {
                    // Player skips a turn, enemy plays again
                    // succesfull kick
                    player.PutRandomCardToDeck_WithAnimation();
                    player.lastlyDamagedByCard = pile.GetLastPlayedCard();
                    player.AddDamage(1);
                    //PlayerCharacterAnimation?.ShowHurt();
                    //EnemyCharacterAnimation?.ShowMyAttackDidHit();
                    if (!IsPlaying)
                        yield break;
                    yield return new WaitForSeconds(delayAfterAnimation);
                    currentState = State.Enemy_DrawOrPlayCard;
                    StartCoroutine(EnemyPlayTurn());
                }
            }
            else if (justPlayedCard.Type == CardType.Feint)
            {
                // Check if player has a Feint card
                List<CardInstance> playerFeintCards = player.GetCardsInHandOfType(CardType.Feint);
                if (playerFeintCards.Count > 0)
                {
                    currentState = State.Player_ReturningFaint;
                }
                else
                {
                    player.lastlyDamagedByCard = justPlayedCard;
                    player.AddDamage(justPlayedCard.GetDamage());
                    //PlayerCharacterAnimation?.ShowHurt();
                    //EnemyCharacterAnimation?.ShowMyAttackDidHit();
                    if (!IsPlaying)
                        yield break;
                    yield return new WaitForSeconds(delayAfterAnimation);
                    currentState = State.Enemy_DrawOrPlayCard;
                    StartCoroutine(EnemyPlayTurn());
                }
            }
            else
            {
                SwapTurns();
            }
            UpdatePlayerHandInteractableState();
        }
    }

    public void SwapTurns()
    {
        if (currentState == State.Player_DrawOrPlayCard)
        {
            currentState = State.Enemy_DrawOrPlayCard;
            StartCoroutine(EnemyPlayTurn());
        }
        else if (currentState == State.Enemy_DrawOrPlayCard)
        {
            currentState = State.Player_DrawOrPlayCard;
        }
        else if (currentState == State.Player_PickColor)
        {
            //PlayerCharacterAnimation?.ShowBasic();
            //EnemyCharacterAnimation?.ShowBasic();

            if (UpperColorSelector.i.colorPicked)
                currentState = State.Enemy_DrawOrPlayCard;

            StartCoroutine(EnemyPlayTurn());
        }
        else
        {
            Debug.LogError("Weird state " + currentState);
        }
    }

    public IEnumerator EnemyPlayTurn()
    {
        if (currentState == State.Player_PickColor)
        {
            yield return null;
        }
        if (currentState != State.Enemy_DrawOrPlayCard)
        {
            Debug.LogError("Unexpected state " + currentState);
        }

        List<CardInstance> playableCards = new List<CardInstance>();
        foreach (var card in enemy.GetCardsInHand())
        {
            if (CanPlayCard(card))
            {
                playableCards.Add(card);
            }
        }

        if (playableCards.Count > 0)
        {
            CardInstance selectedEnemyCard = playableCards[Random.Range(0, playableCards.Count)];
            TryPlayCard(selectedEnemyCard, CardHandManager.Enemy);
        }
        else
        {
            Debug.Log("Enemy has no playable cards. Drawing a card.");
            if (!deck.HasCards())
            {
                deck.TryFillCardsFromPileOrHands();
                while (isCardMoveAnimationInProgress)
                    yield return new WaitForSecondsRealtime(0.1f);
            }

            StartCoroutine(HandDrawCardFromDeck(enemy));
        }
    }

    public IEnumerator HandDrawCardFromDeck(CardHandManager hand)
    {
        var cardToDraw = deck.GetTopCard();
        Debug.Log(hand.gameObject.name + " drew " + cardToDraw);
        hand.AddCard_WithAnimation(cardToDraw, false);

        if (!pile.isInitialCard)
        {
            var lastPlayedCard = pile.GetLastPlayedCard();
            if (
                hand.lastlyDamagedByCard != lastPlayedCard && // already damaged by this card previously
                lastPlayedCard.LastPlayedBy != hand // dont damage by our own card
            )
            {
                if (lastPlayedCard.GetDamage() > 0)
                {
                    if (hand.isPlayer)
                    {
                        //PlayerCharacterAnimation?.ShowHurt();
                        //EnemyCharacterAnimation?.ShowMyAttackDidHit();
                    }
                    else
                    {
                        //PlayerCharacterAnimation?.ShowMyAttackDidHit();
                        //EnemyCharacterAnimation?.ShowHurt();
                    }
                    hand.lastlyDamagedByCard = lastPlayedCard;
                    hand.AddDamage(lastPlayedCard.GetDamage());
                    if (!IsPlaying)
                        yield break;
                    yield return new WaitForSeconds(delayAfterAnimation);
                }

                if (!IsPlaying)
                {
                    // player died, game ended
                    yield break;
                }
            }
        }

        //PlayerCharacterAnimation?.ShowBasic();
        //EnemyCharacterAnimation?.ShowBasic();
        yield return new WaitForSeconds(delayAfterAnimation);


        if (!deck.HasCards())
        {
            deck.TryFillCardsFromPileOrHands();
        }

        SwapTurns();
    }


    public void UpdatePlayerHandInteractableState()
    {
        for (int i = 0; i < player.transform.childCount; i++)
        {
            var card = player.transform.GetChild(i).GetComponent<CardInstance>();
            if (!CanPlayCard(card))
            {
                card.DisableCardClick(true);
                card.AllowDragToPile(false);
            }
            else if (isDealingFirstCards || card == playerSelectedCard)
            {
                card.AllowDragToPile(false);
            }
            else
            {
                card.OnClickDoOnly(card.ToggleSelection);
                card.AllowDragToPile(true);
            }
        }
    }

    public bool CanPlayCard(CardInstance card)
    {
        var pileLastCard = pile.GetLastPlayedCard();

        if (currentState == State.Player_ReturningFaint)
        {
            if (card.Type == CardType.Feint)
                return true;
            else return false;
        }
        else if (currentState == State.Player_ReturningKick)
        {
            if (card.Type == CardType.Kick)
                return true;
            else return false;
        }
        else if (currentState == State.Player_PickColor)
        {
            return false;
        }

        // Rule for Dodge cards: Can be played on any card except Kick or Feint
        if (card.Type == CardType.Dodge)
        {
            if (pileLastCard.Type != CardType.Kick && pileLastCard.Type != CardType.Feint)
                return true;
        }

        var neededColor = pileLastCard.Color;

        // If the last card was an Dodge, use the chosen color
        if (pileLastCard.Type == CardType.Dodge)
        {
            if (!pile.isInitialCard)
            {
                neededColor = UpperColorSelector.i.GetPickedColor();
            }
        }

        // Regular case: Match color or type
        if (card.Color == neededColor || card.Type == pileLastCard.Type)
            return true;

        return false;
    }

    public void ClearPlayerSelectedCard(CardInstance card)
    {
        if (playerSelectedCard == card)
        {
            playerSelectedCard = null;
        }
    }

    public void SetPlayerSelectedCard(CardInstance card)
    {
        playerSelectedCard?.Deselect();
        playerSelectedCard = card;
    }

    CardInstance CreateCard(CardTypeColor typeColor, bool showUpperAsColorLess)
    {
        CardInstance card = Instantiate(configCardPrefab, transform);
        card.Rect.SetParent(transform, true);
        card.TypeColor = typeColor;
        card.ShowTypeAndColor(showDodgeAsAnyColor: true);
        card.gameObject.name = $"Card {typeColor}";
        allCards.Add(card);
        return card;
    }
}
