using UnityEngine;

/// <summary>
/// 协调 Chain + Anchor：伸出锁链，挂住 platform 或达到最大长度后快速缩回。
/// </summary>
public class AnchorChain : MonoBehaviour
{
    private const string PlatformTag = "platform";

    [SerializeField] private float extendSpeed = 60f;
    [SerializeField] private float retractSpeed = 120f;
    [SerializeField] private float attachOffset = 0.05f;
    [SerializeField] private Chain chain;
    [SerializeField] private Anchor anchor;

    private player player;
    private Vector2 originPoint;
    private Vector2 direction;
    private float maxLength;
    private float currentLength;
    private bool isExtending;
    private bool isRetracting;

    public bool HasAttached => anchor != null && anchor.IsAttached;
    public bool IsRetracting => isRetracting;

    private void Awake()
    {
        chain ??= GetComponentInChildren<Chain>();
        anchor ??= GetComponentInChildren<Anchor>();
    }

    public void Launch(Vector2 origin, Vector2 launchDirection, player ownerPlayer, float length)
    {
        player = ownerPlayer;
        originPoint = origin;
        direction = launchDirection.sqrMagnitude > 0.0001f ? launchDirection.normalized : Vector2.right;
        maxLength = length;
        currentLength = 0f;
        isExtending = true;
        isRetracting = false;

        anchor?.ResetAnchor();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        if (player != null)
            originPoint = player.transform.position;

        if (isExtending)
            TickExtend();
        else if (isRetracting)
            TickRetract();
    }

    private void TickExtend()
    {
        float nextLength = Mathf.Min(currentLength + extendSpeed * Time.fixedDeltaTime, maxLength);

        if (TryHitPlatform(nextLength, out RaycastHit2D hit))
        {
            currentLength = Mathf.Max(hit.distance - attachOffset, 0f);
            Vector2 attachPoint = hit.point + hit.normal * attachOffset;
            isExtending = false;
            anchor?.AttachAt(attachPoint, direction);
            UpdateVisuals();
            player?.StartAnchorPull(attachPoint, hit.collider);
            return;
        }

        currentLength = nextLength;
        if (currentLength >= maxLength)
        {
            isExtending = false;
            isRetracting = true;
        }

        UpdateVisuals();
    }

    private void TickRetract()
    {
        currentLength = Mathf.Max(currentLength - retractSpeed * Time.fixedDeltaTime, 0f);
        UpdateVisuals();

        if (currentLength > 0f)
            return;

        isRetracting = false;
        player?.ClearActiveChain();
    }

    public void UpdateOrigin(Vector2 newOrigin)
    {
        originPoint = newOrigin;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        Vector2 tip = anchor != null && anchor.IsAttached
            ? anchor.Position
            : originPoint + direction * currentLength;

        chain?.SetEndpoints(originPoint, tip);

        if (anchor != null && !anchor.IsAttached)
            anchor.SetPosition(tip, direction);
    }

    private bool TryHitPlatform(float length, out RaycastHit2D platformHit)
    {
        platformHit = default;
        if (length <= 0f)
            return false;

        float closest = float.MaxValue;
        bool found = false;

        foreach (RaycastHit2D hit in Physics2D.RaycastAll(originPoint, direction, length))
        {
            if (IsOwnerCollider(hit.collider) || !hit.collider.CompareTag(PlatformTag))
                continue;

            if (hit.distance < closest)
            {
                closest = hit.distance;
                platformHit = hit;
                found = true;
            }
        }

        return found;
    }

    private bool IsOwnerCollider(Collider2D col)
    {
        if (player == null || col == null)
            return false;

        if (player.cap != null && col == player.cap)
            return true;

        return col.attachedRigidbody != null && col.attachedRigidbody == player.rb;
    }
}
