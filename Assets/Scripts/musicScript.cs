using UnityEngine;

public class musicScript : MonoBehaviour
{
    private float speed = 60.0f;

 
    void Start()
    {
        
    }

 
    void Update()
    {
        transform.Rotate(0, speed*Time.deltaTime, 0);
    }
}
