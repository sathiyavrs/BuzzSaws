using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Player Player;
    public SimpleEnemyAI Enemy;
    public Transform ForegroundSprite;
    public SpriteRenderer ForegroundRenderer;
    public Color MaxHealthColor = new Color(255 / 255f, 63 / 255f, 63 / 255f);
    public Color MinHealthColor = new Color(64 / 255f, 137 / 255f, 255 / 255f);

    public void Update()
    {
        if (Player != null)
        {
            var healthPercent = Player.GetHealth() / (float)Player.GetMaxHealth();
            ForegroundSprite.localScale = new Vector3(healthPercent, 1, 1);
            ForegroundRenderer.color = Color.Lerp(MinHealthColor, MaxHealthColor, healthPercent);

        }
        else if(Enemy != null)
        {
            var healthPercent = Enemy.GetHealth() / (float)Enemy.GetMaxHealth();
            ForegroundSprite.localScale = new Vector3(healthPercent, 1, 1);
            ForegroundRenderer.color = Color.Lerp(MinHealthColor, MaxHealthColor, healthPercent);
        }
    }
}