using System;
using System.Diagnostics;

namespace ResourceModule
{
    internal class LoadTimer : IDisposable
    {
        string url;
        string typeName;
        Stopwatch stopwatch;

        public LoadTimer(Type loaderType, string url)
        {
            this.url = url;
            typeName = loaderType.Name;
        }

        public static LoadTimer Start<T>(string url)
        {
            var timer = new LoadTimer(typeof(T), url);
            timer.Start();
            return timer;
        }

        public void Start()
        {
            if (null == stopwatch)
                stopwatch = Stopwatch.StartNew();
            stopwatch.Restart();
            ResLog.LogFormat("Load start ({0}) {1}", typeName, url);
        }

        public void Stop()
        {
            if (null != stopwatch)
            {
                stopwatch.Stop();
                ResLog.LogFormat(
                    "Load finish {0} ms ({1}) {2}",
                    stopwatch.ElapsedMilliseconds, typeName, url);
                stopwatch.Reset();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
