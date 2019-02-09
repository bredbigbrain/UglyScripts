using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ugly.BuildPreprocess
{
    public abstract class AbstractPreprocessor
    {
        public class DefaultConfig
        {
            public bool active = true;
            public string[] assets;
        }

        Type configType;
        protected DefaultConfig defaultConfig;
        public readonly bool isUsingAssets;
        protected UnityEngine.Object[] assets;

        public bool Active
        {
            get => defaultConfig.active;
            set => defaultConfig.active = value;
        }

        public AbstractPreprocessor(Type configType, bool useAssets)
        {
            isUsingAssets = useAssets;
            this.configType = configType;
        }
        
        protected abstract bool Process(out string message);

        protected virtual void Initialize() { }
        public virtual void InitGUI() { }
        public virtual void OnGUI() { }

        public virtual string GetName()
        {
            return GetType().Name;
        }

        public string GetDefaultJsonConfig()
        {
            return JsonUtility.ToJson(Activator.CreateInstance(configType));
        }

        public string SaveConfigToJson()
        {
            if(isUsingAssets)
            {
                if(assets == null)
                {
                    defaultConfig.assets = null;
                }
                else
                {
                    List<string> paths = new List<string>();
                    for (int i = 0; i < assets.Length; i++)
                    {
                        if(assets[i] != null)
                        {
                            paths.Add(AssetDatabase.GetAssetPath(assets[i]));
                        }
                    }
                    defaultConfig.assets = paths.ToArray();
                }
            }
            return GetJsonConfig();
        }

        protected virtual string GetJsonConfig()
        {
            return JsonUtility.ToJson(defaultConfig);
        }

        public void ReadJsonConfing(string jsonStr)
        {
            defaultConfig = JsonUtility.FromJson(jsonStr, configType) as DefaultConfig;
            if(isUsingAssets)
            {
                LoadAssets();
            }
            Initialize();
        }
        public void UseDafaultConfig()
        {
            defaultConfig = Activator.CreateInstance(configType) as DefaultConfig;
            if (isUsingAssets)
            {
                LoadAssets();
            }
            Initialize();
        }

        public bool Check(out string message)
        {
            if(defaultConfig.active)
            {
                bool isOK = Process(out message);
                message = GetName() + " " + (isOK? "OK\n" : "ERROR\n") + message;
                return isOK;
            }
            message = GetName() + " INACITVE";
            return true;
        }

        protected void LoadAssets()
        {
            if(defaultConfig.assets != null)
            {
                var assetsPaths = defaultConfig.assets;
                assets = new UnityEngine.Object[assetsPaths.Length];
                for (int i = 0; i < assetsPaths.Length; i++)
                {
                    assets[i] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetsPaths[i]);
                }
            }
            else
            {
                assets = new UnityEngine.Object[0];
            }
        }

        public T[] GetAssets<T>() where T : UnityEngine.Object
        {
            T[] array = new T[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                array[i] = assets[i] as T;
            }
            return array;
        }

        private int removeIndex = -1;
        protected void DrawAssetsList<T>()
        {
            if (assets != null)
            {
                if (removeIndex != -1)
                {
                    RemoveAtIndex(removeIndex);
                    removeIndex = -1;
                }
                for (int i = 0; i < assets.Length; i++)
                {
                    GUILayout.BeginHorizontal();
                    assets[i] = EditorGUILayout.ObjectField(assets[i], typeof(T), false);
                    if(GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        removeIndex = i;
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Drop new asset here");
                var newAsset = EditorGUILayout.ObjectField(null, typeof(T), false);
                if (newAsset)
                {
                    ExpandAssetsArray(newAsset);
                }
                GUILayout.EndHorizontal();
            }

            void RemoveAtIndex(int removeIndex)
            {
                assets[removeIndex] = null;
                UnityEngine.Object[] newArray = new UnityEngine.Object[assets.Length - 1];
                for (int i = 0; i < assets.Length; i++)
                {
                    if (i < removeIndex)
                    {
                        newArray[i] = assets[i];
                    }
                    else if (i > removeIndex)
                    {
                        newArray[i - 1] = assets[i];
                    }
                }
                assets = newArray;
            }

            void ExpandAssetsArray(UnityEngine.Object newAsset)
            {
                UnityEngine.Object[] newArray = new UnityEngine.Object[assets.Length + 1];
                for (int i = 0; i < assets.Length; i++)
                {
                    newArray[i] = assets[i];
                }
                newArray[newArray.Length - 1] = newAsset;
                assets = newArray;
            }
        }
    }
}
