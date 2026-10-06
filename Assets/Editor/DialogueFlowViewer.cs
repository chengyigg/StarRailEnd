using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

public class DialogueFlowViewer : EditorWindow
{
    private Vector2 scroll;
    private string result = "";
    private bool showDetails = true;

    [MenuItem("Tools/Dialogue Flow Viewer")]
    public static void Open()
    {
        GetWindow<DialogueFlowViewer>("对话流程可视化");
    }

    void OnGUI()
    {
        GUILayout.Label("读取所有 DialogueSequence 的跳转关系", EditorStyles.boldLabel);
        GUILayout.Space(6);

        showDetails = EditorGUILayout.Toggle("显示详细连接", showDetails);

        GUILayout.Space(6);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("生成流程图", GUILayout.Height(30)))
            result = Generate();
        if (GUILayout.Button("复制结果", GUILayout.Height(30)))
            EditorGUIUtility.systemCopyBuffer = result;
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.TextArea(result, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    string Generate()
    {
        string[] guids = AssetDatabase.FindAssets("t:DialogueSequence");
        List<DialogueSequence> all = new List<DialogueSequence>();
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (!path.StartsWith("Assets/Dialogues/")) continue;
            var s = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
            if (s != null) all.Add(s);
        }

        all.Sort((a, b) => string.Compare(a.sequenceID, b.sequenceID));

        HashSet<string> allIDs = new HashSet<string>();
        foreach (var s in all) allIDs.Add(s.sequenceID);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"// 共 {all.Count} 个序列");
        sb.AppendLine();

        // 收集所有跳转
        Dictionary<string, List<string>> edges = new Dictionary<string, List<string>>();
        HashSet<string> allTargets = new HashSet<string>();

        foreach (var s in all)
        {
            edges[s.sequenceID] = new List<string>();

            if (!string.IsNullOrEmpty(s.nextSequenceID))
            {
                edges[s.sequenceID].Add(s.nextSequenceID);
                allTargets.Add(s.nextSequenceID);
            }

            if (s.conditionalNexts != null)
            {
                foreach (var cj in s.conditionalNexts)
                {
                    if (!string.IsNullOrEmpty(cj.targetSequenceID))
                    {
                        edges[s.sequenceID].Add(cj.targetSequenceID);
                        allTargets.Add(cj.targetSequenceID);
                    }
                }
            }

            if (s.lines != null)
            {
                foreach (var line in s.lines)
                {
                    if (line.choices == null) continue;
                    foreach (var c in line.choices)
                    {
                        if (!string.IsNullOrEmpty(c.nextSequenceID))
                        {
                            edges[s.sequenceID].Add(c.nextSequenceID);
                            allTargets.Add(c.nextSequenceID);
                        }
                    }
                }
            }
        }

        // 1. 每个序列的跳转明细
        if (showDetails)
        {
            sb.AppendLine("=== 跳转明细 ===");
            foreach (var s in all)
            {
                sb.AppendLine($"📄 {s.sequenceID}");
                if (edges[s.sequenceID].Count == 0)
                {
                    sb.AppendLine("     (无跳转，可能是结局或死胡同)");
                }
                else
                {
                    foreach (var t in edges[s.sequenceID])
                    {
                        string mark = allIDs.Contains(t) ? "" : " ❓[目标不存在]";
                        sb.AppendLine($"     → {t}{mark}");
                    }
                }
                sb.AppendLine();
            }
        }

        // 2. 问题检测
        sb.AppendLine("=== 问题检测 ===");

        // 死胡同
        sb.AppendLine();
        sb.AppendLine("🚫 死胡同（无 nextSequenceID、无 conditionalNext、无选项跳转，且被用作目标）:");
        bool anyDead = false;
        foreach (var s in all)
        {
            bool noOut = edges[s.sequenceID].Count == 0;
            bool referenced = allTargets.Contains(s.sequenceID);
            if (noOut && referenced)
            {
                sb.AppendLine($"     - {s.sequenceID}");
                anyDead = true;
            }
        }
        if (!anyDead) sb.AppendLine("     (无)");

        // 断链
        sb.AppendLine();
        sb.AppendLine("❓ 断链（跳转目标不存在）:");
        bool anyBroken = false;
        foreach (var kv in edges)
        {
            foreach (var t in kv.Value)
            {
                if (!allIDs.Contains(t))
                {
                    sb.AppendLine($"     - {kv.Key} → {t}");
                    anyBroken = true;
                }
            }
        }
        if (!anyBroken) sb.AppendLine("     (无)");

        // 孤立序列（无人引用）
        sb.AppendLine();
        sb.AppendLine("🔵 孤立序列（没有任何序列指向它，可能是初始序列）:");
        foreach (var s in all)
        {
            if (!allTargets.Contains(s.sequenceID))
                sb.AppendLine($"     - {s.sequenceID}");
        }

        return sb.ToString();
    }
}