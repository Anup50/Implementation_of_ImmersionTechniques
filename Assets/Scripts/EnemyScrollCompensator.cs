using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyScrollCompensator : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (ScrollController.Instance != null &&
            ScrollController.Instance.isEndlessRunnerMode)
        {
            rb.position += Vector2.left * ScrollController.Instance.scrollSpeed * Time.fixedDeltaTime;
        }
    }
}
