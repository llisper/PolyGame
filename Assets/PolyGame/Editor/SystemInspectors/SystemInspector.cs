using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

class SystemInspector : EditorWindow
{
    [MenuItem("[PolyGame]/System Inspector")]
    public static void Open()
    {
        var win = GetWindow<SystemInspector>();
        win.Show();
    }

    class Inspector
    {
        public bool foldout;
        public Type inspectorType;
    }

    IList list;
    Dictionary<Type, Inspector> inspectors = new Dictionary<Type, Inspector>();

    void OnGUI()
    {
        if (null == list)
        {
            if (null == Game.Instance)
                return;

            list = (IList)typeof(Game).InvokeMember(
                "systems",
                BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic,
                null, Game.Instance, null);
        }

        foreach (var s in list)
        {
            object system = s.GetType().InvokeMember(
                "instance",
                BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Public,
                null, s, null);

            Type type = system.GetType();
            Inspector inspector = GetInspector(type);
            inspector.foldout = EditorGUILayout.Foldout(inspector.foldout, type.Name);
            if (inspector.foldout && null != inspector.inspectorType)
            {
                ++EditorGUI.indentLevel;
                inspector.inspectorType.InvokeMember(
                    "OnInspectorGUI",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
                    null, null, null);
                --EditorGUI.indentLevel;
            }
        }
    }

    Inspector GetInspector(Type type)
    {
        Inspector inspector;
        if (!inspectors.TryGetValue(type, out inspector))
        {
            inspector = new Inspector();
            inspector.inspectorType = Assembly.GetExecutingAssembly().GetType(type.Name + "Inspector");
            inspectors.Add(type, inspector);
        }
        return inspector;
    }
}