using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class UglyTools: MonoBehaviour
{
    [MenuItem("Ugly tools/Delete PlayerPrefs")]
    public static void DeletePrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs deleted!");
    }

    [MenuItem("Ugly tools/Log selected count")]
    public static void SelectedCount()
    {
        Debug.Log("Selected: " + Selection.gameObjects.Length + " objects");
    }

    [MenuItem("Ugly tools/Delete selected colliders (include childrens)")]
    public static void DeleteSelectedColliders()
    {
        int count = 0;
        foreach (var go in Selection.objects)
        {
            Collider[] colliders = null;
            try
            {
                colliders = (go as GameObject).GetComponentsInChildren<Collider>();
            }
            catch { }

            if (colliders != null)
            {
                foreach (var collider in colliders)
                {
                    Undo.DestroyObjectImmediate(collider);
                    count++;
                }
            }
        }

        Debug.Log(count + " colliders deleted");
    }

    [MenuItem("Ugly tools/Create Selected ScriptableObject")]
    static void CreateScriptableObject()
    {
        try
        {
            var obj = Selection.activeObject;
            ScriptableObject data = ScriptableObject.CreateInstance(obj.name);
            AssetDatabase.CreateAsset(data, Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj))+ "/" + obj.name + ".asset");
            AssetDatabase.SaveAssets();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    [MenuItem("Ugly tools/Save all selected prefabs")]
    public static void SaveAllPrefabs()
    {
        GameObject[] objects = Selection.gameObjects;
        Undo.RecordObjects(objects, "Save selected prefabs");
        foreach (var go in objects)
        {
            GameObject target = PrefabUtility.GetPrefabParent(go) as GameObject;
            if (target != null)
            {
                PrefabUtility.ReplacePrefab(go, target);
                //PrefabUtility.RevertPrefabInstance(go);
            }
        }
    }
    [MenuItem("Ugly tools/Revert all selected prefabs")]
    public static void RevertAllPrefabs()
    {
        GameObject[] objects = Selection.gameObjects;
        Undo.RecordObjects(objects, "Revert selected prefabs");
        foreach (var go in objects)
        {
            GameObject target = PrefabUtility.GetPrefabParent(go) as GameObject;
            if (target != null)
            {
                //PrefabUtility.ReplacePrefab(go, target);
                PrefabUtility.RevertPrefabInstance(go);
            }
        }
    }

    [MenuItem("Ugly tools/Disable pointed collider %q")]
    public static void DisablePntdCollider()
    {
        EditorWindow.GetWindow<ColliderDisaabler>().Show();
    }

    public class ColliderDisaabler : EditorWindow
    {
        public new void Show()
        {
            minSize = Vector2.one;
            maxSize = Vector2.one;
            base.Show();
        }

        private void OnEnable()
        {
            SceneView.onSceneGUIDelegate += SceneGUI;
        }

        private void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= SceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= SceneGUI;
        }

        private void SceneGUI(SceneView sceneView)
        {
            // This will have scene events including mouse down on scenes objects
            var cur = Event.current;
            AddPoints();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Reload objects"))
            {
                OnEnable();
            }
        }

        private void AddPoints()
        {
            if (Event.current != null)
            {
                UseEvent();
                var ray = Camera.current.ScreenPointToRay(new Vector2(Event.current.mousePosition.x, Camera.current.pixelHeight - Event.current.mousePosition.y));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var colliders = hit.collider.GetComponents<Collider>();
                    if (colliders.Length > 0)
                    {
                        Undo.RecordObject(hit.collider, "Disable collider");
                        int j = -1;
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            if (hit.collider == colliders[i])
                            {
                                j = i;
                            }
                        }
                        hit.collider.enabled = !hit.collider.enabled;
                        Debug.Log("Disabled: " + hit.collider.gameObject + " object's collider (" + j + ")", hit.collider.gameObject);
                    }
                }
            }
            Close();
        }

        private void UseEvent()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
            if (Event.current != null && Event.current.button == 0)// && (Event.current.type == EventType.mouseUp || Event.current.type == EventType.mouseDrag))
            {
                Event.current.Use();
            }
        }
    }
}