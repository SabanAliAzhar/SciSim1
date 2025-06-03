using UnityEngine;

public class ProjectileCollisionDetector : MonoBehaviour
{
    public ProjectileLauncher launcher; // Will be set by the launcher
    private bool hasLanded = false;

    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        if (collision.gameObject.CompareTag("Ground")) // Check for the "Ground" tag
        {
            if (launcher != null)
            {
                hasLanded = true;
                launcher.HandleProjectileLanded(transform.position);
            }
        }
    }
}