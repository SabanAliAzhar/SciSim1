using UnityEngine;

public class BallController : MonoBehaviour
{
    private AudioSource audiosource;
    public AudioClip audioclip;
  
    void Start()
    {
        audiosource = GetComponent<AudioSource>();
        
    }

  
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        PlaySound();
    }
    public void PlaySound()
    {
        audiosource.PlayOneShot(audioclip);
    }
}
