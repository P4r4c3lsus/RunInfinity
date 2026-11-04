using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunInfinity.Adicional
{
    // Centra solo la geometria del texto; no altera botones, fuente ni anclas responsive.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class RIButtonLabelCenter : MonoBehaviour
    {
        [SerializeField] private RectTransform buttonRect;
        // La cara del boton queda ligeramente por encima de su sombra inferior.
        [SerializeField, Range(0f, 1f)] private float verticalCenter = 0.53f;
        private TextMeshProUGUI label;

        private void OnEnable()
        {
            label = GetComponent<TextMeshProUGUI>();
            label.OnPreRenderText -= CenterLetters;
            label.OnPreRenderText += CenterLetters;
            RefreshLabel();
        }

        private void OnDisable()
        {
            if (label == null) return;
            label.OnPreRenderText -= CenterLetters;
            RefreshLabel();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (label != null) RefreshLabel();
        }

        private void OnValidate()
        {
            if (label != null) RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (label == null) return;
            label.havePropertiesChanged = true;
            label.SetVerticesDirty();
        }

        // Image.preserveAspect puede dibujar un sprite menor que su RectTransform.
        // Su ubicacion dentro de ese rectangulo depende del pivot del boton.
        private Rect VisibleButtonRect()
        {
            Image image = buttonRect.GetComponent<Image>();
            if (image == null) return buttonRect.rect;
            Rect rect = image.GetPixelAdjustedRect();
            Sprite sprite = image.overrideSprite != null ? image.overrideSprite : image.sprite;
            if (sprite == null || image.type != Image.Type.Simple || !image.preserveAspect ||
                rect.width <= 0 || rect.height <= 0 || sprite.rect.height <= 0)
                return rect;
            float ratio = sprite.rect.width / sprite.rect.height;
            if (ratio <= 0) return rect;
            if (ratio > rect.width / rect.height)
            {
                float height = rect.width / ratio;
                rect.y += (rect.height - height) * buttonRect.pivot.y;
                rect.height = height;
            }
            else
            {
                float width = rect.height * ratio;
                rect.x += (rect.width - width) * buttonRect.pivot.x;
                rect.width = width;
            }
            return rect;
        }

        private void CenterLetters(TMP_TextInfo info)
        {
            if (buttonRect == null || info.characterCount == 0) return;
            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, 0);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, 0);
            bool found = false;
            for (int i = 0; i < info.characterCount; i++)
            {
                TMP_CharacterInfo character = info.characterInfo[i];
                if (!character.isVisible) continue;
                Vector3[] vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 vertex = vertices[character.vertexIndex + j];
                    min = Vector3.Min(min, vertex);
                    max = Vector3.Max(max, vertex);
                }
                found = true;
            }
            if (!found) return;
            Rect rect = VisibleButtonRect();
            Vector3 point = new Vector3(rect.center.x,
                Mathf.Lerp(rect.yMin, rect.yMax, verticalCenter), 0);
            Vector3 target = label.transform.InverseTransformPoint(buttonRect.TransformPoint(point));
            Vector3 shift = target - (min + max) * 0.5f;
            shift.z = 0;
            for (int i = 0; i < info.meshInfo.Length; i++)
            {
                TMP_MeshInfo mesh = info.meshInfo[i];
                for (int j = 0; j < mesh.vertexCount; j++) mesh.vertices[j] += shift;
            }
        }
    }
}
