using UnityEngine;

public class PuzzleCamera : MonoBehaviour
{
    public static PuzzleCamera Instance;
    public static Camera Main { get { return null != Instance ? Instance.main : null; } }

    float sizeExtendScale { get { return Config.Instance.camera.sizeExtendScale; } }
    float minRangeScale { get { return Config.Instance.camera.minRangeScale; } }
    float cameraMoveSpeed { get { return Config.Instance.camera.moveSpeed; } }
    float zoomScale { get { return Config.Instance.camera.zoomScale; } }
    float followObjMoveDistance { get { return Config.Instance.camera.followObjMoveDistance; } }

    public Camera main;
    Bounds bounds;
    Vector2 zoomRange;

    void Awake()
    {
        Instance = this;
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
        SetupCamera(main, size, sizeExtendScale);
        UpdateBounds();

        zoomRange = new Vector2(main.orthographicSize / minRangeScale, main.orthographicSize);
        PuzzleTouch.onObjMove += OnObjMove;
    }

    public static void SetupCamera(Camera camera, Vector2 graphSize, float extendScale = 1f)
    {
        Vector3 pos = camera.transform.position;
        pos.x = graphSize.x / 2;
        pos.y = graphSize.y / 2;
        camera.transform.position = pos;

        Vector2 extendedSize = graphSize * extendScale;
        float graphAspect = extendedSize.x / extendedSize.y;
        if (graphAspect < camera.aspect)
            camera.orthographicSize = extendedSize.y / 2f;
        else
            camera.orthographicSize = extendedSize.x / (2f * camera.aspect);
    }

    void UpdateBounds()
    {
        bounds = Utils.CalculateBounds(main.transform.position, main.aspect, main.orthographicSize);
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
                pos.z = main.transform.position.z;
                if (null != main.transform.parent)
                    pos = main.transform.parent.InverseTransformPoint(pos);

                var dir = pos - main.transform.localPosition;
                dir = dir.normalized * Mathf.Min(dir.magnitude, followObjMoveDistance);
                MoveCamera(dir);
            }
        }
    }

    void OnCameraMove(Vector2 screenDelta)
    {
        Vector3 delta = (Vector3)screenDelta * (main.orthographicSize / zoomRange.y) * CameraVars.dragSpeed.FloatValue * -1;
        MoveCamera(delta);
    }

    void OnCameraZoom(float pinchRatio)
    {
        float newSize = main.orthographicSize + (pinchRatio - 1f) * CameraVars.zoomScale.FloatValue;
        main.orthographicSize = Mathf.Clamp(newSize, zoomRange.x, zoomRange.y);
        UpdateBounds();
    }
}
