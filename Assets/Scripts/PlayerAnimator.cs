using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;

    private PlayerMovement movement;

    private static readonly int SpeedHash     = Animator.StringToHash("Speed");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        float speed = movement.GetSpeed();

        // Paksa 0 kalau sudah sangat pelan — hilangkan delay berhenti
        if (speed < 1f) speed = 0f;

        animator.SetFloat(SpeedHash, speed, 0.05f, Time.deltaTime);
        animator.SetBool(IsRunningHash, movement.IsRunning());
    }
}