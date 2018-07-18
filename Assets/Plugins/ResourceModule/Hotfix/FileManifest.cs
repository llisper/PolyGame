using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ResourceModule.Hotfix
{
    [Serializable]
    public class FileEntry
    {
        public string name;
        public string md5;
        public int size;
        public bool remote = false;
    }

    [Serializable]
    public class FileManifest
    {
        public List<FileEntry> files = new List<FileEntry>();
        [NonSerialized]
        public Dictionary<string, FileEntry> query;

        public void BuildQuery()
        {
            query = new Dictionary<string, FileEntry>();
            for (int i = 0; i < files.Count; ++i)
                query[files[i].name] = files[i];
        }

        public void WriteToFile()
        {
            string path = PathRouter.SandboxPath + PathRouter.FileManifest;
            string text = JsonUtility.ToJson(this, true);
            File.WriteAllText(path, text, Encoding.UTF8);
        }
    }
}
