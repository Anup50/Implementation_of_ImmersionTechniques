using UnityEngine;

public class ScrollController : MonoBehaviour
{
    public static ScrollController Instance;

    public bool isEndlessRunnerMode = false;

    public float scrollSpeed = 5f;
    public float speedIncreaseRate = 0.2f;
    public float maxSpeed = 12f;
    public bool isScrolling = true;

    [Tooltip("Delay in seconds before scrolling starts")]
    public float startDelay = 2.0f;
    public float startTime;  // Made public so other scripts can check it
    public bool hasStartedMoving = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        startTime = Time.time;
    }

    // Public method to reset the start time (useful when modifying delay after creation)
    public void ResetStartTime()
    {
        startTime = Time.time;
        hasStartedMoving = false;
        Debug.Log($"[ScrollController] Start time reset. Movement will begin in {startDelay} seconds.");
    }

    void Update()
    {
        // Don't move until the delay has passed
        if (!hasStartedMoving && Time.time < startTime + startDelay)
        {
            return;
        }

        // Log when scrolling starts for debugging
        if (!hasStartedMoving)
        {
            hasStartedMoving = true;
            Debug.Log("[ScrollController] Starting movement after delay");
        }

        if (isScrolling && isEndlessRunnerMode)
        {
            scrollSpeed += speedIncreaseRate * Time.deltaTime;
            scrollSpeed = Mathf.Clamp(scrollSpeed, 0f, maxSpeed);
        }
    }
}
