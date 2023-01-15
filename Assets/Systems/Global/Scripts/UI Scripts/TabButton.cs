using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
namespace DreamersInc.UI
{
    [RequireComponent(typeof(Image))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        private TabGroup tabGroup => GetComponentInParent<TabGroup>();
        public Image background;
        public UnityEvent OnTabSelected;
        public UnityEvent OnTabDeslected;
        [SerializeField]
        // Start is called before the first frame update
        void Awake()
        {
            tabGroup.Subscribe(this);
            background = GetComponent<Image>();
            OnTabDeslected = new UnityEvent();
            OnTabSelected = new UnityEvent();
        }

        public void Select()
        {
            if (OnTabSelected != null)
            {
                OnTabSelected.Invoke();

            }
        }
        public void Deselect()
        {
            if (OnTabDeslected != null)
                OnTabDeslected.Invoke();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup.OnTabEnter(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            tabGroup.OnTabSelected(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tabGroup.OnTabExit();
        }

    }

}