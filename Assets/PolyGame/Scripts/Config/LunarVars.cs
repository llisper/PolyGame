using LunarConsolePlugin;

[CVarContainer]
public static class TouchVars
{
    public static readonly CVar holdThreshold = new CVar("holdThreshold", 0.125f);
    public static readonly CVar holdMoveThreshold = new CVar("holdMoveThreshold", 15f);
}

[CVarContainer]
public static class CameraVars
{
    public static readonly CVar dragSpeed = new CVar("dragSpeed", 1f);
    public static readonly CVar zoomScale = new CVar("zoomScale", 400f);
}
