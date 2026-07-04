using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ItemData", menuName = "Item/塑料袋", order = 0)]
public class PlasticBag : ItemData
{
    public override void useitem()
    {
        PlayerManager.Instance.player.GetComponent<Rigidbody2D>().gravityScale = 0.5f;
    }
}
