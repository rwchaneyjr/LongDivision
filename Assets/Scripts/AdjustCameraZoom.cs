using UnityEngine;

public class AdjustCameraZoom : MonoBehaviour
{
    public Camera cam;
    public RectTransform boardRoot;

    [Header("Zoom Settings")]
    public float baseSize = 4.5f;  // starting orthographic size
    public float zoomPerRow = 0.7f;  // 🔼 increase to zoom out more per row
    public float smooth = 5f;    // smoothing for both pan & zoom

    [Header("Pan Up Settings")]
    public float panPerRow = 0.4f;   // 🔼 how much to move camera up per row

    int baseRowCount;
    int lastRowCount = -1;
    float baseY;

    void Start()
    {
        if (!cam) cam = Camera.main;
        if (!cam || !boardRoot) return;

        // Remember starting row count and camera Y
        baseRowCount = CountTotalRows();
        lastRowCount = baseRowCount;
        baseY = cam.transform.position.y;
    }

    void LateUpdate()
    {
        if (!cam || !boardRoot) return;

        int rowCount = CountTotalRows();
        if (rowCount == lastRowCount) return;   // nothing changed this frame

        lastRowCount = rowCount;

        // ---------- ZOOM ----------
        float targetSize = baseSize + rowCount * zoomPerRow;
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetSize,
            Time.deltaTime * smooth
        );

        // ---------- PAN UP ----------
        float rowDelta = rowCount - baseRowCount;
        float targetY = baseY + rowDelta * panPerRow;

        Vector3 pos = cam.transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * smooth);
        cam.transform.position = pos;
    }

    int CountTotalRows()
    {
        int count = 0;
        foreach (Transform child in boardRoot)
        {
            if (child.gameObject.activeInHierarchy)
                count++;
        }
        return count;
    }
}
