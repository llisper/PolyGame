using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(Puzzle))]
class PuzzleEditor : Editor
{
    Puzzle script;
    List<Type> stateTypes;
    string[] stateTypeStrings;

    public override void OnInspectorGUI()
    {
        script = (Puzzle)target;
        OneToFinish();
        GUILayout.Space(5f);
        States();
    }

    void OneToFinish()
    {
        if (GUILayout.Button("OneToFinish"))
        {

            if (null != script.finished)
            {
                int i = Array.IndexOf(script.finished, false);
                if (i >= 0)
                {
                    for (int j = i + 1; j < script.finished.Length; ++j)
                    {
                        if (!script.finished[j])
                        {
                            script.finished[j] = true;
                            script.history.Add(j);
                        }
                    }
                }
            }
        }
    }

    void States()
    {
        var stateMachine = (PuzzleStateMachine)script.GetType().InvokeMember(
            "stateMachine",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField,
            null, script, null);

        if (null != stateMachine && null != stateMachine.Current)
        {
            if (null == stateTypes)
            {
                var states = (Dictionary<Type, PuzzleState>)stateMachine.GetType().InvokeMember(
                    "states",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField,
                    null, stateMachine, null);

                stateTypes = new List<Type>(states.Keys);
                stateTypeStrings = stateTypes.ConvertAll(v => v.Name).ToArray();
            }

            int current = stateTypes.IndexOf(stateMachine.Current.GetType());
            int next = EditorGUILayout.Popup("State", current, stateTypeStrings);
            if (current != next)
                stateMachine.Next(stateTypes[next]);
        }
    }
}