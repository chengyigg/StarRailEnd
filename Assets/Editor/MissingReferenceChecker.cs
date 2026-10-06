using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Text;

public class MissingReferenceChecker : EditorWindow
{
    private Vector2 scroll;
    private string result = "";
    private bool onlyScriptsInAssets = true;

    [MenuItem("Tools/Missing Reference Checker")]
    public static void Open()
    {
        GetWindow<MissingReferenceChecker>("缺失引用检查");
    }

    void OnGUI()
    {
        GUILayout.Label("扫描当前场景的所有组件，列出空引用和 Missing Script", EditorStyles.boldLabel);
        GUILayout.Space(6);

        onlyScriptsInAssets = EditorGUILayout.Toggle("只检查 Assets 里的脚本", onlyScriptsInAssets);

        GUILayout.Space(6);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("扫描当前场景", GUILayout.Height(30)))
            result = Scan();
        if (GUILayout.Button("复制结果", GUILayout.Height(30)))
            EditorGUIUtility.systemCopyBuffer = result;
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.TextArea(result, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    string Scan()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        StringBuilder sb = new StringBuilder();
        int missingScriptCount = 0;
        int emptyRefCount = 0;

        sb.AppendLine($"// 场景: {scene.name}");
        sb.AppendLine($"// 时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        foreach (var root in roots)
            ScanGameObject(root, sb, ref missingScriptCount, ref emptyRefCount);

        string header = $"// 共发现 {missingScriptCount} 个 Missing Script，{emptyRefCount} 个空引用\n\n";
        return header + sb.ToString();
    }

    void ScanGameObject(GameObject go, StringBuilder sb, ref int missingCount, ref int emptyCount)
    {
        string path = GetPath(go);

        // 检查 Missing Script
        Component[] comps = go.GetComponents<Component>();
        bool headerPrinted = false;

        foreach (var c in comps)
        {
            if (c == null)
            {
                sb.AppendLine($"❌ [Missing Script] {path}");
                missingCount++;
                continue;
            }
        }

        // 检查所有脚本组件的字段
        foreach (var c in comps)
        {
            if (c == null) continue;
            if (c is Transform || c is RectTransform) continue;

            System.Type type = c.GetType();
            if (onlyScriptsInAssets && !IsUserScript(type)) continue;

            SerializedObject so = new SerializedObject(c);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.propertyPath == "m_Script") continue;

                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (prop.objectReferenceValue == null && !IsReallyOptional(prop))
                    {
                        // 排除数组/列表本身的元素引用
                        if (prop.propertyPath.Contains(".Array.size")) continue;

                        if (!headerPrinted)
                        {
                            sb.AppendLine($"⚠️ {path}");
                            headerPrinted = true;
                        }
                        sb.AppendLine($"     [{type.Name}] {prop.displayName} 为空");
                        emptyCount++;
                    }
                }
            }
        }

        // 递归子物体
        for (int i = 0; i < go.transform.childCount; i++)
            ScanGameObject(go.transform.GetChild(i).gameObject, sb, ref missingCount, ref emptyCount);
    }

    bool IsUserScript(System.Type type)
    {
        string ns = type.Namespace ?? "";
        // 排除 Unity 自带
        if (ns.StartsWith("UnityEngine") || ns.StartsWith("UnityEditor") ||
            ns.StartsWith("TMPro") || ns.StartsWith("System"))
            return false;
        // 排除一些常见的非用户类型
        if (type.Name == "Canvas" || type.Name == "CanvasScaler" || type.Name == "GraphicRaycaster")
            return false;
        return true;
    }

    bool IsReallyOptional(SerializedProperty prop)
    {
        // 数组元素里的 null 是正常的
        if (prop.propertyPath.Contains(".Array.data[")) return true;
        return false;
    }

    string GetPath(GameObject go)
    {
        string path = go.name;
        Transform p = go.transform.parent;
        while (p != null)
        {
            path = p.name + "/" + path;
            p = p.parent;
        }
        return path;
    }
}