using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEditor;
using System;

namespace Ugly.BuildPreprocess
{
    public class BuildPreprocessor : IPreprocessBuildWithReport
    {
        int IOrderedCallback.callbackOrder => 0;

        private const string configsFolderPath = "BuildPreprocessorConfigs";

        public static string ConfigsFolderPath => string.Concat(Application.dataPath, "/" + configsFolderPath + "/");

        private AbstractPreprocessor[] preprocessors;

        public class ErrorWindow : EditorWindow
        {
            private Vector2 scroll;
            private GUIStyle style;

            private List<string> errors, other;

            public void Show(List<Message> messages)
            {
                style = new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 14 };

                errors = new List<string>();
                other = new List<string>();

                for (int i = 0; i < messages.Count; i++)
                {
                    if(messages[i].isError)
                    {
                        errors.Add(messages[i].text);
                    }
                    else
                    {
                        other.Add(messages[i].text);
                    }
                }

                Show();
            }

            private void OnGUI()
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.LabelField("Build preprocess error!", style);
                for (int i = 0; i < errors.Count; i++)
                {
                    EditorGUILayout.HelpBox(errors[i], MessageType.Error, true);
                }
                for (int i = 0; i < other.Count; i++)
                {
                    EditorGUILayout.HelpBox(other[i], MessageType.Info, true);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        public struct Message
        {
            public bool isError;
            public string text;
        }

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            if (GetPreprocessors(out preprocessors))
            {
                bool error = false;
                string logMessage = null;

                List<Message> messages = new List<Message>();
                for (int i = 0; i < preprocessors.Length; i++)
                {
                    bool isArror = !preprocessors[i].Check(out string message);
                    error |= isArror;

                    messages.Add(new Message() { isError = isArror, text = message });
                    logMessage += logMessage == null ? message : logMessage + "\n---------------------\n" + message;
                }

                if (error)
                {
                    EditorWindow.GetWindow<ErrorWindow>("Error", true).Show(messages);
                    if(report != null)
                    {
                        throw new BuildFailedException("\n---------------------\n" + logMessage + "\n---------------------");
                    }
                }
                else
                {
                    Debug.Log(logMessage);
                }
            }
            else
            {
                Debug.LogWarning("No build preprocessors!");
            }
        }

        [MenuItem("Preprocessor/Preprocess now")]
        public static void Test()
        {
            (new BuildPreprocessor() as IPreprocessBuildWithReport).OnPreprocessBuild(null);
        }

        public static bool GetPreprocessors(out AbstractPreprocessor[] preprocessors)
        {
            List<AbstractPreprocessor> list = new List<AbstractPreprocessor>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++)
            {
                var types = assemblies[a].GetTypes();
                for (int t = 0; t < types.Length; t++)
                {
                    if (types[t].IsSubclassOf(typeof(AbstractPreprocessor)) && !types[t].IsAbstract)
                    {
                        list.Add((AbstractPreprocessor)Activator.CreateInstance(types[t]));
                    }
                }
            }

            if(list.Count > 0)
            {
                preprocessors = list.ToArray();
                SetupPreprocessors(preprocessors);
                return true;
            }
            else
            {
                preprocessors = null;
                return false;
            }
        }

        public static void SetupPreprocessors(AbstractPreprocessor[] preprocessors)
        {
            string path = ConfigsFolderPath;
            string configPath;

            if (Directory.Exists(path))
            {
                for (int i = 0; i < preprocessors.Length; i++)
                {
                    configPath = path + preprocessors[i].GetName() + ".txt";
                    if (File.Exists(configPath))
                    {
                        var txt = File.ReadAllText(configPath);
                        preprocessors[i].ReadJsonConfing(txt);
                    }
                    else
                    {
                        File.WriteAllText(configPath, preprocessors[i].GetDefaultJsonConfig());
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(path);
                for (int i = 0; i < preprocessors.Length; i++)
                {
                    configPath = path + preprocessors[i].GetName() + ".txt";
                    File.WriteAllText(configPath, preprocessors[i].GetDefaultJsonConfig());
                    preprocessors[i].UseDafaultConfig();
                }
            }

            AssetDatabase.Refresh();
        }

        public static void SavePreprocessors(AbstractPreprocessor[] preprocessors)
        {
            string path = ConfigsFolderPath;
            string configPath;

            for (int i = 0; i < preprocessors.Length; i++)
            {
                configPath = path + preprocessors[i].GetName() + ".txt";
                File.WriteAllText(configPath, preprocessors[i].SaveConfigToJson());
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            AssetDatabase.Refresh();
        }
    }
}
