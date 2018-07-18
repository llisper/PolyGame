using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ResourceModule
{
    public class BytesLoader : ResourceLoader
    {
        public static byte[] Load(string path)
        {
            using (var timer = LoadTimer.Start<BytesLoader>(path))
            {
                string fullpath;
                var loc = PathRouter.GetFullPath(path, false, out fullpath);
                if (loc == PathLocation.NotFound)
                    throw new FileNotFoundException("BytesLoader", path);

                ResLog.VerboseFormat("BytesLoader.Load, loc:{0}, fullpath:{1}", loc, fullpath);

                if (loc == PathLocation.InApp && Application.platform == RuntimePlatform.Android)
                    return AndroidPlugin.GetAssetBytes(path);
                else
                    return File.ReadAllBytes(fullpath);
            }
        }

        public static async Task<byte[]> AsyncLoad(string path, OnProgress onProgress = null)
        {
            var loader = AutoNew<BytesLoader>(path, onProgress);
            await loader.task;
            byte[] bytes = (byte[])loader.ResultObject;
            loader.Release();
            return bytes;
        }

        protected override async Task AsyncRun()
        {
            string fullurl;
            var loc = PathRouter.GetFullPath(Url, true, out fullurl);
            if (loc == PathLocation.NotFound)
                throw new FileNotFoundException("BytesLoader", Url);

            ResLog.VerboseFormat("BytesLoader.Load, loc:{0}, fullpath:{1}", loc, fullurl);

            byte[] bytes = null;
            try
            {
                bytes = await WWWLoader.AsyncLoad(fullurl, v => Progress = v);
                Finish(bytes);
            }
            catch (Exception e)
            {
                Finish(null, true);
                throw e;
            }
        }
    }
}
