using UnityEngine;

public class ProceduralSeamlessParallax : MonoBehaviour
{
    [Header("Procedural Scenery Settings")]
    [Tooltip("Assign tree prefabs to be spawned here.")]
    public GameObject[] treePrefabs;

    [Tooltip("Number of trees to spawn in one chunk.")]
    public int numberOfTrees = 10;

    [Tooltip("The width of the parallax loop chunk (distance before it repeats).")]
    public float spawnWidth = 25f;

    [Tooltip("Padding from the edges of each segment to prevent trees from overlapping. Keep it lower than (spawnWidth / numberOfTrees / 2).")]
    public float segmentPadding = 0.5f;

    [Tooltip("Randomize tree scale (min and max).")]
    public float minScale = 0.8f, maxScale = 1.2f;

    [Header("Background Ground Settings")]
    [Tooltip("Assign a simple Square Sprite Prefab here to act as the base ground for this layer.")]
    public GameObject bgGroundPrefab;

    [Tooltip("The Y position of the ground SURFACE (where trees stand).")]
    public float bgGroundYPosition = -2f;

    [Tooltip("The thickness (Y scale) of the background ground. It will grow downwards from the surface.")]
    public float bgGroundThickness = 10f;

    [Tooltip("Order in Layer for the ground. Set this higher than the trees' sorting order to make it render in front.")]
    public int bgGroundSortingOrder = 10;

    [Tooltip("Vertical offset to prevent trees from sinking into the ground. Increase to move trees up.")]
    public float spawnYOffset = 0f;

    [Header("Parallax Settings")]
    [Tooltip("0 = Moves with camera (Foreground), 1 = Static background (Sky). Try 0.5 for Midground.")]
    public float parallaxMultiplierX = 0.5f;

    [Tooltip("Vertical Parallax. 0 = Fixed to camera Y, 1 = Fixed to world Y. Try 0.8 to 0.95 so it moves slightly when jumping.")]
    public float parallaxMultiplierY = 0.9f;

    private Transform cameraTransform;
    private float startPosX;
    private float startPosY;

    void Awake()
    {
        GenerateSeamlessGround();
        GenerateSeamlessTrees();
    }

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        startPosX = transform.position.x;
        startPosY = transform.position.y;
    }

    void GenerateSeamlessGround()
    {
        if (bgGroundPrefab == null) return;

        float[] offsets = { -spawnWidth, 0f, spawnWidth };

        foreach (float offset in offsets)
        {
            float actualGroundY = (transform.position.y + bgGroundYPosition) - (bgGroundThickness / 2f);

            Vector3 spawnPos = new Vector3(transform.position.x + offset, actualGroundY, transform.position.z);
            GameObject ground = Instantiate(bgGroundPrefab, transform);
            ground.transform.position = spawnPos;
            ground.transform.localScale = new Vector3(spawnWidth, bgGroundThickness, 1f);

            SpriteRenderer sr = ground.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = bgGroundSortingOrder;
            }
        }
    }

    void GenerateSeamlessTrees()
    {
        if (treePrefabs.Length == 0) return;

        float segmentWidth = spawnWidth / numberOfTrees;
        float startX = -spawnWidth / 2f;

        for (int i = 0; i < numberOfTrees; i++)
        {
            float segmentMin = startX + (i * segmentWidth) + segmentPadding;
            float segmentMax = startX + ((i + 1) * segmentWidth) - segmentPadding;

            if (segmentMin > segmentMax)
            {
                segmentMin = startX + (i * segmentWidth) + (segmentWidth / 2f);
                segmentMax = segmentMin;
            }

            float randomX = Random.Range(segmentMin, segmentMax);
            float spawnY = transform.position.y + bgGroundYPosition + spawnYOffset;

            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
            float randomScale = Random.Range(minScale, maxScale);
            float flipX = Random.value > 0.5f ? 1f : -1f;
            Vector3 scale = new Vector3(randomScale * flipX, randomScale, 1f);

            float[] offsets = { -spawnWidth, 0f, spawnWidth };

            foreach (float offset in offsets)
            {
                Vector3 spawnPos = new Vector3(transform.position.x + randomX + offset, spawnY, transform.position.z);
                GameObject newTree = Instantiate(prefab, transform);
                newTree.transform.position = spawnPos;
                newTree.transform.localScale = scale;
            }
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        float tempX = cameraTransform.position.x * (1 - parallaxMultiplierX);
        float distanceX = cameraTransform.position.x * parallaxMultiplierX;
        float distanceY = cameraTransform.position.y * parallaxMultiplierY;

        transform.position = new Vector3(startPosX + distanceX, startPosY + distanceY, transform.position.z);

        if (tempX > startPosX + spawnWidth)
        {
            startPosX += spawnWidth;
        }
        else if (tempX < startPosX - spawnWidth)
        {
            startPosX -= spawnWidth;
        }
    }
}