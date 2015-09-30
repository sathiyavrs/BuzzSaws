using UnityEngine;

public class DieOnPlayerDeath : MonoBehaviour
{
    public Player Player;
    public Renderer Renderer;

    public void Start()
    {
        if (renderer != null)
            Renderer = renderer;
    }

    public void Update()
    {
        if (!Player.IsDead)
        {
            Renderer.enabled = true;
        }
        else
        {
            Renderer.enabled = false;
        }
    }
}
