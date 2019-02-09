using System.Collections;
using UnityEngine;

//Outline width hardcoded in shader (ScreeenspaceOutline) at line 50 (NumberOfIterations).
[RequireComponent(typeof(Camera))]
public class OutlinePostEffect : MonoBehaviour {

    public LayerMask cullingMask;
    [Space]
    public bool setObjectsColor = false;
    [SerializeField] private Color objectsColor, outlineColor;
    [Space]
    public Shader Post_Outline;

    public bool renderWithShader = false;
    public Shader renderShader;

    Camera outlineCamera;
    Material Post_Mat;
    RenderTexture tempRT;
    Color white;

    void Start()
    {
        white = Color.white;
        outlineCamera = new GameObject("OutlineCamera").AddComponent<Camera>();
        outlineCamera.enabled = false;
        outlineCamera.transform.SetParent(transform);

        outlineCamera.CopyFrom(GetComponent<Camera>());
        outlineCamera.clearFlags = CameraClearFlags.Color;
        outlineCamera.backgroundColor = Color.black;
        outlineCamera.cullingMask = cullingMask;

        Post_Mat = new Material(Post_Outline);
        Post_Mat.SetColor("_OutlineColor", outlineColor);
        Post_Mat.SetColor("_ObjectColor", setObjectsColor? objectsColor : white);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        tempRT = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.R8);
        tempRT.Create();

        outlineCamera.targetTexture = tempRT;
        if (renderWithShader)
        {
            outlineCamera.RenderWithShader(renderShader, "");
        }
        else
        {
            outlineCamera.Render();
        }

        Post_Mat.SetTexture("_SceneTex", source);

        Graphics.Blit(tempRT, destination, Post_Mat);

        RenderTexture.ReleaseTemporary(tempRT);
    }

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        Post_Mat.SetColor("_OutlineColor", outlineColor);
    }

    public void SetObjectsColor(bool set)
    {
        setObjectsColor = set;
        Post_Mat.SetColor("_ObjectColor", setObjectsColor? objectsColor : white);
    }

    public void SetObjectsColor(Color color)
    {
        setObjectsColor = true;
        objectsColor = color;
        Post_Mat.SetColor("_ObjectColor", objectsColor);
    }

    private void OnValidate()
    {
        if (Post_Mat != null)
        {
            if (setObjectsColor)
            {
                Post_Mat.SetColor("_ObjectColor", objectsColor);
            }
            else
            {
                Post_Mat.SetColor("_ObjectColor", white);
            }
            Post_Mat.SetColor("_OutlineColor", outlineColor);
        }
    }
}
