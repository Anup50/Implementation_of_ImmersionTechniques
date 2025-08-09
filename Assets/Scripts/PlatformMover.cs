using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    void Update()
    {
        if (ScrollController.Instance != null && ScrollController.Instance.isScrolling && ScrollController.Instance.isEndlessRunnerMode)
        {
            float speed = ScrollController.Instance.scrollSpeed;
            transform.position += Vector3.left * speed * Time.deltaTime;
        }
    }
}
