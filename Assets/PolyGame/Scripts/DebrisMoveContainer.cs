using UnityEngine;

public class DebrisMoveContainer : MonoBehaviour
{
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
                    float s = DMCVars.maxScale;
                    target.localScale = new Vector3(s, s, s);
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
            float offset = Utils.Dpi * DMCVars.offsetInches;
            Vector2 screenPos = PuzzleTouch.Instance.MainFinger.ScreenPosition + new Vector2(0f, offset);
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
            target.localPosition = Vector3.Lerp(target.localPosition, targetPosition, Time.deltaTime * DMCVars.moveSpeed);
            target.localScale = Vector3.Lerp(target.localScale, Vector3.one, Time.deltaTime * DMCVars.scalingSpeed);
        }
    }
}
