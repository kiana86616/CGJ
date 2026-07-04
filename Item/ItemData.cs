using UnityEngine;
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;
    /// <summary>公开使用接口，调用子类的 useitem 实现。</summary>
    public void Use() => useitem();

    public virtual void useitem() { }
}
