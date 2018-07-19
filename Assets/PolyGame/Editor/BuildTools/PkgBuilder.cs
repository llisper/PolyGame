using UnityEngine;
using UnityEditor;
using ResourceModule;

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
        options.locationPathName = PathRouter.BuildOutput(BuildTarget.Android.ToString());
        options.options = BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.StrictMode;
        if (debug)
            options.options |= BuildOptions.Development | BuildOptions.AllowDebugging;
        options.scenes = new string[] { PathRouter.StartScene };
        options.target = BuildTarget.Android;

        using (new HideCompileFlags())
        {
            Debug.Log("HideCompileFlags");
            string error = BuildPipeline.BuildPlayer(options);
            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);
            else
                Debug.Log("BuildPipeline.BuildPlayer Successfully!");
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
        options.locationPathName = PathRouter.BuildOutput(BuildTarget.iOS.ToString());
        options.options = BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.StrictMode | BuildOptions.SymlinkLibraries;
        if (debug)
            options.options |= BuildOptions.Development | BuildOptions.AllowDebugging;
        options.scenes = new string[] { PathRouter.StartScene };
        options.target = BuildTarget.iOS;

        using (new HideCompileFlags())
        {
            string error = BuildPipeline.BuildPlayer(options);
            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);
            else
                Debug.Log("BuildPipeline.BuildPlayer Successfully!");
        }
    }
    #endregion ios
}
