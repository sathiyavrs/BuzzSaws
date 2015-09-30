using System;
using UnityEngine;

class PathedProjectile : MonoBehaviour, ITakeDamage
{
    public GameObject DestroyEffect;
    public int PointsToGiveToPlayer;
    public FloatingTextParameters TextParameters;
    public AudioClip DestroySound;
    
    private Transform _destination;
    private float _speed;

    public float Speed { get { return _speed; } }
    public bool isLeft
    {
        get
        {
            if (transform.position.x > _destination.position.x)
                return false;
            else
                return true;
        }
    }

    public void Initialize(Transform destination, float speed)
    {
        _destination = destination;
        _speed = speed;
    }

    public void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _destination.position, _speed * Time.deltaTime);
        var distanceSquared = (transform.position - _destination.position).sqrMagnitude;

        if (distanceSquared > 0.01f * 0.01f)
            return;

        if (DestroySound != null)
            AudioSource.PlayClipAtPoint(DestroySound, transform.position);

        DestroyObject();
    }

    public void DestroyObject()
    {
        if (DestroyEffect != null)
        {
            Instantiate(DestroyEffect, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, GameObject instigator)
    {
        DestroyObject();

        if (PointsToGiveToPlayer == 0)
            return;

        var projectile = instigator.GetComponent<Projectile>();
        if(projectile != null && projectile.Owner.GetComponent<Player>() != null)
        {
            GameManager.Instance.AddPoints(PointsToGiveToPlayer);
            if (PointsToGiveToPlayer > 0)
                FloatingText.Show(string.Format("+{0}!", PointsToGiveToPlayer), "PointStarText",
                    new FromWorldPointTextPositioner(Camera.main, transform.position, TextParameters.TimeToLive, TextParameters.Speed));
            else
                FloatingText.Show(string.Format("-{0}!", PointsToGiveToPlayer), "PointStarText",
                    new FromWorldPointTextPositioner(Camera.main, transform.position, TextParameters.TimeToLive, TextParameters.Speed));
        }
    }
}
