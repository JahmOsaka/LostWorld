using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class HiddenAreaCover : MonoBehaviour
{
    [Header("Cover Visuals")]
    [Tooltip("Sprites / tilemaps that make up the black cover. Auto-filled from children if empty.")]
    [SerializeField] private SpriteRenderer[] coverSprites;
    [SerializeField] private Tilemap[] coverTilemaps;

    [Header("Fade")]
    [Range(0f, 1f)][SerializeField] private float hiddenAlpha = 1f;
    [Range(0f, 1f)][SerializeField] private float revealedAlpha = 0f;
    [Tooltip("Alpha units per second when revealing.")]
    [SerializeField] private float fadeOutSpeed = 3f;
    [Tooltip("Alpha units per second when covering again.")]
    [SerializeField] private float fadeInSpeed = 1.5f;

    [Header("Behavior")]
    [Tooltip("If true, the cover never returns after being revealed once.")]
    [SerializeField] private bool stayRevealed = false;
    
    public string areaID;

    private float currentAlpha;
    private bool playerTouchedThisStep;
    private bool playerInside;
    private bool permanentlyRevealed;

    private SpriteRenderer[] sprites;
    private Tilemap[] tilemaps;
    private Color[] spriteBaseColors;
    private Color[] tilemapBaseColors;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        sprites = (coverSprites != null && coverSprites.Length > 0)
            ? coverSprites
            : GetComponentsInChildren<SpriteRenderer>(true);
        tilemaps = (coverTilemaps != null && coverTilemaps.Length > 0)
            ? coverTilemaps
            : GetComponentsInChildren<Tilemap>(true);

        spriteBaseColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) spriteBaseColors[i] = sprites[i].color;

        tilemapBaseColors = new Color[tilemaps.Length];
        for (int i = 0; i < tilemaps.Length; i++) tilemapBaseColors[i] = tilemaps[i].color;

        currentAlpha = hiddenAlpha;
        ApplyAlpha(currentAlpha);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerTouchedThisStep = true;
    }

    private void FixedUpdate()
    {
        playerInside = playerTouchedThisStep;
        playerTouchedThisStep = false;

        if (playerInside && stayRevealed) permanentlyRevealed = true;
    }

    private void Update()
    {
        bool shouldReveal = playerInside || permanentlyRevealed;
        float target = shouldReveal ? revealedAlpha : hiddenAlpha;
        float speed = shouldReveal ? fadeOutSpeed : fadeInSpeed;

        if (!Mathf.Approximately(currentAlpha, target))
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, target, speed * Time.deltaTime);
            ApplyAlpha(currentAlpha);
        }
    }

    private void ApplyAlpha(float alpha)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null) continue;
            Color c = spriteBaseColors[i];
            c.a = spriteBaseColors[i].a * alpha;
            sprites[i].color = c;
            sprites[i].enabled = alpha > 0.001f;
        }

        for (int i = 0; i < tilemaps.Length; i++)
        {
            if (tilemaps[i] == null) continue;
            Color c = tilemapBaseColors[i];
            c.a = tilemapBaseColors[i].a * alpha;
            tilemaps[i].color = c;
        }
    }

    public void SetRevealedImmediate()
    {
        permanentlyRevealed = true;
        currentAlpha = revealedAlpha;
        ApplyAlpha(currentAlpha);
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 0f, 0f, 0.6f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}