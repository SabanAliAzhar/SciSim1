using UnityEngine;

public class CollisionRelay : MonoBehaviour
{
    public GravityExperiment receiver; 

    private void OnCollisionEnter(Collision collision)
    {
       
        if (receiver != null)
        {
            receiver.HandleCollision(collision);
        }
    }
}