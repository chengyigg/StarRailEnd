using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class AssetTreeExporter : EditorWindow
{
    private string rootFolder = "Assets";
    private Vector2 scroll;
    private string preview = "";
    private string outputPath = "";
    private bool showFiles = true;
    private bool showFoldersOnly = false;
    private bool showMetaFiles = false;
    private bool showFileSize = true;

    [MenuItem("Tools/Asset Tree Exporter")]
    public static void Open()
    {
        GetWindow<AssetTreeExporter>("资源树导出");
    }

    void OnEnable()
    {
        outputPath = Path.Combine(Application.dataPath, "..", "AssetTree.txt");
    }

    void OnGUI()
    {
        GUILayout.Label("导出 Assets 文件夹树状结构", EditorStyles.boldLabel);
        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        GUILayout.Label("根目录：", GUILayout.Width(60));
        rootFolder = EditorGUILayout.TextField(rootFolder);
        if (GUILayout.Button("选择", GUILayout.Width(50)))
        {
            string picked = EditorUtility.OpenFolderPanel("选择目录", rootFolder, "");
            if (!string.IsNullOrEmpty(picked) && picked.StartsWith(Application.dataPath))
                rootFolder = "Assets" + picked.Substring(Application.dataPath.Length);
        }
        GUILayout.EndHorizontal();

        showFiles = EditorGUILayout.Toggle("显示文件", showFiles);
        showFoldersOnly = EditorGUILayout.Toggle("只看文件夹", showFoldersOnly);
        showMetaFiles = EditorGUILayout.Toggle("显示 .meta 文件", showMetaFiles);
        showFileSize = EditorGUILayout.Toggle("显示文件大小", showFileSize);

        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        GUILayout.Label("输出文件：", GUILayout.Width(70));
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("选择", GUILayout.Width(50)))
        {
            string picked = EditorUtility.SaveFilePanel("保存到", Path.GetDirectoryName(outputPath), "AssetTree", "txt");
            if (!string.IsNullOrEmpty(picked)) outputPath = picked;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新预览", GUILayout.Height(30)))
            preview = Generate();
        if (GUILayout.Button("导出到文件", GUILayout.Height(30)))
        {
            string c = Generate();
            File.WriteAllText(outputPath, c, Encoding.UTF8);
            EditorUtility.RevealInFinder(outputPath);
        }
        if (GUILayout.Button("复制到剪贴板", GUILayout.Height(30)))
        {
            EditorGUIUtility.systemCopyBuffer = Generate();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label($"预览（{preview.Length} 字符）：", EditorStyles.boldLabel);
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.TextArea(preview, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
    }

    string Generate()
    {
        if (!Directory.Exists(rootFolder))
            return $"目录不存在: {rootFolder}";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"// ===== Asset Tree =====");
        sb.AppendLine($"// 根目录: {rootFolder}");
        sb.AppendLine($"// 时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        WalkDirectory(rootFolder, sb, "", true);
        return sb.ToString();
    }

    void WalkDirectory(string fullPath, StringBuilder sb, string indent, bool isRoot)
    {
        string[] dirs = Directory.GetDirectories(fullPath);
        string[] files = Directory.GetFiles(fullPath);

        System.Array.Sort(dirs);
        System.Array.Sort(files);

        // 先输出文件夹
        for (int i = 0; i < dirs.Length; i++)
        {
            string dirName = Path.GetFileName(dirs[i]);
            if (dirName.StartsWith(".")) continue;  // 跳过隐藏

            bool isLast = (i == dirs.Length - 1) && (files.Length == 0 || !showFiles);
            string prefix = isLast ? "└── " : "├── ";
            sb.AppendLine($"{indent}{prefix}📁 {dirName}/");

            string childIndent = indent + (isLast ? "    " : "│   ");
            WalkDirectory(dirs[i], sb, childIndent, false);
        }

        if (!showFiles || showFoldersOnly) return;

        // 输出文件
        int visibleCount = 0;
        foreach (var f in files)
        {
            string ext = Path.GetExtension(f).ToLower();
            if (!showMetaFiles && ext == ".meta") continue;
            visibleCount++;
        }

        int printed = 0;
        foreach (var f in files)
        {
            string ext = Path.GetExtension(f).ToLower();
            if (!showMetaFiles && ext == ".meta") continue;

            printed++;
            bool isLast = (printed == visibleCount);
            string prefix = isLast ? "└── " : "├── ";
            string fileName = Path.GetFileName(f);

            string icon = GetFileIcon(ext);
            string size = "";
            if (showFileSize)
            {
                long bytes = new FileInfo(f).Length;
                size = $"  ({FormatSize(bytes)})";
            }

            sb.AppendLine($"{indent}{prefix}{icon} {fileName}{size}");
        }
    }

    string GetFileIcon(string ext)
    {
        switch (ext)
        {
            case ".cs": return "📜";
            case ".asset": return "📦";
            case ".prefab": return "🧩";
            case ".unity": return "🎬";
            case ".png":
            case ".jpg":
            case ".jpeg": return "🖼️";
            case ".wav":
            case ".mp3":
            case ".ogg": return "🔊";
            case ".txt":
            case ".csv":
            case ".json": return "📄";
            case ".mat": return "🎨";
            case ".shader": return "✨";
            case ".anim": return "🎞️";
            case ".controller": return "🕹️";
            default: return "📎";
        }
    }

    string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024f:F1} KB";
        return $"{bytes / 1024f / 1024f:F2} MB";
    }
}