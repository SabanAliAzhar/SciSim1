using UnityEngine;

public class FaceRig : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // Make the object look at the camera
            transform.LookAt(Camera.main.transform);

            // Rotate 180 degrees around the Y axis to face the correct way
            transform.Rotate(0f, 180f, 0f);
        }
    }
}
