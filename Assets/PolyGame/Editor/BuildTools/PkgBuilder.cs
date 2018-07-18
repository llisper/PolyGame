using UnityEngine;
using UnityEditor;

public class PkgBuilder
{
    #region android
    [MenuItem("[Build Tools]/Build Package/Android")]
    public static void BuildAndroid()
    {
        BuildAndroid(true);
    }

    public static void BuildAndroid(bool debug)
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.locationPathName = Application.dataPath + "/../Build/Android";
        options.options = BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.StrictMode;
        if (debug)
            options.options |= BuildOptions.Development | BuildOptions.AllowDebugging;
        options.scenes = new string[] { "Assets/GameInit.unity" };
        options.target = BuildTarget.Android;

        using (new HideCompileFlags())
        {
            Debug.Log("HideCompileFlags");
            using (new EmptyResources())
            {
                Debug.Log("EmptyResources");
                string error = BuildPipeline.BuildPlayer(options);
                if (!string.IsNullOrEmpty(error))
                    Debug.LogError(error);
                else
                    Debug.Log("BuildPipeline.BuildPlayer Successfully!");
            }
        }
    }
    #endregion android

    #region ios
    [MenuItem("[Build Tools]/Build Package/iOS")]
    public static void BuildiOS()
    {
        BuildiOS(true);
    }

    public static void BuildiOS(bool debug)
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.locationPathName = Application.dataPath + "/../Build/iOS";
        options.options = BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.StrictMode | BuildOptions.SymlinkLibraries;
        if (debug)
            options.options |= BuildOptions.Development | BuildOptions.AllowDebugging;
        options.scenes = new string[] { "Assets/GameInit.unity" };
        options.target = BuildTarget.iOS;

        using (new HideCompileFlags())
        {
            using (new EmptyResources())
            {
                string error = BuildPipeline.BuildPlayer(options);
                if (!string.IsNullOrEmpty(error))
                    Debug.LogError(error);
                else
                    Debug.Log("BuildPipeline.BuildPlayer Successfully!");
            }
        }
    }
    #endregion ios
}
