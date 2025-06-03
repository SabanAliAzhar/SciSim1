using UnityEngine;

public class TestScript : MonoBehaviour
{

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Football has collided with the ground.");
        }

        Debug.Log("Object is " + collision.gameObject.name);
    }
}
