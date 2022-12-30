using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FindAssetWindow : EditorWindow
{
    private List<string> m_resultList = new List<string>();
    private string[] m_modes = new string[] { "Search for monoscript", "Search for component name" };

    private MonoScript m_checkUsageComponent;

    private Vector2 m_scroll;
    private int m_mode;

    private string m_componentName;
    private string m_tempComponentName;

    private IReadOnlyList<string> ResultList => m_resultList;
    private string ComponentName => m_mode == 0 ? m_checkUsageComponent?.name : m_componentName;

    [MenuItem("CustomEditor/Find Asset For MonoScript")]
    public static void CustomEditorWindow()
    {
        FindAssetWindow window = (FindAssetWindow)GetWindow(typeof(FindAssetWindow));
        window.Show();
        window.titleContent.text = "Find Asset Window";
    }

    private void OnGUI()
    {
        #region Select Search Option
        EditorGUI.BeginChangeCheck();

        m_mode = GUILayout.Toolbar(m_mode, m_modes);

        if(EditorGUI.EndChangeCheck())
        {
            ClearScreen();
        }

        GUILayout.Space(10);
        #endregion

        switch (m_mode)
        {
            case 0:
                SearchForMonoScript();
                break;
            case 1:
                SearchForComponentName();
                break;
        }

        PrintScreen();
    }

    private void SearchForMonoScript()
    {
        EditorGUI.BeginChangeCheck();

        m_checkUsageComponent = (MonoScript)EditorGUILayout.ObjectField(m_checkUsageComponent, typeof(MonoScript), false);

        if (EditorGUI.EndChangeCheck())
        {
            FindAssetForMonoScript();
        }

    }

    private void SearchForComponentName()
    {
        m_tempComponentName = EditorGUILayout.TextField(m_tempComponentName);

        GUILayout.Space(10);

        if (GUILayout.Button("Find Usage Component Asset"))
        {
            m_componentName = m_tempComponentName;
            FindAssetForComponentName();
        }

    }

    private void FindAssetForComponentName()
    {
        AssetDatabase.SaveAssets();

        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        m_resultList.Clear();

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object[] components = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (var component in components)
            {
                string componentType = component.GetType().Name;

                if (componentType == m_componentName)
                {
                    m_resultList.Add(assetPath);
                }
            }
        }

    }

    private void FindAssetForMonoScript()
    {
        AssetDatabase.SaveAssets();

        string targetPath = AssetDatabase.GetAssetPath(m_checkUsageComponent);
        string[] allPrefabs = GetAllPrefabs();

        m_resultList.Clear();

        foreach (var prefab in allPrefabs)
        {
            string[] dependencies = AssetDatabase.GetDependencies(new string[] { prefab });

            foreach (var dependedAsset in dependencies)
            {
                if (dependedAsset.Equals(targetPath))
                {
                    m_resultList.Add(prefab);
                }
            }
        }
    }

    private void ClearScreen()
    {
        GUI.FocusControl(null);
        m_checkUsageComponent = null;
        m_componentName = m_tempComponentName = "";
        m_resultList.Clear();

        PrintScreen();
    }

    private void PrintScreen()
    {
        GUIStyle middleCenterAlignment = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        GUILayout.Space(10);

        if(ResultList.Count <= 0)
        {
            if(m_mode == 0)
            {
                GUILayout.Label(ComponentName == null ? "Choose Component" : $"No Prefabs Using Component <{ComponentName}>", middleCenterAlignment);
            }
            else
            {
                GUILayout.Label(ComponentName == "" ? "Enter Component Name" : $"No Prefabs Using Component <{ComponentName}>", middleCenterAlignment);
            }
        }
        else
        {
            GUILayout.Label("Prefabs Using Component", middleCenterAlignment);
            GUILayout.Space(10);
            m_scroll = GUILayout.BeginScrollView(m_scroll);

            foreach (var result in ResultList)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(result, GUILayout.Width(position.width / 2));

                if (GUILayout.Button("Select", GUILayout.Width(position.width / 2 - 10)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(result);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }

    public static string[] GetAllPrefabs()
    {
        List<string> prefabList = new List<string>();

        foreach (var item in AssetDatabase.FindAssets("t:Prefab"))
        {
            prefabList.Add(AssetDatabase.GUIDToAssetPath(item));
        }
        //AssetDatabase.LoadAllAssetsAtPath

        return prefabList.ToArray();
    }
}
