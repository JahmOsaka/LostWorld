using UnityEngine;

public class RadialLayoutUtility : MonoBehaviour
{
    [Tooltip("Radius distance from the center of the wheel.")]
    public float radius = 100f;

    [Tooltip("Starting angle offset in degrees.")]
    public float startAngle = 45f;

    private void OnValidate()
    {
        LayoutChildren();
    }

    [ContextMenu("Execute Layout")]
    public void LayoutChildren()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        float angleStep = 360f / childCount;
        float currentAngle = startAngle;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;

            float rad = currentAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;

            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(x, y);
            }

            currentAngle += angleStep;
        }
    }
}