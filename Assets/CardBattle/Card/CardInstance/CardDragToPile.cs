using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class CardDragToPile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    PileManager pile => PileManager.I;

    CardInstance thisCard;
    RectTransform rectTransform;

    Vector2 originalPosition;
    Vector2 lastPosition;
    Vector2 offset;

    bool isDragging;
    float rotationZ = 0;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        thisCard = GetComponent<CardInstance>();
    }

    void Update()
    {
        if (isDragging)
        {
            var delta = lastPosition - rectTransform.anchoredPosition;
            lastPosition = rectTransform.anchoredPosition;
            rotationZ += delta.x * 0.5f;
            rotationZ = Math.Clamp(rotationZ, -60, 60);
            transform.rotation = Quaternion.Euler(0, 0, rotationZ);
            rotationZ = Mathf.Lerp(rotationZ, 0, Time.deltaTime * 10);
        }
    }
    Vector2 GetLocalPointerPosition(PointerEventData eventData)
    {
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponentInParent<Canvas>().GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        );
        return localPointerPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CardBattle.I.CanPlayerPlayCardInHand)
            return;
        var localPointerPosition = GetLocalPointerPosition(eventData);
        offset = rectTransform.anchoredPosition - localPointerPosition;
        originalPosition = rectTransform.anchoredPosition;
        lastPosition = originalPosition;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CardBattle.I.CanPlayerPlayCardInHand)
            return;
        var localPointerPosition = GetLocalPointerPosition(eventData);
        rectTransform.anchoredPosition = localPointerPosition + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CardBattle.I.CanPlayerPlayCardInHand)
            return;

        isDragging = false;

        if (pile.IsCardOverPile(rectTransform, eventData.position, eventData.pressEventCamera))
        {
            // keep rotation close to current
            float rotationOffset = Random.Range(-5f, 5f);
            float currentRotation = thisCard.Rect.rotation.eulerAngles.z;
            float targetRotation = currentRotation + rotationOffset;
            targetRotation = Mathf.Repeat(targetRotation, 360f);
            targetRotation = Mathf.Clamp(targetRotation, -10, 10);

            StartCoroutine(CardBattle.I.TryPlayCard(thisCard, CardHandManager.Player, targetRotation));
        }
        else
        {
            CardHandManager.Player.ReorganizeHand();
        }

        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
    }
}
