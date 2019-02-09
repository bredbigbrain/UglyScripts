using UnityEngine;
using UnityEditor;

public class ScreenShoter : EditorWindow
{
    public Camera camera;
    public int textureSize = 512;
    public string fileName = "ScreenShot";
    public Object folderToSaveTextures;

    SerializedProperty folder, textureSizeS, fileNameS, cameraS;
    SerializedObject so;

    [MenuItem("Ugly tools/Screenshoter")]
    public static void ShowWindow()
    {
        var screenShoter = EditorWindow.GetWindow<ScreenShoter>();

        screenShoter.Init();
        screenShoter.Show();
    }

    public void Init()
    {
        so = new SerializedObject(this);
        folder = so.FindProperty("folderToSaveTextures");
        textureSizeS = so.FindProperty("textureSize");
        fileNameS = so.FindProperty("fileName");
        cameraS = so.FindProperty("camera");

        cameraS.objectReferenceValue = FindObjectOfType<Camera>();
    }

    void OnGUI()
    {
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(cameraS);
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(folder);
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(fileNameS);
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(textureSizeS);
        GUILayout.Space(5);

        GUILayout.BeginVertical();
        if (GUILayout.Button("Screen shot"))
        {
            MakeScreenShot();
        }

        GUILayout.EndVertical();

        so.ApplyModifiedProperties();
    }

    public void MakeScreenShot()
    {
        string folderPath = "";
        if (folderToSaveTextures == null)
        {
            folderPath = Application.dataPath + "/";
        }
        else
        {
            folderPath = AssetDatabase.GetAssetPath(folderToSaveTextures) + "/";
        }
        RenderTexture rt = new RenderTexture(textureSize, textureSize, 24);

        camera.targetTexture = rt;
        camera.Render();

        RenderTexture.active = rt;

        Texture2D virtualPhoto = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

        virtualPhoto.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);

        RenderTexture.active = null;
        camera.targetTexture = null;

        byte[] bytes = virtualPhoto.EncodeToPNG();

        System.IO.File.WriteAllBytes(folderPath + fileName + ".png", bytes);

        AssetDatabase.Refresh();

        Debug.Log("Saved: " + folderPath + fileName + ".png");
    }
}