using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class SequenceReferenceFinder : EditorWindow
{
    private Object targetAsset;
    private Vector2 scroll;
    private string result = "";

    [MenuItem("Tools/Sequence Reference Finder")]
    public static void Open()
    {
        GetWindow<SequenceReferenceFinder>("序列引用追踪");
    }

    void OnGUI()
    {
        GUILayout.Label("查找谁引用了这个资源", EditorStyles.boldLabel);
        GUILayout.Label("支持：DialogueSequence / Sprite / AudioClip / 任何资产", EditorStyles.miniLabel);
        GUILayout.Space(6);

        targetAsset = EditorGUILayout.ObjectField("目标资产", targetAsset, typeof(Object), false);

        GUILayout.Space(6);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("查找引用", GUILayout.Height(28)))
            result = FindReferences();
        if (GUILayout.Button("复制结果", GUILayout.Height(28)))
            EditorGUIUtility.systemCopyBuffer = result;
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.TextArea(result, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    string FindReferences()
    {
        if (targetAsset == null) return "请先拖入一个资产";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"// 查找对 [{targetAsset.name}] 的引用");
        sb.AppendLine();

        int total = 0;

        // 1. 搜所有 DialogueSequence 里的引用
        string[] seqGuids = AssetDatabase.FindAssets("t:DialogueSequence");
        foreach (string guid in seqGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueSequence seq = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
            if (seq == null) continue;

            List<string> hits = FindInSequence(seq, targetAsset);
            if (hits.Count > 0)
            {
                sb.AppendLine($"📄 {seq.sequenceID} ({path})");
                foreach (string h in hits) sb.AppendLine($"     {h}");
                sb.AppendLine();
                total += hits.Count;
            }
        }

        // 2. 搜所有场景里的引用
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
            foreach (var root in scene.GetRootGameObjects())
            {
                total += ScanGameObjectForReference(root, targetAsset, path, sb);
            }
            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
        }

        // 3. 搜所有 prefab
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            total += ScanGameObjectForReference(prefab, targetAsset, path, sb);
        }

        if (total == 0) return $"没有找到对 [{targetAsset.name}] 的引用";

        return $"// 共发现 {total} 处引用\n\n" + sb.ToString();
    }

    List<string> FindInSequence(DialogueSequence seq, Object target)
    {
        List<string> hits = new List<string>();

        if (target is DialogueSequence && seq.nextSequenceID == ((DialogueSequence)target).sequenceID)
            hits.Add($"[nextSequenceID] {seq.nextSequenceID}");

        for (int i = 0; i < seq.lines.Count; i++)
        {
            var line = seq.lines[i];
            if (line.speakerAvatar == target) hits.Add($"Line[{i}] Avatar: {line.textContent}");
            if (line.background == target) hits.Add($"Line[{i}] Background: {line.textContent}");
            if (line.speakerPortrait == target) hits.Add($"Line[{i}] Portrait: {line.textContent}");

            if (line.choices != null)
            {
                for (int j = 0; j < line.choices.Count; j++)
                {
                    var c = line.choices[j];
                    if (c.affectionCharacter == null) { }
                }
            }
        }

        // 条件跳转里的 target
        if (seq.conditionalNexts != null && target is DialogueSequence)
        {
            string tid = ((DialogueSequence)target).sequenceID;
            for (int i = 0; i < seq.conditionalNexts.Count; i++)
            {
                if (seq.conditionalNexts[i].targetSequenceID == tid)
                    hits.Add($"[ConditionalNext {i}] -> {tid}");
            }
        }

        // 选项跳转里的 target
        if (target is DialogueSequence)
        {
            string tid = ((DialogueSequence)target).sequenceID;
            for (int i = 0; i < seq.lines.Count; i++)
            {
                var line = seq.lines[i];
                if (line.choices == null) continue;
                for (int j = 0; j < line.choices.Count; j++)
                {
                    if (line.choices[j].nextSequenceID == tid)
                        hits.Add($"[Line {i} Choice {j}] -> {tid}");
                }
            }
        }

        return hits;
    }

    int ScanGameObjectForReference(GameObject go, Object target, string path, StringBuilder sb)
    {
        int count = 0;
        Component[] comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            SerializedObject so = new SerializedObject(c);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (prop.objectReferenceValue != target) continue;

                count++;
                sb.AppendLine($"🎬 {GetPath(go)}");
                sb.AppendLine($"     [{c.GetType().Name}] {prop.displayName}");
                sb.AppendLine();
            }
        }

        for (int i = 0; i < go.transform.childCount; i++)
            count += ScanGameObjectForReference(go.transform.GetChild(i).gameObject, target, path, sb);

        return count;
    }

    string GetPath(GameObject go)
    {
        string p = go.name;
        Transform t = go.transform.parent;
        while (t != null) { p = t.name + "/" + p; t = t.parent; }
        return p;
    }
}