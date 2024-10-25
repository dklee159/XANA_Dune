using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    Image imageBackGround;
    Image imageController;

    Vector2 touchPosition;
    Vector2 touchScreenPosition;
    void Start()
    {
        imageBackGround = transform.GetChild(0).gameObject.GetComponent<Image>();
        imageController = imageBackGround.transform.GetChild(0).gameObject.GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        touchScreenPosition = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<Image>().rectTransform, eventData.position, eventData.pressEventCamera, out touchScreenPosition))
        {
            touchScreenPosition.x -= imageBackGround.rectTransform.sizeDelta.x / 2;
            touchScreenPosition.y -= imageBackGround.rectTransform.sizeDelta.y / 2;
            imageBackGround.transform.localPosition= touchScreenPosition;
        }
        imageBackGround.enabled = true;
        imageController.enabled = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            imageBackGround.rectTransform, eventData.position, eventData.pressEventCamera, out touchPosition))
        {
            touchPosition.x = (touchPosition.x / imageBackGround.rectTransform.sizeDelta.x);
            touchPosition.y = (touchPosition.y / imageBackGround.rectTransform.sizeDelta.y);

            touchPosition = new Vector2(touchPosition.x * 2 - 1, touchPosition.y * 2 - 1);

            touchPosition = (touchPosition.magnitude > 1) ? touchPosition.normalized : touchPosition;


            imageController.rectTransform.anchoredPosition = new Vector2(
                touchPosition.x * imageBackGround.rectTransform.sizeDelta.x / 2,
                touchPosition.y * imageBackGround.rectTransform.sizeDelta.y / 2);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        imageController.rectTransform.anchoredPosition = new Vector2(0, 0);
        touchPosition = Vector2.zero;

        imageBackGround.enabled = false;
        imageController.enabled = false;
    }
    public float Horizontal()
    {
        return touchPosition.normalized.x;
    }
    public float Vertical()
    {
        return touchPosition.normalized.y;
    }
}
