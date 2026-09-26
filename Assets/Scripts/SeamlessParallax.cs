using UnityEngine;

public class SeamlessParallax : MonoBehaviour
{
    [Tooltip("1 = เลื่อนตามกล้องเป๊ะ (ฉากหลังสุด), 0.5 = ขยับช้าๆ (ระยะกลาง), 0 = อยู่นิ่ง")]
    public float parallaxEffect = 0.5f;

    [Tooltip("ระยะความกว้างของกลุ่มต้นไม้ (ลองกะระยะให้คลุมหน้าจอพอดี เช่น 20 หรือ 30)")]
    public float layerWidth = 20f;

    private Transform cameraTransform;
    private Vector3 startPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        startPosition = transform.position;

        int childCount = transform.childCount;
        Transform[] originalChildren = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            originalChildren[i] = transform.GetChild(i);
        }

        CreateClone(originalChildren, layerWidth);
        CreateClone(originalChildren, -layerWidth);
    }

    void CreateClone(Transform[] originals, float offsetX)
    {
        GameObject cloneObj = new GameObject("CloneExtension");
        cloneObj.transform.SetParent(this.transform);
        cloneObj.transform.localPosition = new Vector3(offsetX, 0, 0);

        foreach (Transform orig in originals)
        {
            GameObject childClone = Instantiate(orig.gameObject, cloneObj.transform);
            childClone.transform.localPosition = orig.localPosition;
            childClone.transform.localRotation = orig.localRotation;
            childClone.transform.localScale = orig.localScale;
        }
    }

    void LateUpdate()
    {
        float temp = (cameraTransform.position.x * (1 - parallaxEffect));
        float distance = (cameraTransform.position.x * parallaxEffect);

        transform.position = new Vector3(startPosition.x + distance, transform.position.y, transform.position.z);

        if (temp > startPosition.x + layerWidth)
        {
            startPosition.x += layerWidth;
        }
        else if (temp < startPosition.x - layerWidth)
        {
            startPosition.x -= layerWidth;
        }
    }
}