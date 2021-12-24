using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PopUpMouseControl : MonoBehaviour, IPointerExitHandler
{


    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(this.gameObject);
    }



}
