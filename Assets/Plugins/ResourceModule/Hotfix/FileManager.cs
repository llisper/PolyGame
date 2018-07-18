using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ResourceModule.Hotfix
{
    internal class FileManager
    {
        static MD5 _md5Hash = MD5.Create();

        public FileManifest manifest;
        public HashSet<string> sandboxFiles;

        public static FileManifest ReadFileManifest(string path)
        {
            string fullpath;
            var loc = PathRouter.GetFullPath(path, false, out fullpath);
            if (loc == PathLocation.NotFound)
                throw new FileNotFoundException("ReadFileManifest", path);

            string json = FileLoader.LoadString(path);
            var manifest = JsonUtility.FromJson<FileManifest>(json);
            manifest.BuildQuery();
            return manifest;
        }

        public static string GetMd5Hash(byte[] bytes)
        {
            byte[] data = _md5Hash.ComputeHash(bytes);
            var builder = new StringBuilder();
            Array.ForEach(data, b => builder.Append(b.ToString("x2")));
            return builder.ToString();
        }

        public static FileManager Create()
        {
            var fileManager = new FileManager();
            fileManager.manifest = ReadFileManifest(PathRouter.FileManifest);
            fileManager.CollectSandboxFiles();
            return fileManager;
        }

        public List<string> CalculateUpdateFiles(FileManifest newManifest)
        {
            List<string> list = new List<string>();
            foreach (var entry in manifest.files)
            {
                FileEntry newEntry;
                if (newManifest.query.TryGetValue(entry.name, out newEntry))
                {
                    bool isLocal = (PathLocation.NotFound != PathRouter.Exists(entry.name));
                    if ((entry.md5 != newEntry.md5 && isLocal) ||
                        (!newEntry.remote && !isLocal))
                    {
                        list.Add(entry.name);
                    }
                }
            }
            return list;
        }

        public void AddFile(string name)
        {
            sandboxFiles.Add(name);
        }

        public void ClearUnusedFiles()
        {
            List<string> filesToRemove = new List<string>();
            foreach (string dir in Directory.EnumerateDirectories(PathRouter.SandboxPath))
            {
                foreach (string file in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    string path = PathRouter.NormalizePath(file);
                    string name = path.Remove(0, PathRouter.SandboxPath.Length);
                    if (!manifest.query.ContainsKey(name))
                        filesToRemove.Add(path);
                }
            }

            foreach (string file in filesToRemove)
            {
                File.Delete(file);
                string name = file.Remove(0, PathRouter.SandboxPath.Length);
                sandboxFiles.Remove(name);
                HotfixLog.Log("Remove unused file: " + name);
            }
        }

        void CollectSandboxFiles()
        {
            sandboxFiles = new HashSet<string>();
            foreach (string dir in Directory.EnumerateDirectories(PathRouter.SandboxPath))
            {
                foreach (string file in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    string path = PathRouter.NormalizePath(file);
                    sandboxFiles.Add(path.Remove(0, PathRouter.SandboxPath.Length));
                }
            }
        }
    }
}
