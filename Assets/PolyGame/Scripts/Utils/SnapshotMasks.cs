using UnityEngine;
using ResourceModule;

public class SnapshotMasks : MonoBehaviour
{
    public Material[] materials;

    static SnapshotMasks instance;
    public static SnapshotMasks Instance
    {
        get
        {
            if (null == instance)
            {
                var prefab = PrefabLoader.Load(Prefabs.SnapshotMasks);
                var go = prefab.Instantiate<GameObject>();
                instance = go.GetComponent<SnapshotMasks>();
                DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.HideAndDontSave;
            }
            return instance;
        }
    }
}
