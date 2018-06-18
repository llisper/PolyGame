using UnityEngine;

public class DebrisMoveContainer : MonoBehaviour
{
    public float maxScale = 1.5f;
    public float scalingSpeed = 7f;
    public float screenYOffset = 20f;
    public float moveSpeed = 7f;

    Transform targetParent;
    Transform target;
    Vector3 targetPosition;

    public Transform Target
    {
        get { return target; }
        set
        {
            if (target != value)
            {
                if (null != target)
                {
                    target.localScale = Vector3.one;
                    target.SetParent(targetParent);
                    target = null;
                    targetParent = null;
                }

                if (null != value)
                {
                    target = value;
                    targetParent = target.parent;
                    target.SetParent(transform);
                    target.localScale = new Vector3(maxScale, maxScale, maxScale);
                    CalculateTargetPosition();
                }
            }
        }
    }

    void CalculateTargetPosition()
    {
        targetPosition = target.transform.position;
        if (null != PuzzleTouch.Instance && 
            null != PuzzleTouch.Instance.MainFinger &&
            null != PuzzleCamera.Main)
        {
            Vector2 screenPos = PuzzleTouch.Instance.MainFinger.ScreenPosition + new Vector2(0f, screenYOffset);
            Vector3 worldPos = PuzzleCamera.Main.ScreenToWorldPoint(screenPos);
            if (worldPos.y > targetPosition.y)
                targetPosition.y = worldPos.y;
        }
        targetPosition = transform.InverseTransformPoint(targetPosition);
    }

    void Update()
    {
        if (null != target)
        {
            target.localPosition = Vector3.Lerp(target.localPosition, targetPosition, Time.deltaTime * moveSpeed);
            target.localScale = Vector3.Lerp(target.localScale, Vector3.one, Time.deltaTime * scalingSpeed);
        }
    }
}
