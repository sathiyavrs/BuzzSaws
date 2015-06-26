using UnityEngine;

public class SimpleEnemyAI : MonoBehaviour, ITakeDamage, Living
{
    public float Speed;
    public float FireRate;
    public Projectile Ammo;
    public GameObject DestroyEffect;
    public GameObject HitEffect;
    public float LOSDistance;
    public LayerMask Layers;
    public Transform ProjectileSpawn;
    public int MaxHealth = 100;
    public AudioClip ShootSound;

    [Range(0, 1)]
    public float Smoothing;

    public int Health { get; private set; }

    private CharacterController2D _controller;
    private Vector2 _direction;
    private Vector2 _startPosition; // for respawn.
    private float _canFireIn;

    public void Start()
    {
        _controller = GetComponent<CharacterController2D>();
        _direction = new Vector2(-1, 0);
        _startPosition = transform.position;
        _canFireIn = 0;

        Health = MaxHealth;
        _controller.SetHorizontalForce(_direction.x * Speed);
    }

    public void Update()
    {
        _controller.SetHorizontalForce(Mathf.Lerp(_controller.Velocity.x, _direction.x * Speed, Smoothing));
        if ((_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight))
        {
            _direction = -_direction;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        } // Flip

        if ((_canFireIn -= Time.deltaTime) > 0)
            return; // Cannot fire tup tup tup.

        var raycastHit = Physics2D.Raycast(ProjectileSpawn.position, _direction, LOSDistance, Layers);
        if (!raycastHit)
            return; // No one in front of him

        if (raycastHit.collider.gameObject.GetComponent<Player>() == null)
            return;

        var projectile = (Projectile)Instantiate(Ammo, ProjectileSpawn.position, ProjectileSpawn.rotation);
        projectile.Initialize(gameObject, _direction, new Vector2(0, 0));
        AudioSource.PlayClipAtPoint(ShootSound, transform.position);
        _canFireIn = 1 / FireRate;
    }

    public void TakeDamage(int damage, GameObject instigator)
    {
        if (HitEffect != null)
            Instantiate(HitEffect, transform.position, transform.rotation);

        Health -= damage;

        if (Health <= 0)
        {
            gameObject.SetActive(false);
            LevelManager.Instance.AddEnemy(this);
            if (DestroyEffect != null)
                Instantiate(DestroyEffect, transform.position, transform.rotation);
        }


    }

    public int GetHealth()
    {
        return Health;
    }

    public int GetMaxHealth()
    {
        return MaxHealth;
    }

    public void ResetHeath()
    {
        Health = MaxHealth;
    }

    public void Respawn()
    {
        ResetHeath();
        gameObject.SetActive(true);
    }
}
