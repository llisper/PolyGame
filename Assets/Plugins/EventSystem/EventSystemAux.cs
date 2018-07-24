using System;
using System.Collections.Generic;

public partial class EvSystem<T> where T : EvSystem<T>, new()
{
    static EvSystem()
    {
        InitParamPool();
        InitCopyPool();
    }

    #region param pool
    static List<object[]>[] _paramPool;
    static object[] _empty = new object[0];

    static void InitParamPool()
    {
        _paramPool = new List<object[]>[paramLength];
        for (int i = 1, imax = _paramPool.Length; i <= imax; ++i)
        {
            List<object[]> pool = new List<object[]>() { new object[i] };
            _paramPool[i - 1] = pool;
        }
    }

    static object[] ParamArray(int len)
    {
        if (0 == len)
            return _empty;

        var pool = _paramPool[len - 1];
        object[] array;
        if (pool.Count > 0)
        {
            array = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
        }
        else
        {
            array = new object[len];
        }

        Array.Clear(array, 0, array.Length);
        return array;
    }

    static void ReleaseParamArray(object[] array)
    {
        int len = array.Length;
        if (len > 0)
        {
            Array.Clear(array, 0, len);
            _paramPool[len - 1].Add(array);
        }
    }
    #endregion param pool

    #region copy pool
    static List<List<EventHandler>> _copyPool;

    static void InitCopyPool()
    {
        _copyPool = new List<List<EventHandler>>();
        for (int i = 0; i < 3; ++i)
            _copyPool.Add(new List<EventHandler>(16));
    }

    static List<EventHandler> CopyList(List<EventHandler> list)
    {
        List<EventHandler> copy;
        if (_copyPool.Count > 0)
        {
            copy = _copyPool[_copyPool.Count - 1];
            _copyPool.RemoveAt(_copyPool.Count - 1);
        }
        else
        {
            copy = new List<EventHandler>(16);
        }
        copy.Clear();
        copy.AddRange(list);
        return copy;
    }

    static void ReleaseCopiedList(ref List<EventHandler> list)
    {
        list.Clear();
        _copyPool.Add(list);
        list = null;
    }
    #endregion copy pool
}
