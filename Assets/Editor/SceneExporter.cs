using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;

public class SceneExporter : EditorWindow
{
    private string outputPath = "";
    private bool showInactive = true;
    private Vector2 scroll;
    private string preview = "";

    [MenuItem("Tools/Scene Exporter")]
    public static void Open()
    {
        GetWindow<SceneExporter>("场景导出工具");
    }

    void OnEnable()
    {
        outputPath = Path.Combine(Application.dataPath, "..", "SceneDump.txt");
    }

    void OnGUI()
    {
        GUILayout.Label("导出当前场景的 Hierarchy 结构和组件引用", EditorStyles.boldLabel);
        GUILayout.Space(8);

        showInactive = EditorGUILayout.Toggle("包含未激活物体", showInactive);

        GUILayout.BeginHorizontal();
        GUILayout.Label("输出文件：", GUILayout.Width(70));
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("选择", GUILayout.Width(50)))
        {
            string picked = EditorUtility.SaveFilePanel("保存到", Path.GetDirectoryName(outputPath), "SceneDump", "txt");
            if (!string.IsNullOrEmpty(picked)) outputPath = picked;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新预览", GUILayout.Height(30)))
            preview = Dump();

        if (GUILayout.Button("导出到文件", GUILayout.Height(30)))
        {
            string content = Dump();
            File.WriteAllText(outputPath, content, Encoding.UTF8);
            GameLog.Info($"[SceneExporter] 已导出到: {outputPath}");
            EditorUtility.RevealInFinder(outputPath);
        }

        if (GUILayout.Button("复制到剪贴板", GUILayout.Height(30)))
        {
            string content = Dump();
            EditorGUIUtility.systemCopyBuffer = content;
            GameLog.Info($"[SceneExporter] 已复制，共 {content.Length} 字符");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label($"预览（{preview.Length} 字符）：", EditorStyles.boldLabel);
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.TextArea(preview, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    string Dump()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"// ===== Scene Dump =====");
        sb.AppendLine($"// 场景: {scene.name}");
        sb.AppendLine($"// 时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"// 根物体数: {roots.Length}");
        sb.AppendLine();

        foreach (GameObject root in roots)
        {
            DumpGameObject(root, sb, 0);
        }

        return sb.ToString();
    }

    void DumpGameObject(GameObject go, StringBuilder sb, int depth)
    {
        if (!showInactive && !go.activeInHierarchy) return;

        string indent = new string(' ', depth * 2);
        string activeMark = go.activeSelf ? "" : " [未激活]";

        sb.AppendLine($"{indent}- {go.name}{activeMark}");

        // RectTransform 信息
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            sb.AppendLine($"{indent}  [RectTransform] pos={rt.anchoredPosition} size={rt.sizeDelta} anchor={rt.anchorMin}~{rt.anchorMax}");
        }

        // 遍历所有组件
        Component[] comps = go.GetComponents<Component>();
        foreach (Component c in comps)
        {
            if (c == null)
            {
                sb.AppendLine($"{indent}  [Missing Script!]");
                continue;
            }

            if (c is Transform || c is RectTransform) continue;

            // 只列脚本，跳过 Image/Text 等基础组件？不，都列出来更全
            string typeName = c.GetType().Name;
            sb.AppendLine($"{indent}  [{typeName}]");

            // 用 SerializedObject 遍历所有字段，把引用显示出来
            SerializedObject so = new SerializedObject(c);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;

                // 跳过 m_Script 自己
                if (prop.propertyPath == "m_Script") continue;

                string value = GetPropValue(prop, depth);
                if (!string.IsNullOrEmpty(value))
                    sb.AppendLine($"{indent}      {prop.displayName}: {value}");
            }
        }

        // 递归子物体
        for (int i = 0; i < go.transform.childCount; i++)
        {
            DumpGameObject(go.transform.GetChild(i).gameObject, sb, depth + 1);
        }
    }

    string GetPropValue(SerializedProperty prop, int depth)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.ObjectReference:
                if (prop.objectReferenceValue == null) return "(None)";
                // 拖引用显示成物体的完整路径
                GameObject refGo = prop.objectReferenceValue as GameObject;
                if (refGo != null) return GetGameObjectPath(refGo);
                Component refComp = prop.objectReferenceValue as Component;
                if (refComp != null) return GetGameObjectPath(refComp.gameObject) + $" ({refComp.GetType().Name})";
                return prop.objectReferenceValue.name;

            case SerializedPropertyType.String:
                return string.IsNullOrEmpty(prop.stringValue) ? "" : prop.stringValue;

            case SerializedPropertyType.Integer:
            case SerializedPropertyType.Float:
            case SerializedPropertyType.Boolean:
                return prop.propertyType == SerializedPropertyType.Boolean
                    ? prop.boolValue.ToString()
                    : prop.propertyType == SerializedPropertyType.Integer
                        ? prop.intValue.ToString()
                        : prop.floatValue.ToString();

            case SerializedPropertyType.Enum:
                return prop.enumDisplayNames.Length > prop.enumValueIndex && prop.enumValueIndex >= 0
                    ? prop.enumDisplayNames[prop.enumValueIndex]
                    : prop.enumValueIndex.ToString();

            default:
                return "";
        }
    }

    string GetGameObjectPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}