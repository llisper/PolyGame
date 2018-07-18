using UnityEditor;

namespace ResourceModule.Debug
{
    [CustomEditor(typeof(LoaderVisualizer))]
    class LoaderVisualizerInspector : Editor
    {
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            var loader = ((LoaderVisualizer)target).loader;
            EditorGUILayout.TextField("Url", loader.Url, EditorStyles.label);
            EditorGUILayout.LabelField("RefCount", loader.RefCount.ToString());
            EditorGUILayout.LabelField("IsComplete", loader.IsComplete.ToString());
            EditorGUILayout.LabelField("Progress", string.Format("{0:P1}", loader.Progress));
        }
    }
}
