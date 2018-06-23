using UnityEngine;

public class PuzzleCamera : MonoBehaviour
{
    public static PuzzleCamera Instance;
    public static Camera Main { get { return null != Instance ? Instance.main : null; } }

    const float sizeExtendScale = 1.5f;
    const float minRangeScale = 3f; 
    const float cameraMoveSpeed = 2f;
    const float zoomScale = 125f;

    Camera main;
    Vector2 size;
    Bounds bounds;
    Vector2 zoomRange;

    void Awake()
    {
        Instance = this;
        main = Camera.main;
        PuzzleTouch.onFingerDrag += OnCameraMove;
        PuzzleTouch.onFingerPinched += OnCameraZoom;
    }

    void OnDestroy()
    {
        PuzzleTouch.onFingerDrag -= OnCameraMove;
        PuzzleTouch.onFingerPinched -= OnCameraZoom;
        Instance = null;
    }

    public Bounds Bounds { get { return bounds; } }

    public void Init(Vector2 size)
    {
        Vector3 pos = transform.localPosition;
        pos.x = size.x / 2;
        pos.y = size.y / 2;
        transform.localPosition = pos;

        Vector2 extendedSize = size * sizeExtendScale;
        this.size = extendedSize;
        float graphAspect = extendedSize.x / extendedSize.y;
        if (graphAspect < main.aspect)
            main.orthographicSize = extendedSize.y / 2f;
        else
            main.orthographicSize = extendedSize.x / (2f * main.aspect);

        Vector2 orthoSize = new Vector2(main.aspect * main.orthographicSize, main.orthographicSize);
        bounds = new Bounds(new Vector3(pos.x, pos.y, 0f), orthoSize * 2);
        zoomRange = new Vector2(main.orthographicSize / minRangeScale, main.orthographicSize);
    }

    void OnCameraMove(Vector2 screenDelta)
    {
        Vector3 delta = (Vector3)screenDelta * (main.orthographicSize / zoomRange.y) * cameraMoveSpeed * -1;
        Vector3 newPos = main.transform.localPosition + delta;
        Vector2 orthoSize = new Vector2(main.aspect * main.orthographicSize, main.orthographicSize);

        Vector3 finalPos = main.transform.localPosition;
        if ((delta.x < 0 && newPos.x - orthoSize.x >= bounds.min.x) || (delta.x >= 0 && newPos.x + orthoSize.x <= bounds.max.x))
            finalPos.x = newPos.x;
        if ((delta.y < 0 && newPos.y - orthoSize.y >= bounds.min.y) || (delta.y >= 0 && newPos.y + orthoSize.y <= bounds.max.y))
            finalPos.y = newPos.y;
        main.transform.localPosition = finalPos;
    }

    void OnCameraZoom(float pinchRatio)
    {
        float newSize = main.orthographicSize + (pinchRatio - 1f) * zoomScale;
        main.orthographicSize = Mathf.Clamp(newSize, zoomRange.x, zoomRange.y);
    }
}
