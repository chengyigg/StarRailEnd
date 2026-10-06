using UnityEngine;
using System.Collections.Generic;

public static class AffectionManager
{
    private const string PREFIX = "Affection_";
    private const string UNLOCK_PREFIX = "Unlock_";
    private const string TRIGGERED_PREFIX = "AffTriggered_";
    private const int MAX = 100;

    private static CharacterData[] allCharacters;
    private static Dictionary<string, CharacterData> characterCache;

    public static event System.Action<string, int, int> OnAffectionChanged;

    public static int MaxValue => MAX;

    // ================== 角色数据 ==================
    public static CharacterData[] GetAllCharacters()
    {
        if (allCharacters == null || allCharacters.Length == 0)
        {
            allCharacters = Resources.LoadAll<CharacterData>("Characters");
            RebuildCache();
        }
        return allCharacters;
    }

    public static CharacterData GetCharacterData(string characterName)
    {
        if (string.IsNullOrEmpty(characterName)) return null;
        if (characterCache == null) GetAllCharacters();
        characterCache.TryGetValue(characterName, out var data);
        return data;
    }

    static void RebuildCache()
    {
        characterCache = new Dictionary<string, CharacterData>();
        if (allCharacters == null) return;
        foreach (var c in allCharacters)
        {
            if (c == null || string.IsNullOrEmpty(c.characterName)) continue;
            characterCache[c.characterName] = c;
        }
    }

    // ================== 好感度 ==================
    public static int Get(string character)
    {
        if (string.IsNullOrEmpty(character)) return 0;
        var data = GetCharacterData(character);
        int initial = data != null ? data.initialAffection : 0;
        return PlayerPrefs.GetInt(PREFIX + character, initial);
    }

    public static void Set(string character, int value)
    {
        if (string.IsNullOrEmpty(character)) return;
        value = Mathf.Clamp(value, 0, MAX);
        int oldValue = Get(character);
        PlayerPrefs.SetInt(PREFIX + character, value);

    

        if (AudioManager.Instance != null)
        {
            if (value > oldValue) AudioManager.Instance.PlaySFX("affection_up");
            else if (value < oldValue) AudioManager.Instance.PlaySFX("affection_down");
        }

        if (oldValue != value)
            OnAffectionChanged?.Invoke(character, oldValue, value);
    }

    public static void Add(string character, int delta)
    {
        if (string.IsNullOrEmpty(character) || delta == 0) return;
        Set(character, Get(character) + delta);
    }

    // ================== 解锁 ==================
    public static bool IsUnlocked(string character)
    {
        if (string.IsNullOrEmpty(character)) return false;
        var data = GetCharacterData(character);
        bool defaultUnlock = data != null && data.unlockedByDefault;
        if (PlayerPrefs.GetInt(UNLOCK_PREFIX + character, defaultUnlock ? 1 : 0) == 1) return true;
        return defaultUnlock;
    }

    public static void Unlock(string character)
    {
        if (string.IsNullOrEmpty(character)) return;
        PlayerPrefs.SetInt(UNLOCK_PREFIX + character, 1);
        GameLog.Info($"[解锁] 角色: {character}");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("unlock_character");
    }

    public static List<CharacterData> GetUnlockedCharacters()
    {
        List<CharacterData> list = new List<CharacterData>();
        foreach (var c in GetAllCharacters())
            if (c != null && IsUnlocked(c.characterName)) list.Add(c);
        return list;
    }

    // ================== 零好感触发 ==================
    public static bool IsZeroTriggered(string character)
    {
        return PlayerPrefs.GetInt(TRIGGERED_PREFIX + character + "_zero", 0) == 1;
    }

    public static void MarkZeroTriggered(string character)
    {
        PlayerPrefs.SetInt(TRIGGERED_PREFIX + character + "_zero", 1);
    }

    // ================== 重置 ==================
    public static void ResetAll()
    {
        foreach (var c in GetAllCharacters())
        {
            if (c == null) continue;
            PlayerPrefs.DeleteKey(PREFIX + c.characterName);
            PlayerPrefs.DeleteKey(UNLOCK_PREFIX + c.characterName);
            PlayerPrefs.DeleteKey(TRIGGERED_PREFIX + c.characterName + "_zero");
        }
        GameLog.Info("[好感度] 已全部重置");
    }

    public static void LogAll()
    {
        foreach (var c in GetAllCharacters())
            if (c != null) GameLog.Info($"[好感度] {c.characterName}: {Get(c.characterName)}, 已解锁: {IsUnlocked(c.characterName)}");
    }

    // ================== 存档接口 ==================
    public static void FillSaveData(SaveData data)
    {
        data.affectionKeys.Clear();
        data.affectionValues.Clear();
        data.unlockedCharacters.Clear();
        data.triggeredZeroChars.Clear();

        foreach (var c in GetAllCharacters())
        {
            if (c == null) continue;
            data.affectionKeys.Add(c.characterName);
            data.affectionValues.Add(Get(c.characterName));
            if (IsUnlocked(c.characterName))
                data.unlockedCharacters.Add(c.characterName);
            if (IsZeroTriggered(c.characterName))
                data.triggeredZeroChars.Add(c.characterName);
        }
    }

    public static void LoadFromSaveData(SaveData data)
    {
        if (data.affectionKeys == null || data.affectionValues == null) return;
        for (int i = 0; i < data.affectionKeys.Count && i < data.affectionValues.Count; i++)
            PlayerPrefs.SetInt(PREFIX + data.affectionKeys[i], data.affectionValues[i]);

        if (data.unlockedCharacters != null)
        {
            foreach (var c in GetAllCharacters())
            {
                if (c == null) continue;
                PlayerPrefs.SetInt(UNLOCK_PREFIX + c.characterName,
                    data.unlockedCharacters.Contains(c.characterName) ? 1 : 0);
            }
        }

        if (data.triggeredZeroChars != null)
        {
            foreach (var c in GetAllCharacters())
            {
                if (c == null) continue;
                PlayerPrefs.SetInt(TRIGGERED_PREFIX + c.characterName + "_zero",
                    data.triggeredZeroChars.Contains(c.characterName) ? 1 : 0);
            }
        }

        GameLog.Info("[好感度] 已加载存档数据");
    }
}