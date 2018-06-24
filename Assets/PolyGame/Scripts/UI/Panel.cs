using UnityEngine;

public abstract class Panel : MonoBehaviour
{
    public virtual bool Persistent { get { return false; } }

    public void Close()
    {
        UI.Instance.ClosePanel(GetType());
    }
}
