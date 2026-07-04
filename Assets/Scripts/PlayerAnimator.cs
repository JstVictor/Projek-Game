using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;

    private PlayerMovement movement;

    private static readonly int SpeedHash     = Animator.StringToHash("Speed");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning"); // ← tambah

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        animator.SetFloat(SpeedHash,     movement.GetSpeed(), 0.1f, Time.deltaTime);
        animator.SetBool(IsRunningHash,  movement.IsRunning()); // ← tambah
    }
}