using System;
using UnityEngine;

class PathedProjectileSpawner : MonoBehaviour
{
    public Transform Destination;
    public PathedProjectile Projectile;
    public GameObject SpawnEffect;
    public AudioClip ShooterSound;
    public Animator CanonAnimator;

    public float Speed = 20;
    public float FireRate = 3;

    private float _nextShotInSeconds;

    public void Start()
    {
        _nextShotInSeconds = 1 / FireRate;
    }

    public void Update()
    {
        _nextShotInSeconds -= Time.deltaTime;
        if (_nextShotInSeconds > 0)
            return;
        _nextShotInSeconds = 1 / FireRate;

        var projectile = (PathedProjectile)Instantiate(Projectile, transform.position, transform.rotation);
        projectile.Initialize(Destination, Speed);
        AudioSource.PlayClipAtPoint(ShooterSound, transform.position);

        if (SpawnEffect != null)
            Instantiate(SpawnEffect, transform.position, transform.rotation);

        CanonAnimator.SetTrigger("Fire");
    }

    public void OnDrawGizmosSelected()
    {
        if (Destination == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, Destination.position);
    }
}
