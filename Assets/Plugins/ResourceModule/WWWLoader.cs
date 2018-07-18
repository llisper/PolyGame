using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResourceModule
{
    public class WWWLoader : ResourceLoader
    {
        static bool _loadOneByOne = false;
        static int _max_parallel = 30;

        static int _runningCount = 0;
        static bool _monitorRunning = false;
        static Queue<WWWLoader> _loaderQueue = new Queue<WWWLoader>();

        Exception exception;

        public static async Task<byte[]> AsyncLoad(string url, OnProgress onProgress = null)
        {
            var loader = AutoNew<WWWLoader>(url, onProgress);
            await loader.task;
            byte[] bytes = (byte[])loader.ResultObject;
            loader.Release();
            return bytes;
        }

        protected override async Task AsyncRun()
        {
            _loaderQueue.Enqueue(this);
            if (!_monitorRunning)
                LoadingMonitor();

            while (!IsComplete)
                await Awaiters.NextFrame;

            if (null != exception)
                throw exception;
        }

        /// <summary>
        /// use www to load stuff
        /// never let exception out of this function
        /// </summary>
        protected async void AsyncStartLoading()
        {
            ++_runningCount;
            ResLog.VerboseFormat(
                "({0}) Dispatch {1} for loading, current running {2}",
                GetType().Name, Url, _runningCount);
            try
            {
                using (WWW www = new WWW(Url))
                {
                    www.threadPriority = Application.backgroundLoadingPriority;
                    while (!www.isDone)
                    {
                        Progress = www.progress;
                        await Awaiters.NextFrame;
                    }
                    Progress = www.progress;

                    if (!string.IsNullOrEmpty(www.error))
                    {
                        throw new ApplicationException(string.Format(
                            "WWW error, url={0}, error={1}",
                            Url, www.error));
                    }
                    else
                    {
                        Finish(www.bytes);
                    }
                }
            }
            catch (Exception e)
            {
                Finish(null, true);
                exception = e;
            }
            finally
            {
                --_runningCount;
            }
        }

        static async void LoadingMonitor()
        {
            _monitorRunning = true;
            while (_runningCount + _loaderQueue.Count > 0)
            {
                int parallel = _loadOneByOne ? 1 : _max_parallel;
                while (_runningCount < parallel && _loaderQueue.Count > 0)
                {
                    var loader = _loaderQueue.Dequeue();
                    loader.AsyncStartLoading();
                }
                await Awaiters.NextFrame;
            }
            _monitorRunning = false;
        }
    }
}
