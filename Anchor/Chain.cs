using UnityEngine;

/// <summary>
/// 锁链视觉：从 origin 画到 tip，链尾贴在角色端。
/// 要求 Sprite Pivot = Bottom，Draw Mode = Sliced。
/// </summary>
public class Chain : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer ??= GetComponent<SpriteRenderer>();
    }

    public void SetEndpoints(Vector2 origin, Vector2 tip)
    {
        float length = Vector2.Distance(origin, tip);
        if (length < 0.01f)
            length = 0.01f;

        Vector2 dir = (tip - origin) / length;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);

        if (spriteRenderer == null)
            return;

        if (spriteRenderer.drawMode == SpriteDrawMode.Sliced || spriteRenderer.drawMode == SpriteDrawMode.Tiled)
            spriteRenderer.size = new Vector2(spriteRenderer.size.x, length);
        else
            transform.localScale = new Vector3(1f, length, 1f);

        // 根据 Sprite Pivot 对齐链尾到 origin（推荐 Pivot = Bottom）
        Sprite sprite = spriteRenderer.sprite;
        if (sprite != null)
        {
            float pivotRatioY = sprite.pivot.y / sprite.rect.height;
            transform.position = origin + dir * (length * pivotRatioY);
        }
        else
        {
            transform.position = origin;
        }
    }
}
