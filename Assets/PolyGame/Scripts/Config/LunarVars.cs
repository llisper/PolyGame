using LunarConsolePlugin;

[CVarContainer]
public static class TouchVars
{
    public static readonly CVar holdThreshold = new CVar("holdThreshold", 0.125f);
    public static readonly CVar holdMoveThreshold = new CVar("holdMoveThreshold", 50f);
    public static readonly CVar raycastRadius = new CVar("raycastRadius", 50f);
}

[CVarContainer]
public static class CameraVars
{
    public static readonly CVar dragSpeed = new CVar("dragSpeed", 1f);
    public static readonly CVar zoomScale = new CVar("zoomScale", 400f);
}

[CVarContainer]
public static class DMCVars
{
    public static readonly CVar maxScale = new CVar("maxScale", 1.5f);
    public static readonly CVar scalingSpeed = new CVar("scalingSpeed", 7f);
    public static readonly CVar screenYOffset = new CVar("screenYOffset", 100f);
    public static readonly CVar moveSpeed = new CVar("moveSpeed", 30f);
}
