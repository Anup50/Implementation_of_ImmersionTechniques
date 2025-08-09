using UnityEngine;
using UnityEngine.UI;

public class UIProgressBarController : MonoBehaviour
{
    public Transform player;
    public Transform levelStart;
    public Transform levelEnd;
    public Image progressFill;

    private float totalDistance;

    void Start()
    {
        totalDistance = Vector2.Distance(levelStart.position, levelEnd.position);
    }

    void Update()
    {
        Debug.Log("Progress: " + progressFill.fillAmount);

        float currentDistance = Vector2.Distance(player.position, levelStart.position);
        float progress = Mathf.Clamp01(currentDistance / totalDistance);

        progressFill.fillAmount = progress;
    }
}
