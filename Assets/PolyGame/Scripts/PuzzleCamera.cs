using UnityEngine;

public class PuzzleCamera : MonoBehaviour
{
    public static PuzzleCamera Instance;
    public static Camera Main { get { return null != Instance ? Instance.main : null; } }

    const float sizeExtendScale = 1.5f;
    const float minRangeScale = 3f; 
    const float cameraMoveSpeed = 2f;

    Camera main;
    Vector2 size;
    Vector2 range;

    void Awake()
    {
        Instance = this;
        main = Camera.main;
        PuzzleTouch.onFingerDrag += OnCameraMove;
    }

    void OnDestroy()
    {
        PuzzleTouch.onFingerDrag -= OnCameraMove;
        Instance = null;
    }

    public void Init(Vector2 size)
    {
        Vector3 pos = transform.localPosition;
        pos.x = size.x / 2;
        pos.y = size.y / 2;
        transform.localPosition = pos;

        size *= sizeExtendScale;
        this.size = size;
        float graphAspect = size.x / size.y;
        if (graphAspect < main.aspect)
            main.orthographicSize = size.y / 2f;
        else
            main.orthographicSize = size.x / (2f * main.aspect);

        range = new Vector2(main.orthographicSize / minRangeScale, main.orthographicSize);
    }

    void OnCameraMove(Vector2 screenDelta)
    {
        main.transform.localPosition += (Vector3)screenDelta * (main.orthographicSize / range.y) * cameraMoveSpeed;
        // restrcts

    }
}
