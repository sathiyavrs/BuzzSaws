using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour , ITakeDamage, Living
{
    private bool _isFacingRight;
    private CharacterController2D _controller;
    private float _normalizedHorizontalSpeed;
    private int _prevHealth;
    private float _canFireIn;
    private Vector3 _mousePosition;

    public GameObject OuchEffect;
    public float MaxSpeed = 10;
    public float SpeedAccelerationOnGround = 10f;
    public float SpeedAccelerationInAir = 5f;
    public int MaxHealth = 100;
    public int MinRespawnHealth = 50;
    public FloatingTextParameters TextParameters;
    public Projectile Projectile;
    public GameObject FireProjectileEffect;
    public float FireRate;
    public Transform ProjectileFireLocation;
    public AudioClip PlayerHitSound;
    public AudioClip PlayerShootSound;
    public AudioClip PlayerHealthSound;
    public Animator PlayerAnimator;

    public bool IsDead { get; private set; }
    public int Health { get; private set; }

    public void Awake()
    {
        _canFireIn = -1f;
        _controller = GetComponent<CharacterController2D>();
        _isFacingRight = transform.localScale.x > 0;
        _prevHealth = MaxHealth;
        Health = MaxHealth;

        if (MinRespawnHealth > MaxHealth)
            MinRespawnHealth = MaxHealth;
    }

    public void Update()
    {
        if (Health > MaxHealth)
            Health = MaxHealth;

        _canFireIn -= Time.deltaTime;
        _normalizedHorizontalSpeed = 0;
        if (!IsDead)
            HandleInput();

        var movementFactor = _controller.State.IsGrounded ? SpeedAccelerationOnGround : SpeedAccelerationInAir;
        _controller.SetHorizontalForce(Mathf.Lerp(_controller.Velocity.x, MaxSpeed * _normalizedHorizontalSpeed, Time.deltaTime * movementFactor));

        PlayerAnimator.SetBool("IsGrounded", _controller.State.IsGrounded);
        PlayerAnimator.SetBool("IsDead", IsDead);
        PlayerAnimator.SetFloat("Speed", Mathf.Abs(_controller.Velocity.x / MaxSpeed));
    }

    public void Kill()
    {
        _controller.HandleCollisions = false;
        collider2D.enabled = false;
        _controller.SetHorizontalForce(0);
        _controller.SetVerticalForce(12);
        IsDead = true;
        _prevHealth = Health;
        Health = 0;
    }

    public void RespawnAt(Transform spawnPoint)
    {
        if (!_isFacingRight)
        {
            Flip();
        }

        IsDead = false;
        collider2D.enabled = true;
        _controller.HandleCollisions = true;

        transform.position = spawnPoint.position;
        Health = _prevHealth;

        if (Health < MinRespawnHealth)
            Health = MinRespawnHealth;
    }

    public void TakeDamage(int damage)
    {
        var boxCollider2D = GetComponent<BoxCollider2D>();
        FloatingText.Show(string.Format("-{0}!", damage), "DamageText", new FromWorldPointTextPositioner(
                Camera.main, transform.position + new Vector3(-boxCollider2D.size.x / 2, boxCollider2D.size.y / 2, 0),
                TextParameters.TimeToLive, TextParameters.Speed));

        if (OuchEffect != null)
            Instantiate(OuchEffect, transform.position, transform.rotation);

        Health -= damage;
        if (Health <= 0)
            LevelManager.Instance.KillPlayer();

        AudioSource.PlayClipAtPoint(PlayerHitSound, transform.position);
    }

    public void GiveHealth(int health, GameObject owner)
    {
        AudioSource.PlayClipAtPoint(PlayerHealthSound, transform.position);
        Health += health;
        if (health > MaxHealth)
            health = MaxHealth;
        var boxCollider2D = GetComponent<BoxCollider2D>();
        FloatingText.Show(string.Format("+{0}!", health), "GiveHealthText", new FromWorldPointTextPositioner(
                Camera.main, transform.position + new Vector3(-boxCollider2D.size.x / 2, boxCollider2D.size.y / 2, 0),
                TextParameters.TimeToLive, TextParameters.Speed));
    }

    public void FinishLevel()
    {
        enabled = false;
        _controller.SetForce(new Vector2(0, 0));
    }

    private void HandleInput()
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _normalizedHorizontalSpeed = 1;
            if (!_isFacingRight)
            {
                Flip();
            }
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _normalizedHorizontalSpeed = -1;
            if (_isFacingRight)
            {
                Flip();
            }
        }
        else
        {
            _normalizedHorizontalSpeed = 0;
        }

        if (_controller.CanJump && Input.GetKeyDown(KeyCode.W))
        {
            if (_controller.State.IsMovingDownSlope || _controller.State.IsMovingUpSlope)
            {
                _controller.JumpWhileMovingDown();
            }

            _controller.Jump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            FireProjectileGeneral();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            FireProjectileQuick();
        }
    }

    private void FireProjectileQuick()
    {
        if (_canFireIn > 0)
            return;

        AudioSource.PlayClipAtPoint(PlayerShootSound, transform.position);
        if (FireProjectileEffect != null)
        {
            var effect = (GameObject)Instantiate(FireProjectileEffect, ProjectileFireLocation.position, ProjectileFireLocation.rotation);
            effect.transform.parent = transform;
        }

        var velocity = _controller.Velocity;
        var direction = _isFacingRight ? Vector2.right : -Vector2.right;

        var projectile = (Projectile)Instantiate(Projectile, ProjectileFireLocation.position, new Quaternion(0,0,0,0));
        projectile.Initialize(gameObject, direction, new Vector2(0, 0));
        _canFireIn = 1 / FireRate;

        PlayerAnimator.SetTrigger("Fire");
    }

    private void FireProjectileGeneral()
    {
        if (_canFireIn > 0)
            return;
        AudioSource.PlayClipAtPoint(PlayerShootSound, transform.position);
        if (FireProjectileEffect != null)
        {
            var effect = (GameObject)Instantiate(FireProjectileEffect, ProjectileFireLocation.position, ProjectileFireLocation.rotation);
            effect.transform.parent = transform;
        }

        var velocity = _controller.Velocity;
        var direction = (_mousePosition - transform.position) / Mathf.Sqrt(Vector3.SqrMagnitude(_mousePosition - transform.position));

        if (_isFacingRight && direction.x < 0)
            Flip();
        else if (!_isFacingRight && direction.x > 0)
            Flip();

        var projectile = (Projectile)Instantiate(Projectile, ProjectileFireLocation.position, new Quaternion(0, 0, 0, 0));
        projectile.Initialize(gameObject, direction, new Vector2(0, 0));
        _canFireIn = 1 / FireRate;

        PlayerAnimator.SetTrigger("Fire");
    }

    private void Flip()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        _isFacingRight = transform.localScale.x > 0;
    }


    public void TakeDamage(int damage, GameObject instigator)
    {
        TakeDamage(damage);
    }

    public int GetHealth()
    {
        return Health;
    }

    public int GetMaxHealth()
    {
        return MaxHealth;
    }
}
