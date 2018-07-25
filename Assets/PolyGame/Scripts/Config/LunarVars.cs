using LunarConsolePlugin;

[CVarContainer]
public static class TouchVars
{
    // 从触屏到开始选择碎片的时间
    public static readonly CVar holdThreshold = new CVar("holdThreshold", 0.125f);
    // 触屏中手指移动的容忍值，如果移动距离超过这个值，操作就判定为拖动屏幕
    public static readonly CVar holdMoveThreshold = new CVar("holdMoveThreshold", 50f);
    // 选择碎片的半径，中心是当前触点
    public static readonly CVar raycastRadius = new CVar("raycastRadius", 25f);
}

[CVarContainer]
public static class CameraVars
{
    // 屏幕拖动的速度
    public static readonly CVar dragSpeed = new CVar("dragSpeed", 1f);
    // 屏幕缩放的速度
    public static readonly CVar zoomScale = new CVar("zoomScale", 400f);
}

[CVarContainer]
public static class DMCVars
{
    // 被选中的碎片会做一个放大处理，这是放大的最大比例
    public static readonly CVar maxScale = new CVar("maxScale", 1.5f);
    // 被选中碎片放大动画的速度
    public static readonly CVar scalingSpeed = new CVar("scalingSpeed", 7f);
    // 被选中碎片需要移动到手指上方，这个是偏移值
    public static readonly CVar screenYOffset = new CVar("screenYOffset", 100f);
    // 被选中碎片的移动速度
    public static readonly CVar moveSpeed = new CVar("moveSpeed", 30f);
}

[CVarContainer]
public static class PuzzleVars
{
    // 释放碎片的时候，吸附的距离阈值
    public static readonly CVar fitThreshold  = new CVar("fitThreshold", 50f);
}