using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    public float scrollMultiplier = 1.0f;
    private bool hasLoggedStatus = false;
    private Vector3 lastPosition;
    private float lastMoveCheck = 0f;

    void Start()
    {
        // Store initial position for movement tracking
        lastPosition = transform.position;
        Debug.Log($"[MoveLeft] Started on {gameObject.name}, initial position: {lastPosition}");
    }

    void Update()
    {
        // First check if we have a ScrollController
        if (ScrollController.Instance == null)
        {
            // If we haven't logged this error already
            if (!hasLoggedStatus)
            {
                Debug.LogError($"[MoveLeft] ScrollController.Instance is NULL on {gameObject.name}!");
                hasLoggedStatus = true;
            }
            return;
        }

        // Check if we should be moving
        if (ScrollController.Instance.isEndlessRunnerMode &&
            ScrollController.Instance.isScrolling &&
            ScrollController.Instance.hasStartedMoving)
        {
            // Calculate movement this frame
            float speed = ScrollController.Instance.scrollSpeed * scrollMultiplier;
            transform.position += Vector3.left * speed * Time.deltaTime;

            // Log first successful movement
            if (!hasLoggedStatus)
            {
                Debug.Log($"[MoveLeft] {gameObject.name} is moving at speed {speed}");
                hasLoggedStatus = true;
            }

            // Periodically check if we're actually moving
            if (Time.time > lastMoveCheck + 1.0f)
            {
                lastMoveCheck = Time.time;
                float distance = Vector3.Distance(transform.position, lastPosition);
                if (distance < 0.01f)
                {
                    Debug.LogWarning($"[MoveLeft] {gameObject.name} doesn't appear to be moving! Speed: {speed}");
                    // Force movement to ensure something happens
                    transform.position += Vector3.left * 0.1f;
                }
                lastPosition = transform.position;
            }
        }
        else if (Time.frameCount % 300 == gameObject.GetInstanceID() % 300) // Staggered logs
        {
            // Debug why we're not moving
            string reason = "";
            if (!ScrollController.Instance.isEndlessRunnerMode) reason = "endlessRunnerMode is false";
            else if (!ScrollController.Instance.isScrolling) reason = "scrolling is false";
            else if (!ScrollController.Instance.hasStartedMoving) reason = "hasStartedMoving is false";

            Debug.Log($"[MoveLeft] {gameObject.name} not moving because: {reason}. Time: {Time.time}");
        }
    }
}
