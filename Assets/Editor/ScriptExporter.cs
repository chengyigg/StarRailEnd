using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class ScriptExporter : EditorWindow
{
    private string scanFolder = "Assets";
    private string outputPath = "";
    private bool includePath = true;
    private Vector2 scroll;
    private string preview = "";

    [MenuItem("Tools/Script Exporter")]
    public static void Open()
    {
        GetWindow<ScriptExporter>("脚本导出工具");
    }

    void OnEnable()
    {
        // 默认输出到项目根目录
        outputPath = Path.Combine(Application.dataPath, "..", "AllScripts.txt");
    }

    void OnGUI()
    {
        GUILayout.Label("扫描并导出项目内所有 C# 脚本", EditorStyles.boldLabel);
        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        GUILayout.Label("扫描目录：", GUILayout.Width(70));
        scanFolder = EditorGUILayout.TextField(scanFolder);
        if (GUILayout.Button("选择", GUILayout.Width(50)))
        {
            string picked = EditorUtility.OpenFolderPanel("选择目录", scanFolder, "");
            if (!string.IsNullOrEmpty(picked))
            {
                // 转成相对 Assets 的路径
                if (picked.StartsWith(Application.dataPath))
                    scanFolder = "Assets" + picked.Substring(Application.dataPath.Length);
                else
                    scanFolder = picked;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("输出文件：", GUILayout.Width(70));
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("选择", GUILayout.Width(50)))
        {
            string picked = EditorUtility.SaveFilePanel("保存到", Path.GetDirectoryName(outputPath), "AllScripts", "txt");
            if (!string.IsNullOrEmpty(picked)) outputPath = picked;
        }
        GUILayout.EndHorizontal();

        includePath = EditorGUILayout.Toggle("包含文件路径注释", includePath);

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("扫描并预览", GUILayout.Height(30)))
        {
            preview = Scan();
        }

        if (GUILayout.Button("导出到文件", GUILayout.Height(30)))
        {
            string content = Scan();
            if (!string.IsNullOrEmpty(content))
            {
                File.WriteAllText(outputPath, content, Encoding.UTF8);
                GameLog.Info($"[ScriptExporter] 已导出到: {outputPath}");
                EditorUtility.RevealInFinder(outputPath);
            }
        }

        if (GUILayout.Button("复制到剪贴板", GUILayout.Height(30)))
        {
            string content = Scan();
            if (!string.IsNullOrEmpty(content))
            {
                EditorGUIUtility.systemCopyBuffer = content;
                GameLog.Info($"[ScriptExporter] 已复制到剪贴板，共 {content.Length} 字符");
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label($"预览（{preview.Length} 字符）：", EditorStyles.boldLabel);

        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.TextArea(preview, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    string Scan()
    {
        if (!Directory.Exists(scanFolder))
        {
            GameLog.Error($"[ScriptExporter] 目录不存在: {scanFolder}");
            return "";
        }

        string[] files = Directory.GetFiles(scanFolder, "*.cs", SearchOption.AllDirectories);

        // 排除 Editor 文件夹和临时文件
        List<string> valid = new List<string>();
        foreach (var f in files)
        {
            string p = f.Replace('\\', '/');
            if (p.Contains("/Library/") || p.Contains("/Temp/") || p.Contains("/obj/")) continue;
            valid.Add(p);
        }
        valid.Sort();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"// ===== Script Export =====");
        sb.AppendLine($"// 生成时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"// 扫描目录: {scanFolder}");
        sb.AppendLine($"// 脚本数量: {valid.Count}");
        sb.AppendLine();

        int index = 1;
        foreach (string f in valid)
        {
            if (includePath)
            {
                sb.AppendLine();
                sb.AppendLine($"// ============================================================");
                sb.AppendLine($"// [{index}/{valid.Count}] {f}");
                sb.AppendLine($"// ============================================================");
            }

            try
            {
                string code = File.ReadAllText(f, Encoding.UTF8);
                sb.AppendLine(code);
            }
            catch (System.Exception e)
            {
                sb.AppendLine($"// 读取失败: {e.Message}");
            }

            sb.AppendLine();
            index++;
        }

        GameLog.Info($"[ScriptExporter] 扫描完成，共 {valid.Count} 个脚本，{sb.Length} 字符");
        return sb.ToString();
    }
}