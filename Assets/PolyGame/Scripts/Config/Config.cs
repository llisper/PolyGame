using UnityEngine;
using System;

[Serializable]
public class Config : IConfig<Config>
{
    [Serializable]
    public class ZOrder
    {
        public float debrisStart = -0.2f;
        public float debrisFinished = -0.1f;
        public float wireframe = -0.01f;
        public float background = -0.001f;
    }

    [Serializable]
    public class Puzzle
    {
        public float scrambleRadius = 150f;
        public float moveSpeed = 30f;
        public float fadeSpeed = 15f;
        public float fitThreshold = 50f;
        public float finishDebrisMoveSpeed = 10f;
        public float saveInterval = 2f;
    }

    [Serializable]
    public class Camera
    {
        public float distance = 1001f;
        public float sizeExtendScale = 1.5f;
        public float minRangeScale = 3f;
        public float followObjMoveDistance = 5f;
    }

    [Serializable]
    public class Wireframe
    {
        public float width = 0.85f;
        public string color = "c8c8c8";
    }

    public ZOrder zorder = new ZOrder();
    public Puzzle puzzle = new Puzzle();
    public Camera camera = new Camera();
    public Wireframe wireframe = new Wireframe();
    public Vector2Int snapshotSize = new Vector2Int(256, 256);

}