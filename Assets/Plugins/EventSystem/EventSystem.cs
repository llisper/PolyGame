using System;
using System.Reflection;
using System.Collections.Generic;

public sealed class EventLog : LogDefine<EventLog> { }

public partial class EvSystem<T> where T : EvSystem<T>, new()
{
    public const int LogicEventStartAt = 1000;
    public delegate void EventHandler(int e, object[] p);

    public static T Instance;

    public static void Init()
    {
        Instance = new T();
    }

    public EvSystem()
    {
        CollectEventNames();
    }

    public void Subscribe(int e, EventHandler h)
    {
        List<EventHandler> list;
        if (!mHandlers.TryGetValue(e, out list))
            mHandlers.Add(e, list = new List<EventHandler>());
        list.Add(h);
    }

    public void Unsubscribe(int e, EventHandler h)
    {
        List<EventHandler> list;
        if (mHandlers.TryGetValue(e, out list))
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] == h)
                {
                    list.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void Clear(int e)
    {
        mHandlers.Remove(e);
    }

    public string EventName(int ev)
    {
        string name;
        if (!mEventNames.TryGetValue(ev, out name))
            name = ev.ToString();
        return name;
    }

    void CollectEventNames()
    {
        mEventNames = new Dictionary<int, string>();
        BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
        foreach (var field in GetType().GetFields(flags))
        {
            if (field.IsLiteral)
            {
                int key = (int)field.GetValue(null);
                if (mEventNames.ContainsKey(key))
                {
                    throw new Exception(string.Format(
                        "{0} is duplicated, current mapping is {1}:{0}",
                        key, mEventNames[key]));
                }
                else
                {
                    mEventNames.Add(key, field.Name);
                }
            }
        }
    }

    void Fire(int e, object[] p)
    {
        List<EventHandler> list;
        if (mHandlers.TryGetValue(e, out list))
        {
            List<EventHandler> copy = CopyList(list);
            for (int i = 0; i < copy.Count; ++i)
            {
                EventHandler handler = copy[i];
                if (list.Contains(handler))
                {
                    try
                    {
                        handler(e, p);
                    }
                    catch (Exception exception)
                    {
                        list.Remove(handler);
                        EventLog.LogError(ErrorMsg(e, handler, exception));
                    }
                }
            }
            ReleaseCopiedList(ref copy);
        }
    }

    string ErrorMsg(int ev, EventHandler h, Exception e)
    {
        string error;
        if (h.Target is UnityEngine.Object)
        {
            error = string.Format(
                "<b>{4}:</b> event handler ([{0}]{1}.[{2}]) throws exception:\n{3}",
                h.Target.GetType().Name,
                ((UnityEngine.Object)h.Target).name,
                h.Method,
                e,
                EventName(ev));
        }
        else
        {
            error = string.Format(
                "<b>{3}:</b> event handler ([{0}].[{1}]) throws exception:\n{2}",
                h.Method.ReflectedType,
                h.Method,
                e,
                EventName(ev));
        }
        return error;
    }

    Dictionary<int, string> mEventNames;
    Dictionary<int, List<EventHandler>> mHandlers = new Dictionary<int, List<EventHandler>>();
}
