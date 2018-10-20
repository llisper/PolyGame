using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(Puzzle))]
class PuzzleEditor : Editor
{
    Puzzle script;

    public override void OnInspectorGUI()
    {
        script = (Puzzle)target;
        OneToFinish();
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
}