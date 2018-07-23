using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using R = ResourceModule.Hotfix;

public class BuildAPIs
{
    [Flags]
    enum BuildFlags
    {
        None = 0,
        Resource = 1,
        Bundle = 2,
    }

    class Args
    {
        public string identifier;
        public R.Version version;
        public BuildFlags buildFlags;
        public bool debug;
    }

    const string buildAndroidMethod = "BuildAPIs.BuildAndroid";
    const string buildiOSMethod = "BuildAPIs.BuildiOS";

    public static void BuildAndroid()
    {
        SetupSDKs.Setup();
        Args args = ParseArgs();

        PlayerSettings.applicationIdentifier = args.identifier;
        PlayerSettings.bundleVersion = args.version.Name;
        PlayerSettings.Android.bundleVersionCode = args.version.Major;

        if ((args.buildFlags & BuildFlags.Resource) == BuildFlags.Resource)
        {
            PackUtility.Setup(args.version.Conf);
            PackUtility.BuildAndroidResources();
        }

        PackUtility.GenerateAndroidStreamingAssets();

        if ((args.buildFlags & BuildFlags.Bundle) == BuildFlags.Bundle)
        {
            PkgBuilder.BuildAndroid(args.debug);
        }

        Debug.Log(buildAndroidMethod + " Done!");
    } 

    public static void BuildiOS()
    {
        SetupSDKs.Setup();
        Args args = ParseArgs();

        PlayerSettings.applicationIdentifier = args.identifier;
        PlayerSettings.bundleVersion = args.version.Name;
        PlayerSettings.Android.bundleVersionCode = args.version.Major;

        if ((args.buildFlags & BuildFlags.Resource) == BuildFlags.Resource)
        {
            PackUtility.Setup(args.version.Conf);
            PackUtility.BuildiOSResources();
        }

        PackUtility.GenerateiOSStreamingAssets();

        if ((args.buildFlags & BuildFlags.Bundle) == BuildFlags.Bundle)
        {
            PkgBuilder.BuildiOS(args.debug);
        }

        Debug.Log(buildiOSMethod + " Done!");
    }

    static Args ParseArgs()
    {
        string[] args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, buildAndroidMethod);
        if (i < 0)
            throw new Exception("Parse args failed, unable to find " + buildAndroidMethod);

        ++i;
        if (args.Length - i < 7)
            throw new Exception("Invalid args format, correct args format is: appIdentifier version(major.minor) cdnUrl buildFlags isDebug branch rev");

        var log = new StringBuilder("Commandline Args:\n");
        for (int j = i; j < args.Length; ++j)
            log.Append(args[j]).Append('\n');
        Debug.Log(log);

        Args ret = new Args();
        ret.identifier = args[i++];

        var vconf = new R.Version.Config();
        vconf.name = args[i++];
        vconf.cdn = args[i++];

        ret.buildFlags = BuildFlags.None;
        foreach (string f in args[i++].Split('|'))
            ret.buildFlags |= (BuildFlags)Enum.Parse(typeof(BuildFlags), f);

        ret.debug = bool.Parse(args[i++]);
        vconf.branch = args[i++];
        vconf.rev = args[i++];

        ret.version = R.Version.Create(vconf);

        return ret;
    }
}
