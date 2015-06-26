using UnityEngine;

public class GiveHealth : MonoBehaviour
{
    public GameObject Effect;
    public int HealthToGive;
    public FloatingTextParameters TextParameters;

    public void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<Player>();
        if (player == null)
            return;

        player.GiveHealth(HealthToGive, gameObject);
        if (Effect != null)
            Instantiate(Effect, transform.position, transform.rotation);

        gameObject.SetActive(false);
        LevelManager.Instance.AddHealthPack(this);
        
    }

    public void Respawn()
    {
        gameObject.SetActive(true);
    }

}
