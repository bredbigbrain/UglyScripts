using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CubeMapCreatorWindow : EditorWindow
{
    public Camera camera;
    public Object folderToSaveTextures;
    public string cubemapName = "CubeMap";
    public int textureSize = 512;

    SerializedObject serializedObject;
    SerializedProperty folder, cubeMapNameS, textureSizeS, cameraS;

    [MenuItem("Ugly tools/Cubemap creator")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<CubeMapCreatorWindow>();

        window.Init();
        window.Show();
    }

    public void Init()
    {
        serializedObject = new SerializedObject(this);

        folder = serializedObject.FindProperty("folderToSaveTextures");
        cameraS = serializedObject.FindProperty("camera");
        cubeMapNameS = serializedObject.FindProperty("cubemapName");
        textureSizeS = serializedObject.FindProperty("textureSize");

        cameraS.objectReferenceValue = FindObjectOfType<Camera>();
    }

    private void OnGUI()
    {
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(cameraS);
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(folder);
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(cubeMapNameS);
        EditorGUILayout.PropertyField(textureSizeS);
        GUILayout.Space(5);

        GUILayout.BeginVertical();
        if (GUILayout.Button("Create cubemap"))
        {
            CreateCubemap();
        }
        if (GUILayout.Button("Create cubemap sides"))
        {
            CreateCubeMapSides();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }


    public void CreateCubemap()
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

        var texture = CubeMapCreator.CreateCubemapTexture(camera, textureSize);
        
        File.WriteAllBytes(folderPath + cubemapName + ".png", texture.EncodeToPNG());

        AssetDatabase.Refresh();

        try
        {
            TextureImporter textureImporter = TextureImporter.GetAtPath(folderPath + cubemapName + ".png") as TextureImporter;
            textureImporter.textureShape = TextureImporterShape.TextureCube;
            textureImporter.SaveAndReimport();
        }
        catch { }

        Debug.Log("Saved : " + folderPath + cubemapName + ".png", folderToSaveTextures);
    }

    public void CreateCubeMapSides()
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

        Cubemap cubemap = new Cubemap(textureSize, TextureFormat.RGB24, false);

        camera.RenderToCubemap(cubemap);

        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.PositiveX));
        var bytes = tex.EncodeToPNG();
        File.WriteAllBytes(folderPath + cubemapName + "_PositiveX.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.NegativeX));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(folderPath + cubemapName + "_NegativeX.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.PositiveY));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(folderPath + cubemapName + "_PositiveY.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.NegativeY));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(folderPath + cubemapName + "_NegativeY.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.PositiveZ));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(folderPath + cubemapName + "_PositiveZ.png", bytes);

        tex.SetPixels(cubemap.GetPixels(CubemapFace.NegativeZ));
        bytes = tex.EncodeToPNG();
        File.WriteAllBytes(folderPath + cubemapName + "_NegativeZ.png", bytes);

        AssetDatabase.Refresh();

        DestroyImmediate(tex);

        Debug.Log("Saved to: " + folderPath, folderToSaveTextures);
    }
}