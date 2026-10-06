using UnityEngine;
using System.Collections.Generic;

public enum PortraitPosition { None, Left, Center, Right }

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 5)] public string textContent;

    [Header("头像")]
    public Sprite speakerAvatar;

    [Header("背景")]
    public Sprite background;

    [Header("立绘")]
    public Sprite speakerPortrait;
    public PortraitPosition portraitPos = PortraitPosition.Center;

    [Header("分支选项（留空则无选项）")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}