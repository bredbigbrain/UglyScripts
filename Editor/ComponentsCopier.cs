using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public class ComponentsCopier : EditorWindow
{

    [MenuItem("Ugly tools/Components copier")]
    static void Init()
    {
        ComponentsCopier window = (ComponentsCopier)GetWindow(typeof(ComponentsCopier));

        window.minSize = new Vector2(430, 250);
        window.maxSize = new Vector2(430, 4000);

        window.targetObjects = new GameObject[0];
        window.targetObjectsSO = new SerializedObject(window);

        window.Show();
    }

    GameObject templateObject;
    GameObject lastTemplateObject;

    public GameObject[] targetObjects;
    public SerializedObject targetObjectsSO;

    Component[] components;
    bool[] enabled;
    Vector2 scrollPosition = Vector2.zero;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Template GameObject");
        templateObject = (GameObject)EditorGUILayout.ObjectField("", templateObject, typeof(GameObject), true);
        if (templateObject == null)
        {
            lastTemplateObject = null;
        }
        if (lastTemplateObject != templateObject && templateObject != null)
        {
            lastTemplateObject = templateObject;
            components = templateObject.GetComponents<Component>();
            enabled = new bool[components.Length];
        }

        GUILayout.Space(5);

        EditorGUILayout.PropertyField(targetObjectsSO.FindProperty("targetObjects"), true);
        targetObjectsSO.ApplyModifiedProperties();

        GUILayout.Space(5);

        if (templateObject != null && components.Length > 1)
        {
            if (targetObjects.Length > 0)
            {
                GUILayout.Space(3);
                if (GUILayout.Button("Copy"))
                {
                    CopyComponents();
                }

                try
                {
                    GUILayout.Space(3);
                }
                catch { }
            }

            try
            {
                if (GUILayout.Button("Enable/disable all", GUILayout.Width(110)))
                {
                    bool enable = enabled[1];
                    for (int j = 1; j < enabled.Length; j++)
                    {
                        enabled[j] = !enable;
                    }
                }

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                for (int i = 1; i < components.Length; i++)
                {
                    enabled[i] = GUILayout.Toggle(enabled[i], components[i].GetType().ToString());
                }
                GUILayout.EndScrollView();
            }
            catch
            {
            }
        }
    }

    void CopyComponents()
    {
        for (int i = 1; i < components.Length; i++)
        {
            if (enabled[i])
            {
                var type = components[i].GetType();
                for (int j = 0; j < targetObjects.Length; j++)
                {
                    if (targetObjects[j] != null)
                    {
                        var comp = Undo.AddComponent(targetObjects[j], type);
                        Copy(comp, components[i]);
                    }
                }

            }
        }
    }

    void Copy<T>(T targetComp, T templateComp)
    {
        Component compTempl = null;
        Component compTarg = targetComp as Component;
        if (compTarg != null)
        {
            compTempl = templateComp as Component;
        }
        if (compTempl != null)
        {
            if (UnityEditorInternal.ComponentUtility.CopyComponent(compTempl))
            {
                if (!UnityEditorInternal.ComponentUtility.PasteComponentValues(compTarg))
                {
                    Debug.Log(targetComp.GetType().ToString() + " not copied");
                }
            }
            else
            {
                Debug.Log(targetComp.GetType().ToString() + " not copied");
            }
        }
    }
}