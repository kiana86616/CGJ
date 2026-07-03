using UnityEngine;

public class player : MonoBehaviour
{
    [Header("Anchor Chain")]
    [SerializeField] private GameObject anchorChainPrefab;
    [SerializeField] private float anchorChainMaxLength = 100f;
    [SerializeField] private float mixTime = 0.15f;
    [SerializeField] private float anchorLandOffset = 0.2f;

    private PlayerStateMachine stateMachine;
    private float lastLaunchTime;

    public Rigidbody2D rb { get; private set; }
    public CapsuleCollider2D cap { get; private set; }
    public Animator anim { get; private set; }
    public float AnchorChainMaxLength => anchorChainMaxLength;
    public float MixTime => mixTime;
    public float AnchorLandOffset => anchorLandOffset;
    public AnchorChain ActiveChain { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cap = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<Animator>();

        stateMachine = new PlayerStateMachine(this);
        stateMachine.Initialize(new PlayerIdleState(stateMachine, this));
    }

    void Update() => stateMachine?.Update();
    void FixedUpdate() => stateMachine?.FixedUpdate();

    public bool CanLaunchAnchor() => Time.time >= lastLaunchTime + mixTime;

    public AnchorChain SpawnChain(Vector2 direction)
    {
        if (!CanLaunchAnchor() || anchorChainPrefab == null)
            return null;

        if (ActiveChain != null)
            Destroy(ActiveChain.gameObject);

        Vector2 origin = transform.position;
        GameObject chainObject = Instantiate(anchorChainPrefab, origin, Quaternion.identity);
        AnchorChain chain = chainObject.GetComponent<AnchorChain>();
        if (chain == null)
            chain = chainObject.AddComponent<AnchorChain>();

        chain.Launch(origin, direction, this, anchorChainMaxLength);
        ActiveChain = chain;
        lastLaunchTime = Time.time;
        return chain;
    }

    public void ClearActiveChain()
    {
        if (ActiveChain == null)
            return;

        Destroy(ActiveChain.gameObject);
        ActiveChain = null;
    }

    public void StartAnchorPull(Vector2 anchorPosition, Collider2D platformCollider)
    {
        stateMachine.ChangeState(new PlayerAnchorPullState(stateMachine, this, anchorPosition, platformCollider));
    }
}
