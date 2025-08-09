using UnityEngine;

public class EndlessCameraFollow : MonoBehaviour
{
    public Transform target;             // Mario/player
    public float followAhead = 2.6f;     // Look-ahead on X
    public float smoothing = 5f;         // How smooth camera follows
    public float verticalSmoothing = 2f; // Y-axis smoothing

    public float minY = -2f;             // Minimum Y camera can go (to avoid showing off-screen)
    public float maxY = 5f;              // Maximum Y camera can go

    private Vector3 targetPosition;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("❌ Camera target not assigned!");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + followAhead;
        float targetY = Mathf.Clamp(target.position.y, minY, maxY);

        targetPosition = new Vector3(targetX, targetY, transform.position.z);

        Vector3 smoothedPosition = new Vector3(
            Mathf.Lerp(transform.position.x, targetPosition.x, smoothing * Time.deltaTime),
            Mathf.Lerp(transform.position.y, targetPosition.y, verticalSmoothing * Time.deltaTime),
            targetPosition.z
        );

        transform.position = smoothedPosition;
    }
}
