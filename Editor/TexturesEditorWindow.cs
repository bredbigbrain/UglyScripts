using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class TexturesEditorWindow : EditorWindow
{
    public class TextureItem
    {
        public Texture2D texture;
        public bool enable = true, pot = false, reimported = false;
        public float size = 0;
        public int heightOrigin, widthOrigin;
        public static float totalSize;
    }

    string path = "Assets/Models/_3d/_textures/";
    List<TextureItem> textures;
    Vector2 scrollPosition = Vector2.zero;
    TextureImporterFormat preferedRgbaFormatAndroid = TextureImporterFormat.PVRTC_RGBA2;
    TextureImporterFormat preferedRgbFormatAndroid = TextureImporterFormat.PVRTC_RGB2;

    TextureImporterFormat preferedRgbaFormatIOS = TextureImporterFormat.PVRTC_RGBA2;
    TextureImporterFormat preferedRgbFormatIOS = TextureImporterFormat.PVRTC_RGB2;

    bool set = false, enable = true, enableNpot = true, alphaIsTransparencyRgb = false, alphaIsTransparencyRgba = true, defauldReimport = false;
    bool setSize = false, setCompression = false, setIosCompression = true, setAndroidCompression = true, setIosSize = true, setAndroidSize = true;
    int index = 0, maxMaxSize = 1024, compressionQualityAndroid = 50, compressionQualityIOS = 50;

    int[] intPopupValues = new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
    string[] intPopupNames;

    Object selectedObject;

    [MenuItem("Ugly tools/Textures compression editor")]
    static void Init()
    {
        TexturesEditorWindow window = (TexturesEditorWindow)GetWindow(typeof(TexturesEditorWindow));

        window.titleContent = new GUIContent("Textures editor");
        window.SetDefaultValues();
        window.Show();
    }

    public void SetDefaultValues()
    {
        minSize = new Vector2(500, 455);
        maxSize = new Vector2(500, 4000);

        intPopupNames = new string[intPopupValues.Length];
        for (int i = 0; i < intPopupNames.Length; i++)
        {
            intPopupNames[i] = intPopupValues[i].ToString();
        }
    }

    void OnGUI()
    {
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Folder path");
        if (GUILayout.Button("Pick selected folder"))
        {
            path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/";
            LoadTextures();
        }

        if (GUILayout.Button("Pick selected textures"))
        {
            LoadSelectedTextures();
        }
        GUILayout.EndHorizontal();
        path = GUILayout.TextField(path);

        if (GUILayout.Button("Load textures from folder"))
        {
            LoadTextures();
        }
        GUILayout.Space(10);

        if (textures != null && textures.Count > 0)
        {
            setSize = GUILayout.Toggle(setSize, "Override 'Max Size' by texture minSize");
            if (setSize)
            {
                BeginHorizontalOffset(18);

                maxMaxSize = EditorGUILayout.IntPopup("Max 'Max Size'", maxMaxSize, intPopupNames, intPopupValues);

                GUILayout.BeginHorizontal();
                setAndroidSize = GUILayout.Toggle(setAndroidSize, "Android");
                setIosSize = GUILayout.Toggle(setIosSize, "iOS");
                GUILayout.EndHorizontal();

                EndHorizontalOffset();
            }

            GUILayout.Space(5);
            setCompression = GUILayout.Toggle(setCompression, "Override compression");
            if (setCompression)
            {
                BeginHorizontalOffset(18);

                setAndroidCompression = EditorGUILayout.BeginToggleGroup("Android", setAndroidCompression);
                compressionQualityAndroid = EditorGUILayout.IntSlider("Compression quality", compressionQualityAndroid, 0, 100);
                GUILayout.Space(3);

                preferedRgbaFormatAndroid = (TextureImporterFormat)EditorGUILayout.EnumPopup("Prefered RGBA Format:", preferedRgbaFormatAndroid);
                alphaIsTransparencyRgba = GUILayout.Toggle(alphaIsTransparencyRgba, "Alpha Is Transparency");
                GUILayout.Space(3);

                preferedRgbFormatAndroid = (TextureImporterFormat)EditorGUILayout.EnumPopup("Prefered RGB Format:", preferedRgbFormatAndroid);
                alphaIsTransparencyRgb = GUILayout.Toggle(alphaIsTransparencyRgb, "Alpha Is Transparency");

                EditorGUILayout.EndToggleGroup();
                GUILayout.Space(5);

                setIosCompression = EditorGUILayout.BeginToggleGroup("iOS", setIosCompression);
                compressionQualityIOS = EditorGUILayout.IntSlider("Compression quality", compressionQualityIOS, 0, 100);
                GUILayout.Space(3);

                preferedRgbaFormatIOS = (TextureImporterFormat)EditorGUILayout.EnumPopup("Prefered RGBA Format:", preferedRgbaFormatIOS);
                alphaIsTransparencyRgba = GUILayout.Toggle(alphaIsTransparencyRgba, "Alpha Is Transparency");
                GUILayout.Space(3);

                preferedRgbFormatIOS = (TextureImporterFormat)EditorGUILayout.EnumPopup("Prefered RGB Format:", preferedRgbFormatIOS);
                alphaIsTransparencyRgb = GUILayout.Toggle(alphaIsTransparencyRgb, "Alpha Is Transparency");

                EditorGUILayout.EndToggleGroup();
                EndHorizontalOffset();
            }

            GUILayout.Space(10);
            if ((setSize && (setIosSize || setAndroidSize)) || (setCompression && (setIosCompression || setAndroidCompression)))
            {
                if (GUILayout.Button("Apply new settings", GUILayout.Height(50)))
                {
                    index = 0;
                    selectedObject = Selection.activeObject;
                    Selection.activeObject = null;
                    set = true;
                    defauldReimport = false;
                }
            }

            if (GUILayout.Button("Default reimport", GUILayout.Height(20)))
            {
                index = 0;
                selectedObject = Selection.activeObject;
                Selection.activeObject = null;
                set = true;
                defauldReimport = true;
            }
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            enable = textures[0].enable;
            if (GUILayout.Button(enable? "Disable all" : "Enable all", GUILayout.Width(110)))
            {
                enable = !enable;
                enableNpot = enable;
                for (int j = 0; j < textures.Count; j++)
                {
                    textures[j].enable = enable;
                }
            }

            if (GUILayout.Button(enableNpot ? "Disable NPOT" : "Enable NPOT", GUILayout.Width(110)))
            {
                enableNpot = !enableNpot;
                for (int j = 0; j < textures.Count; j++)
                {
                    if(!textures[j].pot)
                    {
                        textures[j].enable = enableNpot;
                    }
                }
            }

            GUILayout.Space(maxSize.x - 470);
            GUILayout.Label("Origin", GUILayout.Width(70));
            GUILayout.Label("Imported", GUILayout.Width(70));

            GUILayout.Label(TextureItem.totalSize.ToString("0.000") + " Mb", GUILayout.Width(70));

            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);            
            for (int j = 0; j < textures.Count; j++)
            {
                if(textures[j].texture == null)
                {
                    textures = null;
                    set = false;
                    index = 0;
                    GUILayout.EndScrollView();
                    return;
                }
                GUI.color = j % 2 == 0? Color.red / 10 : new Color(0,0,0,0);
                EditorGUI.DrawPreviewTexture(new Rect(0, 2 + j * 19 , maxSize.x, 17), Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.backgroundColor = textures[j].pot ? Color.white : Color.gray * 1.5f;
                GUILayout.BeginHorizontal();

                GUI.color = textures[j].reimported ? Color.green : Color.white;
                textures[j].enable = GUILayout.Toggle(textures[j].enable, textures[j].texture.name, GUILayout.Width(maxSize.x - 290), GUILayout.Height(17));
                GUI.color = Color.white;

                if (GUILayout.Button(textures[j].pot ? "" : "NPOT", "Label", GUILayout.Width(40), GUILayout.Height(17)) ||
                    GUILayout.Button(textures[j].widthOrigin.ToString() + "x" + textures[j].heightOrigin.ToString(), "Label", GUILayout.Width(70), GUILayout.Height(17)) ||
                    GUILayout.Button(textures[j].texture.width.ToString() + "x" + textures[j].texture.height.ToString(), "Label", GUILayout.Width(70), GUILayout.Height(17)) ||
                    GUILayout.Button(textures[j].size.ToString("0.0000") + " Mb", "Label", GUILayout.Width(70), GUILayout.Height(17)))
                {
                    Selection.activeObject = textures[j].texture;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }

    void LoadTextures()
    {
        textures = new List<TextureItem>();
        var tepm = path.Replace("Assets", "");
        var aMaterialFiles = Directory.GetFiles(Application.dataPath + tepm);

        Texture2D texture;
        string assetPath;
        TextureImporter importer;
        foreach (string matFile in aMaterialFiles)
        {
            if (!matFile.ToLower().Contains(".meta"))
            {
                assetPath = "Assets" + matFile.Replace(Application.dataPath, "").Replace('\\', '/');
                texture = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));

                importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
                object[] args = new object[2] { 0, 0 };
                MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                mi.Invoke(importer, args);

                if (texture != null)
                {
                    textures.Add(new TextureItem() { texture = texture, pot = (int)args[0] == (int)args[1], widthOrigin = (int)args[0], heightOrigin = (int)args[1] });
                }
            }
        }

        CalculateTexturesSize();

        if (textures.Count == 0)
        {
            Debug.Log("No textures in folder");
        }
    }

    void LoadSelectedTextures()
    {
        path = "";
        textures = new List<TextureItem>();

        var selection = Selection.objects;

        Texture2D texture;
        string assetPath;
        TextureImporter importer;
        foreach (var selected in selection)
        {
            assetPath = AssetDatabase.GetAssetPath(selected);
            texture = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
            importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);

            importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
            
            object[] args = new object[2] { 0, 0 };
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(importer, args);

            if (texture != null)
            {
                textures.Add(new TextureItem() { texture = texture, pot = (int)args[0] == (int)args[1], widthOrigin = (int)args[0], heightOrigin = (int)args[1] });
            }
        }

        CalculateTexturesSize();

        if (textures.Count == 0)
        {
            Debug.Log("No textures in selection");
        }
    }

    void CalculateTexturesSize()
    {
        if(textures != null)
        {
            TextureItem.totalSize = 0;
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].size = textures[i].texture.GetRawTextureData().Length / 1024f / 1024f;
                TextureItem.totalSize += textures[i].size;

                textures[i].reimported = false;
            }
        }
    }

    void UseSettings()
    {
        var tex2D = textures[index].texture;
        var texPath = AssetDatabase.GetAssetPath(tex2D);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texPath);

        if(defauldReimport)
        {
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings() { overridden = false , name = "Android" });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings() { overridden = false , name = "iPhone" });
        }
        else
        {
            if(setAndroidCompression || setAndroidSize)
            {
                SetPlarfomSettings(importer, tex2D, "Android");
            }
            if(setIosCompression || setIosSize)
            {
                SetPlarfomSettings(importer, tex2D, "iPhone");
            }
        }
        
        AssetDatabase.WriteImportSettingsIfDirty(texPath);
        importer.SaveAndReimport();

        textures[index].reimported = true;
        index++;
    }

    void SetPlarfomSettings(TextureImporter importer, Texture2D tex2D, string platfom)
    {
        var oldSetttings = importer.GetPlatformTextureSettings(platfom);
        oldSetttings.overridden = true;
        AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        
        if (setSize)
        {
            int maxSize = -1;
            
            if (textures[index].heightOrigin <= 32)
            {
                maxSize = 32;
            }
            else if (textures[index].heightOrigin >= maxMaxSize)
            {
                maxSize = maxMaxSize;
            }
            else
            {
                maxSize = textures[index].heightOrigin;
            }
            if (maxSize != -1)
            {
                oldSetttings.maxTextureSize = maxSize;
            }
        }
        if (setCompression)
        {
            if (importer.DoesSourceTextureHaveAlpha())
            {
                oldSetttings.format = platfom == "Android" ? preferedRgbaFormatAndroid: preferedRgbaFormatIOS;
                importer.alphaIsTransparency = alphaIsTransparencyRgba;
            }
            else
            {
                oldSetttings.format = platfom == "Android" ? preferedRgbFormatAndroid : preferedRgbFormatIOS;
                importer.alphaIsTransparency = alphaIsTransparencyRgb;
            }

            oldSetttings.compressionQuality = platfom == "Android" ? compressionQualityAndroid : compressionQualityIOS;
        }

        importer.SetPlatformTextureSettings(oldSetttings);
    }
    
    void Update()
    {
        if (textures != null && textures.Count > 0 && set)
        {
            if (index < textures.Count)
            {
                float progress = index / (float)(textures.Count - 1);
                string note = (index).ToString() + "/" + textures.Count.ToString() + " " + textures[index].texture.name;
                if (EditorUtility.DisplayCancelableProgressBar("Applying", note, progress))
                {
                    index = 0;
                    set = false;
                    CalculateTexturesSize();
                    Selection.activeObject = selectedObject;
                    selectedObject = null;
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    while (index < textures.Count)
                    {
                        if (textures[index].enable)
                        {
                            UseSettings();
                            break;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
            }
            else
            {
                CalculateTexturesSize();
                Selection.activeObject = selectedObject;
                EditorUtility.ClearProgressBar();
                set = false;
                selectedObject = null;
            }
            Repaint();
        }
    }

    void BeginHorizontalOffset(int offset)
    {
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(offset));
        GUILayout.Space(10);
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
    }
    void EndHorizontalOffset()
    {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}