using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家管理器 —— 管理玩家引用、物品栏和玩家状态。
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("玩家引用")]
    public player player;

    [Header("物品栏")]
    [SerializeField] private List<ItemData> inventory = new List<ItemData>();
    [SerializeField] private int maxInventorySize = 10;

    // 玩家状态
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (player == null)
        {
            player = FindObjectOfType<player>();
        }
    }

    /// <summary>
    /// 添加物品到物品栏。
    // /// </summary>
    // public bool AddItem(ItemData item)
    // {
    //     if (inventory.Count >= maxInventorySize)
    //     {
    //         Debug.LogWarning("[PlayerManager] 物品栏已满！");
    //         return false;
    //     }

    //     inventory.Add(item);
    //     Debug.Log($"[PlayerManager] 获得物品: {item.itemName}");
    //     return true;
    // }

    // /// <summary>
    // /// 移除物品。
    // /// </summary>
    // public void RemoveItem(ItemData item)
    // {
    //     if (inventory.Remove(item))
    //     {
    //         Debug.Log($"[PlayerManager] 移除物品: {item.itemName}");
    //     }
    // }

    // /// <summary>
    // /// 使用指定索引的物品。
    // /// </summary>
    // public bool UseItem(int index)
    // {
    //     if (index < 0 || index >= inventory.Count) return false;

    //     ItemData item = inventory[index];
    //     item.Use();
    //     return true;
    // }

    // /// <summary>
    // /// 检查是否拥有某个物品。
    // /// </summary>
    // public bool HasItem(ItemData item)
    // {
    //     return inventory.Contains(item);
    // }

    // /// <summary>
    // /// 获取物品栏列表（只读）。
    // /// </summary>
    // public IReadOnlyList<ItemData> GetInventory() => inventory;
}
