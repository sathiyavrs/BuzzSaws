using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public FloatingTextParameters TextParameters;

    public void Start()
    {

    }

    public void PlayerHitCheckpoint()
    {
        LevelManager.Instance.DestroyStars();
        LevelManager.Instance.DestroyEnemies();
        StartCoroutine(DisplayCheckpoint());

        FloatingText.Show("Checkpoint Reached", "CheckpointDisplayText",
            new CenteredFloatingTextPositioner(TextParameters.Speed, TextParameters.TimeToLive));
    }

    private IEnumerator DisplayCheckpoint()
    {
        LevelManager.Instance.DisplayCheckpoint();
        yield return new WaitForSeconds(LevelManager.Instance.CheckpointDisplaySeconds);

        LevelManager.Instance.RemoveDisplayCheckpoint();
        yield break;
    }

    private IEnumerator PlayerHitCheckpointCo(int bonus)
    {
        yield break;
    }

    public void PlayerLeftCheckpoint()
    {

    }

    public void SpawnPlayer(Player player)
    {
        player.RespawnAt(transform);
    }

    public void AddObectToCheckpoint()
    {

    }

}
