﻿using UnityEngine;

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
            var bounds = target.GetComponent<MeshRenderer>().bounds;
            float offset = DMCVars.screenYOffset + bounds.extents.y;
            var screenPos = PuzzleTouch.Instance.MainFinger.ScreenPosition + new Vector2(0f, offset);
            var worldPos = PuzzleCamera.Main.ScreenToWorldPoint(screenPos);
            targetPosition = new Vector3(worldPos.x, worldPos.y, targetPosition.z);
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
