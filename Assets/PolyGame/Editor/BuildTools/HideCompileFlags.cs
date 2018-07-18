using UnityEngine;
using System;
using System.IO;

class HideCompileFlags : IDisposable
{
    static string file = Application.dataPath + "/mcs.rsp";
    static string renamedFile = Application.dataPath + "/mcs.rsp.hide";

    public HideCompileFlags()
    {
        if (File.Exists(file))
            File.Move(file, renamedFile);
    }

    public void Dispose()
    {
        if (File.Exists(renamedFile))
            File.Move(renamedFile, file);
    }
}
