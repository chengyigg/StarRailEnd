using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string sceneName;
    public string sequenceID;
    public int lineIndex;
    public string saveTime;
    public string chapterName;
    public string playerName;
    public string screenshotFile;   // 截图文件名，例如 "slot_0.png"，为空表示没截图
    // 好感度：用两个 list 存字典（JsonUtility 不支持 Dictionary）
    public List<string> affectionKeys = new List<string>();
    public List<int> affectionValues = new List<int>();

    // 已解锁的角色
    public List<string> unlockedCharacters = new List<string>();

    // 好感度归零剧情是否已触发
    public List<string> triggeredZeroChars = new List<string>();
}