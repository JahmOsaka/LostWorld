using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class DummyFeedback : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void PlayHitFeedback()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRedRoutine());
    }

    private IEnumerator FlashRedRoutine()
    {
        sr.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        sr.color = originalColor;
    }
}