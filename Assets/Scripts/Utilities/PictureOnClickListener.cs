using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PictureOnClickListener : MonoBehaviour, IPointerClickHandler
{
    public UnityAction action;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 사진 클릭 시 이벤트
        action();
        Debug.Log(eventData.ToString());
        
    }
}