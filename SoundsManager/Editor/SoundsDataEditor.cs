using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;

[CustomEditor(typeof(SoundsData))]
public class SoundsDataEditor : Editor{

    public Object enumFile;
    string data, path = "", enumName = "";
    public List<string> literals;

    private bool show = false;
    private float[] listHeights;
    private ReorderableList list;
    private ReorderableList[] clipLists;
    private SoundsData soundsData;
    private SerializedObject so;
    private SerializedProperty sp;

    private void OnEnable()
    {
        soundsData = target as SoundsData;
        so = new SerializedObject(soundsData);
        literals = new List<string>();
        ReadEnum();
        NewList();
    }

    public override void OnInspectorGUI()
    {
        EditSoundTypeUI();
        if (!show)
        {
            //base.OnInspectorGUI();
            MainUI();
        }
    }

    void ReadEnum()
    {
        if (enumFile != null)
        {
            path = Application.dataPath.Replace("Assets", AssetDatabase.GetAssetPath(enumFile));
        }
        else
        {
            string pathLocal = "";
            var assets = AssetDatabase.FindAssets("SoundType");
            for (int i = 0; i < assets.Length; i++)
            {
                if (AssetDatabase.GUIDToAssetPath(assets[i]).EndsWith("SoundType.cs"))
                {
                    pathLocal = AssetDatabase.GUIDToAssetPath(assets[i]);
                }
            }
            if (pathLocal == "")
            {
                Debug.LogError("Cant find SoundType.cs", target);
            }
            path = Application.dataPath.Replace("Assets", pathLocal);
        }

        if (!string.IsNullOrEmpty(path))
        {
            StreamReader reader = new StreamReader(path);
            reader.ReadLine();
            enumName = reader.ReadLine();
            reader.ReadLine();
            data = reader.ReadLine();

            literals = new List<string>();
            foreach (var item in data.Split(','))
            {
                literals.Add(item);
            }
            reader.Close();
        }
    }

    void EditSoundTypeUI()
    {
        if (show)
        {
            if (!string.IsNullOrEmpty(path))
            {
                GUILayout.Label("enum " + enumName);

                for (int i = 0; i < literals.Count; i++)
                {
                    GUILayout.BeginHorizontal();

                    literals[i] = GUILayout.TextArea(literals[i]);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        literals.RemoveAt(i);
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add literal"))
                {
                    literals.Add("New_" + literals.Count);
                }

                if (GUILayout.Button("Save"))
                {
                    StreamWriter writer = new StreamWriter(path);

                    writer.WriteLine("public enum");
                    writer.WriteLine(enumName);
                    writer.WriteLine("{");

                    string line = "";
                    foreach (var item in literals)
                    {
                        line = line + item + ",";
                    }
                    line = line.Remove(line.Length - 1);

                    writer.WriteLine(line);
                    writer.WriteLine("}");

                    writer.Close();

                    AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(enumFile));
                    importer.SaveAndReimport();

                    show = false;
                }

                if (GUILayout.Button("Close"))
                {
                    show = false;
                }
            }
            else
            {
                enumFile = (Object)EditorGUILayout.ObjectField("SoundType.cs", enumFile, typeof(Object), false);
                if (enumFile != null)
                {
                    if (GUILayout.Button("Read enum"))
                    {
                        ReadEnum();
                    }
                }
                if (GUILayout.Button("Close"))
                {
                    show = false;
                }
            }
        }
        else
        {
            if(GUILayout.Button("Edit enum SoundType"))
            {
                ReadEnum();
                show = true;
            }
        }
    }

    void MainUI()
    {
        EditorGUI.BeginChangeCheck();
        list.DoLayoutList();
        if (EditorGUI.EndChangeCheck())
        {
            so.ApplyModifiedProperties();
            //NewList();
        }
    }

    void NewList()
    {
        listHeights = new float[soundsData.sounds.Length];
        list = new ReorderableList(so, so.FindProperty("sounds"), true, true, true, true)
        {
            drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Sounds");
            },

            drawElementCallback = DrawElement,
            drawElementBackgroundCallback = DrawBackground,
            elementHeightCallback = GetHeight
        };

        clipLists = new ReorderableList[soundsData.sounds.Length];

        for (int i = 0; i < clipLists.Length; i++)
        {
            NewClipsList(i);
        }
    }

    void NewClipsList(int index)
    {
        clipLists[index] = new ReorderableList(so, so.FindProperty("sounds").GetArrayElementAtIndex(index).FindPropertyRelative("clips"), true, true, true, true)
        {
            drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Clips");
            },
            drawElementCallback = (Rect rect, int index_, bool isActive, bool isFocused) =>
            {
                try
                {
                    soundsData.sounds[index].clips[index_] = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), soundsData.sounds[index].clips[index_], typeof(AudioClip), true) as AudioClip;
                }
                catch
                {
                    NewClipsList(index);
                }
            },
            elementHeightCallback = (int index_) =>
            {
                return EditorGUIUtility.singleLineHeight;
            },
            drawElementBackgroundCallback = (Rect rect, int index_, bool isActive, bool isFocused) =>
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                Texture2D tex = new Texture2D(1, 1);
                if (clipLists[index].index == index_)
                {
                    tex.SetPixel(0, 0, new Color(0.33f, 0.66f, 1, 0.4f));
                }
                else
                {
                    tex.SetPixel(0, 0, index_ % 2 == 1 ? new Color(0.2f, 0.2f, 0.2f, 0.1f) : new Color(1, 1, 1, 0));
                }
                tex.Apply();
                GUI.DrawTexture(rect, tex as Texture);
            }
        };
    }

    float GetHeight(int index)
    {
        try
        {
            return listHeights[index];
        }
        catch
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

    void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        try
        {
            rect.y += 2;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), soundsData.sounds[index].type.ToString(), new GUIStyle() { fontStyle = FontStyle.Bold });

            EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2f, rect.width / 4f, EditorGUIUtility.singleLineHeight), "Type: ");
            soundsData.sounds[index].type = (SoundType)EditorGUI.EnumPopup(new Rect(rect.x + rect.width / 4f, rect.y + EditorGUIUtility.singleLineHeight + 2f, rect.width / 4f, EditorGUIUtility.singleLineHeight), soundsData.sounds[index].type);

            EditorGUI.LabelField(new Rect(rect.x + rect.width / 1.95f, rect.y + EditorGUIUtility.singleLineHeight + 2f, rect.width * 4f, EditorGUIUtility.singleLineHeight), "Group: ");
            soundsData.sounds[index].group = (SoundsManager.SoundGroup)EditorGUI.EnumPopup(new Rect(rect.x + rect.width * 3f / 4f, rect.y + EditorGUIUtility.singleLineHeight + 2f, rect.width / 4f, EditorGUIUtility.singleLineHeight), soundsData.sounds[index].group);

            EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 2f + 2f, rect.width / 4f, EditorGUIUtility.singleLineHeight), "Loop: ");
            soundsData.sounds[index].loop = EditorGUI.Toggle(new Rect(rect.x + rect.width / 4f, rect.y + EditorGUIUtility.singleLineHeight * 2f + 2f, rect.width / 4f, EditorGUIUtility.singleLineHeight), soundsData.sounds[index].loop);

            EditorGUI.LabelField(new Rect(rect.x + rect.width / 1.95f, rect.y + EditorGUIUtility.singleLineHeight * 2f + 2f, rect.width * 4f, EditorGUIUtility.singleLineHeight), "Is Sequence: ");
            soundsData.sounds[index].isSequence = EditorGUI.Toggle(new Rect(rect.x + rect.width * 2.9f / 4f, rect.y + EditorGUIUtility.singleLineHeight * 2f + 2f, rect.width / 4f, EditorGUIUtility.singleLineHeight), soundsData.sounds[index].isSequence);

            soundsData.sounds[index].volume = EditorGUI.FloatField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 3f + 2f, rect.width, EditorGUIUtility.singleLineHeight), "Volume: ", soundsData.sounds[index].volume);
            soundsData.sounds[index].maxActive =  EditorGUI.IntField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 4f + 2f, rect.width, EditorGUIUtility.singleLineHeight), "Max active: ", soundsData.sounds[index].maxActive);

            //sp = so.FindProperty("sounds").GetArrayElementAtIndex(index).FindPropertyRelative("clips");
            //EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 5f + 2f, rect.width, EditorGUIUtility.singleLineHeight), sp, true);

            clipLists[index].DoList(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 5f + 4f, rect.width, soundsData.sounds[index].clips.Length * EditorGUIUtility.singleLineHeight));

            listHeights[index] = EditorGUIUtility.singleLineHeight * 5f + 6 + soundsData.sounds[index].clips.Length * (EditorGUIUtility.singleLineHeight + 5f) + 35f;
        }
        catch
        {
            NewList();
        }
    }

    void DrawBackground(Rect rect, int index, bool isActive, bool isFocused)
    {
        rect.height = EditorGUIUtility.singleLineHeight;
        Texture2D tex = new Texture2D(1, 1);
        if (list.index == index)
        {
            tex.SetPixel(0, 0, new Color(0.33f, 0.66f, 1, 0.4f));
        }
        else
        {
            tex.SetPixel(0, 0, index % 2 == 1 ? new Color(0.2f, 0.2f, 0.2f, 0.1f) : new Color(1, 1, 1, 0));
        }
        tex.Apply();
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, listHeights[index]), tex as Texture);
    }
}