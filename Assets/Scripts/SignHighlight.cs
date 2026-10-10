using UnityEngine;

public class SignHighlight : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private float highlightDistance = 1.5f;

    private void Start()
    {
        highlightObject.SetActive(false);
    }

    private void Update()
    {
        // BreathSignPoint와 플레이어 사이의 거리 계산
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        bool isPlayerNear = distance <= highlightDistance;
        highlightObject.SetActive(isPlayerNear);
    }
}