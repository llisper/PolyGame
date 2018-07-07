using UnityEngine;

public class DrawBounds : MonoBehaviour
{
    public Bounds bounds;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(bounds.min, bounds.min + new Vector3(bounds.size.x, 0f, 0f));
        Gizmos.DrawLine(bounds.min + new Vector3(bounds.size.x, 0f, 0f), bounds.max);
        Gizmos.DrawLine(bounds.max, bounds.max - new Vector3(bounds.size.x, 0f, 0f));
        Gizmos.DrawLine(bounds.max - new Vector3(bounds.size.x, 0f, 0f), bounds.min);
    }
}
