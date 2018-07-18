using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ResourceModule
{
    public class FileLoader : ResourceLoader
    {
        public static byte[] Load(string path)
        {
            using (var timer = LoadTimer.Start<FileLoader>(path))
            {
                if (IsDev)
                {
                    #if UNITY_EDITOR
                    path = string.Format("{0}/{1}", PathRouter.Res, path);
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    return null != asset ? asset.bytes : null;
                    #else
                    throw new ApplicationException("IsDev is not allowed unless in Editor");
                    #endif
                }
                else
                {
                    return BytesLoader.Load(path);
                }
            }
        }

        public static string LoadString(string path)
        {
            byte[] bytes = Load(path);
            return Encoding.UTF8.GetString(RemoveBOM(bytes));
        }

        public static byte[] RemoveBOM(byte[] bytes)
        {
            if (bytes.Length >= 3)
            {
                if (bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf)
                {
                    byte[] newBytes = new byte[bytes.Length - 3];
                    Array.Copy(bytes, 3, newBytes, 0, bytes.Length - 3);
                    bytes = newBytes;
                }
            }
            return bytes;
        }

        public static bool Exists(string path)
        {
            if (IsDev)
            {
                #if UNITY_EDITOR
                path = string.Format("{0}/{1}", PathRouter.Res, path);
                return null != UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                #else
                throw new ApplicationException("IsDev is not allowed unless in Editor");
                #endif
            }
            else
            {
                return PathLocation.NotFound != PathRouter.Exists(path);
            }
        }

        protected override Task AsyncRun()
        {
            throw new NotImplementedException();
        }
    }
}
