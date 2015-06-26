using UnityEngine;
using System.Collections;

public class JumpPlatform : MonoBehaviour
{
    public float JumpMagnitude = 20f;
    public AudioClip JumpSound;

    public void ControllerEnter2D(CharacterController2D controller)
    {
        controller.SetVerticalForce(JumpMagnitude);
        AudioSource.PlayClipAtPoint(JumpSound, transform.position);
    }
}
