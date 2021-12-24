using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DreamersInc.UI;
namespace Dreamers.Global
{
    public class UIManager : MonoBehaviour
    {

        public static UIManager instance;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            if (instance == null)
                instance = this;
            if (instance != this)
                Destroy(this);
            UIPanelPrefab = Resources.Load("UI Prefabs/Panel") as GameObject;
            ButtonPrefab = Resources.Load("UI Prefabs/Button") as GameObject;
            TextBoxPrefab = Resources.Load("UI Prefabs/Text") as GameObject;
            SliderPrefab = Resources.Load("UI Prefabs/Slider") as GameObject;

        }

        private GameObject _uICanvas;

        public GameObject UICanvas()
        {
            GameObject Instance;
            if (!_uICanvas)
            {
                Instance = new GameObject
                {
                    name = "Canvas"
                };
                GameObject EventInstance = new GameObject
                {
                    name = "Event System"
                };
                Instance.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                EventInstance.AddComponent<StandaloneInputModule>();
                CanvasScaler scaler = Instance.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.0f;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                Instance.AddComponent<GraphicRaycaster>();
                _uICanvas = Instance;
            }
            else
                Instance = _uICanvas;
            return Instance;
        }

        private GameObject UIPanelPrefab;
        private GameObject TextBoxPrefab;
        private GameObject ButtonPrefab;
        private GameObject SliderPrefab;
        // write anchoring system



        public GameObject GetPanel(Transform Parent, Vector2 Size, Vector2 Position)
        {
            GameObject temp = Instantiate(UIPanelPrefab);
            temp.transform.SetParent(Parent, false);
            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = new Vector2(0, 1);
            PanelRect.anchorMin = new Vector2(0, 1);
            PanelRect.sizeDelta = Size;
            PanelRect.anchoredPosition = Position;


            return temp;
        }

        public GameObject GetPanel(Transform Parent, Vector2 Size, Vector2 Position, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject temp = Instantiate(UIPanelPrefab);
            temp.transform.SetParent(Parent, false);
            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = anchorMax;
            PanelRect.anchorMin = anchorMin;
            PanelRect.sizeDelta = Size;
            PanelRect.anchoredPosition = Position;

            return temp;
        }

        public GameObject GetPanel(Transform Parent, Vector2 Size, Vector2 Position = default, LayoutGroup layout = LayoutGroup.None, string Name = "New UI Panel")
        {
            GameObject temp = Instantiate(UIPanelPrefab);
            temp.transform.SetParent(Parent, false);
            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = new Vector2(0, 1);
            PanelRect.anchorMin = new Vector2(0, 1);
            PanelRect.sizeDelta = Size;
            PanelRect.anchoredPosition = Position;
            switch (layout)
            {
                case LayoutGroup.Grid:
                    temp.AddComponent<GridLayoutGroup>();
                    break;
                case LayoutGroup.Horizontal:
                    temp.AddComponent<HorizontalLayoutGroup>();
                    break;
                case LayoutGroup.Vertical:
                    temp.AddComponent<VerticalLayoutGroup>();
                    break;
            }
            temp.name = Name;

            return temp;
        }




        public Text TextBox(Transform Parent, Vector2 Size)
        {

            GameObject temp = Instantiate(TextBoxPrefab);
            temp.transform.SetParent(Parent, false);
            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = new Vector2(0, 1);
            PanelRect.anchorMin = new Vector2(0, 1);
            PanelRect.sizeDelta = Size;
            return temp.GetComponent<Text>();
        }
        public Button UIButton(Transform Parent, string TextToDisplay)
        {
            Button temp = Instantiate(ButtonPrefab).GetComponent<Button>();
            temp.GetComponentInChildren<Text>().text = TextToDisplay;
            temp.transform.SetParent(Parent, false);
            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = new Vector2(0, 1);
            PanelRect.anchorMin = new Vector2(0, 1);

            return temp;
        }
        public Button UIButton(Transform Parent, string TextToDisplay, Vector2 Size, Vector2 Position)
        {
            Button temp = UIButton(Parent, TextToDisplay);
            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.sizeDelta = Size;
            PanelRect.anchoredPosition = Position;

            return temp;
        }
        public Image GetImage(Transform Parent, Sprite sprite = default, string name = "New Image")
        {
            GameObject temp = new GameObject();
            temp.name = name;
            Image image = temp.AddComponent<Image>();
            image.sprite = sprite;
            temp.transform.SetParent(Parent, false);

            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = new Vector2(0, 1);
            PanelRect.anchorMin = new Vector2(0, 1);

            return image;

        }

        public TabButton TabButton(Transform Parent, string content = default, string name = "New Image", Sprite sprite = default)
        {
            GameObject temp = new GameObject();
            temp.name = name;
            Image image = temp.AddComponent<Image>();
            if (!sprite)
                image.sprite = sprite;
            temp.transform.SetParent(Parent, false);
            TabButton button = temp.AddComponent<TabButton>();


            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = new Vector2(0, 1);
            PanelRect.anchorMin = new Vector2(0, 1);
            if (content != default)
            {
                AddTextBox(content, temp.transform, 40, TextAnchor.MiddleCenter);
            }

            return button;
        }

        public Text AddTextBox(string input, Transform parent, int size = 14, TextAnchor align = default)
        {

            GameObject temp = new GameObject();
            temp.transform.SetParent(parent);
            temp.name = "TextBox";


            Text TextBox = temp.AddComponent<Text>();
            TextBox.text = input;
            TextBox.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            TextBox.color = Color.black;
            TextBox.fontSize = size;
            TextBox.alignment = align;
            RectTransform PanelRect = temp.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchoredPosition = Vector2.zero;
            PanelRect.anchorMax = new Vector2(1, 1);
            PanelRect.anchorMin = new Vector2(0, 0);
            return TextBox;
        }


        public Slider UISlider(Transform Parent, bool whole = true, int minValue = 1, int maxValue = 10)
        {
            Slider slider = Instantiate(SliderPrefab).GetComponent<Slider>();
            slider.transform.SetParent(Parent, false);
            RectTransform PanelRect = slider.GetComponent<RectTransform>();
            PanelRect.pivot = new Vector2(0.5f, .5f);
            PanelRect.anchorMax = new Vector2(0, 1);
            PanelRect.anchorMin = new Vector2(0, 1);
            slider.wholeNumbers = whole;
            slider.maxValue = maxValue;
            slider.minValue = minValue;
            return slider;
        }

        public Slider UISlider(Transform Parent, Vector2 Size, Vector2 Position, bool whole = true, int minValue = 1, int maxValue = 10)
        {

            Slider slider = UISlider(Parent, whole, minValue, maxValue);
            RectTransform PanelRect = slider.GetComponent<RectTransform>();
            PanelRect.sizeDelta = Size;
            PanelRect.anchoredPosition = Position;
            return slider;
        }
    }

    public enum LayoutGroup { None, Horizontal, Vertical, Grid }
}