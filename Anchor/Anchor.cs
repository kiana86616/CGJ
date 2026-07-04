using UnityEngine;

/// <summary>
/// 锚头：沿锁链方向移动，碰到 platform 后停止。
/// </summary>
public class Anchor : MonoBehaviour
{
    public bool IsAttached { get; private set; }
    public Vector2 Position => transform.position;

    public void ResetAnchor()
    {
        IsAttached = false;
    }

    public void SetPosition(Vector2 position, Vector2 direction)
    {
        if (IsAttached)
            return;

        transform.position = position;
        ApplyRotation(direction);
    }

    public void AttachAt(Vector2 position, Vector2 direction)
    {
        IsAttached = true;
        transform.position = position;
        ApplyRotation(direction);
    }

    private void ApplyRotation(Vector2 direction)
    {
        transform.rotation = Quaternion.Euler(
            0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
    }
}
