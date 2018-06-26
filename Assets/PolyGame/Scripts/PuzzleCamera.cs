using UnityEngine;

public class PuzzleCamera : MonoBehaviour
{
    public static PuzzleCamera Instance;
    public static Camera Main { get { return null != Instance ? Instance.main : null; } }

    const float sizeExtendScale = 1.5f;
    const float minRangeScale = 3f; 
    const float cameraMoveSpeed = 2f;
    const float zoomScale = 125f;
    const float followObjMoveDistance = 5f;

    Camera main;
    Bounds bounds;
    Vector2 zoomRange;

    void Awake()
    {
        Instance = this;
        main = GetComponent<Camera>();
        PuzzleTouch.onFingerDrag += OnCameraMove;
        PuzzleTouch.onFingerPinched += OnCameraZoom;
    }

    void OnDestroy()
    {
        PuzzleTouch.onFingerDrag -= OnCameraMove;
        PuzzleTouch.onFingerPinched -= OnCameraZoom;
        PuzzleTouch.onObjMove -= OnObjMove;
        Instance = null;
    }

    public void Init(Vector2 size)
    {
        Vector3 pos = transform.position;
        pos.x = size.x / 2;
        pos.y = size.y / 2;
        transform.position = pos;

        Vector2 extendedSize = size * sizeExtendScale;
        float graphAspect = extendedSize.x / extendedSize.y;
        if (graphAspect < main.aspect)
            main.orthographicSize = extendedSize.y / 2f;
        else
            main.orthographicSize = extendedSize.x / (2f * main.aspect);

        zoomRange = new Vector2(main.orthographicSize / minRangeScale, main.orthographicSize);
        UpdateBounds();

        PuzzleTouch.onObjMove += OnObjMove;
    }

    void UpdateBounds()
    {
        Vector2 orthoSize = new Vector2(main.aspect * main.orthographicSize, main.orthographicSize);
        var pos = transform.position;
        bounds = new Bounds(new Vector3(pos.x, pos.y, 0f), orthoSize * 2);
    }

    void MoveCamera(Vector3 delta)
    {
        Vector3 newPos = main.transform.localPosition + delta;
        Vector2 orthoSize = new Vector2(main.aspect * main.orthographicSize, main.orthographicSize);
        Bounds b = Puzzle.Current.PlaygroundBounds;

        Vector3 finalPos = main.transform.localPosition;
        if ((delta.x < 0 && newPos.x - orthoSize.x >= b.min.x) || (delta.x >= 0 && newPos.x + orthoSize.x <= b.max.x))
            finalPos.x = newPos.x;
        if ((delta.y < 0 && newPos.y - orthoSize.y >= b.min.y) || (delta.y >= 0 && newPos.y + orthoSize.y <= b.max.y))
            finalPos.y = newPos.y;
        main.transform.localPosition = finalPos;
        UpdateBounds();
    }

    void OnObjMove(Transform objPicked, Vector2 screenCurrent)
    {
        var objRenderer = objPicked.GetComponent<MeshRenderer>();
        if (null != objRenderer)
        {
            var b = objRenderer.bounds;
            if (!bounds.Contains(b.min) || !bounds.Contains(b.max))
            {
                var pos = bounds.center;
                var exDelta = bounds.extents - b.extents;
                pos.x = Mathf.Clamp(pos.x, b.center.x - exDelta.x, b.center.x + exDelta.x);
                pos.y = Mathf.Clamp(pos.y, b.center.y - exDelta.y, b.center.y + exDelta.y);
                pos.z = transform.position.z;
                if (null != transform.parent)
                    pos = transform.parent.InverseTransformPoint(pos);

                var dir = pos - transform.localPosition;
                dir = dir.normalized * Mathf.Min(dir.magnitude, followObjMoveDistance);
                MoveCamera(dir);
            }
        }
    }

    void OnCameraMove(Vector2 screenDelta)
    {
        Vector3 delta = (Vector3)screenDelta * (main.orthographicSize / zoomRange.y) * cameraMoveSpeed * -1;
        MoveCamera(delta);
    }

    void OnCameraZoom(float pinchRatio)
    {
        float newSize = main.orthographicSize + (pinchRatio - 1f) * zoomScale;
        main.orthographicSize = Mathf.Clamp(newSize, zoomRange.x, zoomRange.y);
        UpdateBounds();
    }
}
