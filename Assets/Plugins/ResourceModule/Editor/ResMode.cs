using UnityEngine;
using UnityEditor;
using ResourceModule;

[InitializeOnLoad]
class ResMode
{
    const string DevMode = "[Res Mode]/Dev";
    const string ABMode = "[Res Mode]/AssetBundles";

    static ResMode()
    {
        EditorApplication.delayCall += ApplyResMode;
    }

    [MenuItem(DevMode)]
    static void SetDevMode()
    {
        EditorPrefs.SetInt("ResMode", (int)ResourceSystem.Mode.Dev);
        ApplyResMode();
    }

    [MenuItem(ABMode)]
    static void SetABMode()
    {
        EditorPrefs.SetInt("ResMode", (int)ResourceSystem.Mode.AssetBundle);
        ApplyResMode();
    }

    static void ApplyResMode()
    {
        var mode = (ResourceSystem.Mode)EditorPrefs.GetInt("ResMode", (int)ResourceSystem.Mode.Dev);
        Menu.SetChecked(DevMode, mode == ResourceSystem.Mode.Dev);
        Menu.SetChecked(ABMode, mode == ResourceSystem.Mode.AssetBundle);
        Debug.LogFormat("<color=purple>Current Resource Mode: {0}</color>", mode);
    }
}