using UnityEditor;
using System;
using System.IO;

/// <summary>
/// ANDROID_SDK_HOME: path to android sdk, eg:E:/Android/SDK
/// ANDROID_NDK_HOME: path to android ndk, eg:E:/Android/NDK/android-ndk-r13b
/// JAVA_SDK_HOME: path to java sdk, eg:E:/Java/jdk1.8.0_162
/// </summary>
class SetupSDKs
{
    const string AndroidSdkHome = "ANDROID_SDK_HOME";
    const string AndroidNdkHome = "ANDROID_NDK_HOME";
    const string JavaSdkHome = "JAVA_SDK_HOME";

    public static void Setup()
    {
        string adk = Environment.GetEnvironmentVariable(AndroidSdkHome);
        CheckPath(AndroidSdkHome, adk);
        string ndk = Environment.GetEnvironmentVariable(AndroidNdkHome);
        CheckPath(AndroidNdkHome, ndk);
        string jdk = Environment.GetEnvironmentVariable(JavaSdkHome);
        CheckPath(JavaSdkHome, jdk);

        EditorPrefs.SetString("AndroidSdkRoot", adk);
        EditorPrefs.SetString("AndroidNdkRoot", ndk);
        EditorPrefs.SetString("JdkPath", jdk);
    }

    static void CheckPath(string varName, string path)
    {
        if (!Directory.Exists(path))
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception(varName + " is null");
            else
                throw new Exception(string.Format("{0}:{1} is not a valid path", varName, path));
        }
    }
}
