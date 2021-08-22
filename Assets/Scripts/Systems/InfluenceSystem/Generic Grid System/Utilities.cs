using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreamersInc.Utils
{
    public static class Utilities
    {
        public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPos = default(Vector3), int fontSize = 40, Color color=default(Color),TextAnchor textAnchor = default(TextAnchor), TextAlignment alignment = default, int sortingLayer = default(int)) {
            if (color == null) color = Color.white;
            return CreateWorldText( parent,  text,  localPos,  fontSize, color,  textAnchor, alignment, sortingLayer);
        }

        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPos, int fontSize, Color color, TextAnchor textAnchor, TextAlignment alignment, int sortingLayer) {

            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPos;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = alignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingLayer;
            return textMesh;
        }
    }
}
