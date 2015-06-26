using UnityEngine;

public class SimpleProjectile : Projectile, ITakeDamage
{
    public int Damage;
    public GameObject DestroyedEffect;
    public int PointsToGiveToPlayer;
    public float TimeToLive;
    public FloatingTextParameters TextParameters;
    public KnockbackModelParameters KnockbackParameters;
    public AudioClip DestroySound;

    private Vector2 _velocity;

    public void Start()
    {
        KnockbackParameters.Damage = Damage;
    }

    public void Update()
    {
        if ((TimeToLive -= Time.deltaTime) <= 0)
        {
            DestroyProjectile();
            return;
        }

        _velocity = InitialVelocity + Direction * Speed;
        transform.Translate(_velocity * Time.deltaTime, Space.World);
    }

    public void TakeDamage(int damage, GameObject instigator)
    {
        if (PointsToGiveToPlayer != 0)
        {
            var projectile = instigator.GetComponent<Projectile>();
            if (projectile != null && projectile.Owner.GetComponent<Player>() != null)
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

        DestroyProjectile();
    }

    protected override void OnInitialized()
    {
        var angle = Vector2.Angle(Direction, Vector2.right);
        if (Direction.y < 0)
            angle *= -1;

        transform.Rotate(0, 0, angle);
    }

    protected override void OnCollideOwner()
    {
        DestroyProjectile();
    }

    protected override void OnCollideOther(Collider2D other)
    {
        DestroyProjectile();
    }

    protected override void OnCollideTakeDamage(Collider2D other, ITakeDamage takeDamage)
    {
        takeDamage.TakeDamage(Damage, gameObject);

        var controller = other.gameObject.GetComponent<CharacterController2D>();
        if (controller != null)
        {
            HandleKnockback(controller);
        }

        DestroyProjectile();
    }

    private void HandleKnockback(CharacterController2D controller)
    {
        Knockback knockback = new Knockback(KnockbackParameters);
        knockback.HandleKnockback(controller, _velocity, gameObject);
    }

    private void DestroyProjectile()
    {
        if (DestroyedEffect != null)
            Instantiate(DestroyedEffect, transform.position, transform.rotation);

        if (DestroySound != null)
            AudioSource.PlayClipAtPoint(DestroySound, transform.position);

        Destroy(gameObject);
    }
}