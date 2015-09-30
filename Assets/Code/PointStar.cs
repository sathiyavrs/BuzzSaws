using UnityEngine;

public class PointStar : MonoBehaviour, IPlayerRespawnListener
{
    public GameObject Effect; // This will be a prefab that will be instantiated when the Star has been collected.
    public int PointsToAdd = 10;
    public FloatingTextParameters TextParameters;
    public AudioClip StarSound;
    public Animator StarAnimator;
    public GameObject Star;

    private bool _hasBeenCollected;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasBeenCollected)
            return;

        if (other.GetComponent<Player>() == null)
            return;

        GameManager.Instance.AddPoints(PointsToAdd);
        if (Effect != null)
            Instantiate(Effect, transform.position, transform.rotation);

        if (StarSound != null)
            AudioSource.PlayClipAtPoint(StarSound, transform.position);

        SetInactive();

        LevelManager.Instance.AddStar(this);

        FloatingText.Show(string.Format("+{0}!", PointsToAdd), "PointStarText",
            new FromWorldPointTextPositioner(Camera.main, transform.position, TextParameters.TimeToLive, TextParameters.Speed));
    }

    private void SetInactive()
    {
        StarAnimator.SetTrigger("Collect");
        _hasBeenCollected = true;
    }

    public void ResetAnimationEvent()
    {
        StarAnimator.SetTrigger("Reset");
        gameObject.SetActive(false);
    }

    public void OnPlayerRespawnInThisCheckpoint(Checkpoint checkpoint, Player player)
    {
        gameObject.SetActive(true);
    }

    public void Respawn()
    {
        Star.transform.position = transform.position;
        _hasBeenCollected = false;

        var renderer = Star.GetComponent<SpriteRenderer>();
        renderer.color = new Color(1, 1, 1, 1);

        gameObject.SetActive(true);
    }

    public void DestroyStar()
    {
        Destroy(gameObject);
    }
}