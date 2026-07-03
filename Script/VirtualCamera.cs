using Cinemachine;
using UnityEngine;

public class CameraOffsetByHeight : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    public float baseOffsetY = 2f;
    public float extraOffsetPerHeight = 0.1f;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // 根据玩家Y坐标增加偏移量
        float offsetY = baseOffsetY + player.position.y * extraOffsetPerHeight;
        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            Vector3 offset = transposer.m_FollowOffset;
            offset.y = offsetY;
            transposer.m_FollowOffset = offset;
        }
    }
}