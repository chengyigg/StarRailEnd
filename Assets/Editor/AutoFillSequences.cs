using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AutoFillSequences : EditorWindow
{
    [MenuItem("Tools/Auto Fill All Sequences")]
    public static void Open()
    {
        GetWindow<AutoFillSequences>("自动填充序列");
    }

    void OnGUI()
    {
        GUILayout.Label("自动扫描 Assets/Dialogues/ 下所有 DialogueSequence");
        GUILayout.Label("填入当前场景的 DialogueManager.allSequences");
        GUILayout.Space(10);

        if (GUILayout.Button("一键填充"))
        {
            Fill();
        }

        GUILayout.Space(10);
        GUILayout.Label("如果同时选中场景里的 DialogueManager，效果最好。", EditorStyles.wordWrappedLabel);
    }

    void Fill()
    {
        DialogueManager dm = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<DialogueManager>()
            : null;

        if (dm == null)
            dm = FindObjectOfType<DialogueManager>();

        if (dm == null)
        {
            EditorUtility.DisplayDialog("错误", "场景里找不到 DialogueManager", "好");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:DialogueSequence");
        List<DialogueSequence> list = new List<DialogueSequence>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/Dialogues/")) continue;
            DialogueSequence seq = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
            if (seq != null) list.Add(seq);
        }

        list.Sort((a, b) => string.Compare(a.sequenceID, b.sequenceID));

        Undo.RecordObject(dm, "Auto Fill Sequences");
        dm.allSequences = list;
        EditorUtility.SetDirty(dm);

        GameLog.Info($"[AutoFill] 已填充 {list.Count} 个序列到 DialogueManager");
    }
}