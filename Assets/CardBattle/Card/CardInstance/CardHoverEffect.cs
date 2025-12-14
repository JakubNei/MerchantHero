using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1f);
    private bool isSelected;


    void OnDisable()
    {
        transform.localScale = Vector3.one;
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (isSelected)
        {
            transform.DOScale(hoverScale, 0.2f).SetEase(Ease.OutQuad);
        }
        else
        {
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            transform.DOScale(hoverScale, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
        }
    }
}
