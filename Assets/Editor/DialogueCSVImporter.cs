using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DialogueCSVImporter : EditorWindow
{
    private TextAsset textFile;
    private string outputFolder = "Assets/Dialogues";

    [MenuItem("Tools/Dialogue CSV Importer")]
    public static void Open()
    {
        GetWindow<DialogueCSVImporter>("对话导入工具");
    }

    void OnGUI()
    {
        GUILayout.Label("格式说明：", EditorStyles.boldLabel);
        GUILayout.Label("SEQ: SeqID | NextSeqID | ChapterTitle | AffChar | AffDelta | Unlock | SetVar | SetVal");
        GUILayout.Label("CJ:  Cond | Var | Val | Target");
        GUILayout.Label("T:   Speaker | Text | Avatar | Bg | Portrait | PortraitPos");
        GUILayout.Label("CH:  Text | Target | Cond | Var | Val | AffChar | AffDelta");
        GUILayout.Label("PortraitPos 可选：None / Left / Center / Right");
        GUILayout.Label("空字段用 - 占位");
        GUILayout.Space(10);

        textFile = (TextAsset)EditorGUILayout.ObjectField("文本文件", textFile, typeof(TextAsset), false);
        outputFolder = EditorGUILayout.TextField("输出文件夹", outputFolder);

        if (GUILayout.Button("导入"))
        {
            if (textFile == null)
            {
                EditorUtility.DisplayDialog("错误", "请先拖入文本文件", "好");
                return;
            }
            Import();
        }
    }

    void Import()
    {
        string[] lines = textFile.text.Split('\n');
        DialogueSequence currentSeq = null;
        DialogueLine lastLine = null;
        Dictionary<string, DialogueSequence> sequences = new Dictionary<string, DialogueSequence>();
        int seqCount = 0;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith("//") || line.StartsWith("#")) continue;

            if (line.StartsWith("SEQ:"))
            {
                string[] parts = SplitFields(line.Substring(4));
                if (parts.Length < 8) { GameLog.Warning($"SEQ 字段不足: {line}"); continue; }

                string seqID = parts[0];
                string path = $"{outputFolder}/{SanitizeFileName(seqID)}.asset";

                DialogueSequence existing = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
                if (existing != null)
                {
                    currentSeq = existing;
                    currentSeq.lines.Clear();
                    if (currentSeq.conditionalNexts == null)
                        currentSeq.conditionalNexts = new List<ConditionalJump>();
                    currentSeq.conditionalNexts.Clear();
                }
                else
                {
                    currentSeq = ScriptableObject.CreateInstance<DialogueSequence>();
                    AssetDatabase.CreateAsset(currentSeq, path);
                }

                currentSeq.sequenceID = seqID;
                currentSeq.nextSequenceID = NormalizeField(parts[1]);
                currentSeq.chapterTitle = NormalizeField(parts[2]);
                currentSeq.affectionCharacter = NormalizeField(parts[3]);
                currentSeq.affectionDelta = ParseInt(parts[4]);
                currentSeq.unlockCharacter = NormalizeField(parts[5]);
                currentSeq.setVariableName = NormalizeField(parts[6]);
                currentSeq.setVariableValue = ParseInt(parts[7]);
                if (currentSeq.conditionalNexts == null)
                    currentSeq.conditionalNexts = new List<ConditionalJump>();

                sequences[seqID] = currentSeq;
                lastLine = null;
                seqCount++;
                continue;
            }

            if (currentSeq == null) continue;

            if (line.StartsWith("CJ:"))
            {
                string[] parts = SplitFields(line.Substring(3));
                if (parts.Length < 4) { GameLog.Warning($"CJ 字段不足: {line}"); continue; }
                ConditionalJump cj = new ConditionalJump
                {
                    condition = ParseCondition(parts[0]),
                    variableName = NormalizeField(parts[1]),
                    variableValue = ParseInt(parts[2]),
                    targetSequenceID = NormalizeField(parts[3])
                };
                currentSeq.conditionalNexts.Add(cj);
                continue;
            }

            if (line.StartsWith("T:"))
            {
                string[] parts = SplitFields(line.Substring(2));
                if (parts.Length < 4) { GameLog.Warning($"T 字段不足: {line}"); continue; }
                DialogueLine dl = new DialogueLine
                {
                    speakerName = NormalizeField(parts[0]),
                    textContent = NormalizeField(parts[1]),
                    speakerAvatar = LoadSprite(NormalizeField(parts[2])),
                    background = LoadSprite(NormalizeField(parts[3])),
                    choices = new List<DialogueChoice>()
                };
                if (parts.Length > 4)
                    dl.speakerPortrait = LoadSprite(NormalizeField(parts[4]));
                if (parts.Length > 5)
                    dl.portraitPos = ParsePortraitPos(parts[5]);

                currentSeq.lines.Add(dl);
                lastLine = dl;
                continue;
            }

            if (line.StartsWith("CH:"))
            {
                if (lastLine == null) { GameLog.Warning($"CH 没有对应台词: {line}"); continue; }
                string[] parts = SplitFields(line.Substring(3));
                if (parts.Length < 7) { GameLog.Warning($"CH 字段不足: {line}"); continue; }
                DialogueChoice choice = new DialogueChoice
                {
                    choiceText = NormalizeField(parts[0]),
                    nextSequenceID = NormalizeField(parts[1]),
                    condition = ParseCondition(parts[2]),
                    variableName = NormalizeField(parts[3]),
                    variableValue = ParseInt(parts[4]),
                    affectionCharacter = NormalizeField(parts[5]),
                    affectionDelta = ParseInt(parts[6]),
                    hideWhenUnmet = true
                };
                lastLine.choices.Add(choice);
                continue;
            }
        }

        foreach (var kv in sequences)
            EditorUtility.SetDirty(kv.Value);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GameLog.Info($"[Dialogue CSV Importer] 导入完成，共 {seqCount} 个序列");
    }

    PortraitPosition ParsePortraitPos(string s)
    {
        s = s.Trim();
        if (s == "-" || s == "") return PortraitPosition.None;
        if (System.Enum.TryParse(s, out PortraitPosition p)) return p;
        return PortraitPosition.None;
    }

    string[] SplitFields(string s)
    {
        string[] raw = s.Split('|');
        for (int i = 0; i < raw.Length; i++) raw[i] = raw[i].Trim();
        return raw;
    }

    string NormalizeField(string s)
    {
        s = s.Trim();
        if (s == "-" || s == "") return "";
        return s;
    }

    int ParseInt(string s)
    {
        s = s.Trim();
        if (int.TryParse(s, out int v)) return v;
        return 0;
    }

    ChoiceCondition ParseCondition(string s)
    {
        s = s.Trim();
        if (System.Enum.TryParse(s, out ChoiceCondition c)) return c;
        return ChoiceCondition.None;
    }

    string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    Sprite LoadSprite(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        string[] guids = AssetDatabase.FindAssets($"{name} t:Sprite");

        // 优先精确匹配文件名
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (fileName == name)
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }

        // 没有精确匹配，退化到第一个包含匹配
        if (guids.Length > 0)
        {
            GameLog.Warning($"[Importer] {name} 没有精确匹配，使用模糊匹配: {AssetDatabase.GUIDToAssetPath(guids[0])}");
            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        GameLog.Warning($"[Importer] 找不到 Sprite: {name}");
        return null;
    }
}