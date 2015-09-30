using UnityEngine;

public class FinishLevel : MonoBehaviour
{
    public string LevelName;

    public void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<Player>();
        
        if (player == null)
            return;

        player.PlayerAnimator.SetFloat("Speed", 0f);
        player.PlayerAnimator.SetBool("IsGrounded", true);
        LevelManager.Instance.GoToNextLevel(LevelName);
    }
}
