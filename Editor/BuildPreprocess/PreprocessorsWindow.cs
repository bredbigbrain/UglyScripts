using UnityEditor;
using UnityEngine;

namespace Ugly.BuildPreprocess
{
    class PreprocessorsWindow: EditorWindow
    {
        [MenuItem("Tools/Preprocessor/Open config window")]
        public static void Init()
        {
            var window = GetWindow<PreprocessorsWindow>();
            window.InitGUI();
            window.Show();
        }

        private AbstractPreprocessor[] preprocessors;
        private bool noPreprocessors = false;

        private Vector2 scroll;

        public void InitGUI()
        {
            noPreprocessors = !BuildPreprocessor.GetPreprocessors(out preprocessors);
            if(!noPreprocessors)
            {
                for (int i = 0; i < preprocessors.Length; i++)
                {
                    preprocessors[i].InitGUI();
                }
            }
        }

        private void OnGUI()
        {
            if(noPreprocessors)
            {
                EditorGUILayout.HelpBox("No preprocessors!", MessageType.Warning);
            }
            else
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                for (int i = 0; i < preprocessors.Length; i++)
                {
                    preprocessors[i].Active = EditorGUILayout.BeginToggleGroup(preprocessors[i].GetName(), preprocessors[i].Active);
                    preprocessors[i].OnGUI();
                    EditorGUILayout.EndToggleGroup();
                }
                GUILayout.Space(15);
                if(GUILayout.Button("Save"))
                {
                    BuildPreprocessor.SavePreprocessors(preprocessors);
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
