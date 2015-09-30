using UnityEngine;
using System.Collections;

public class GiveDamageToPlayer : MonoBehaviour
{
    
    public KnockbackModelParameters Parameters;

    private Vector2
        _lastPosition,
        _velocity;

    public void Awake()
    {
        _lastPosition = transform.position;
    }

    public void LateUpdate()
    {
        if (_lastPosition != null)
            _velocity = ((Vector2)transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<Player>();
        if (player == null)
            return;
        player.TakeDamage(Parameters.Damage);

        var controller = player.GetComponent<CharacterController2D>();
        var projectile = GetComponent<PathedProjectile>();

        if (_velocity.sqrMagnitude == 0)
            StartCoroutine(RectifyVelocity(controller, projectile));
        else
            HandleKnockback(controller);
    }

    private void HandleKnockback(CharacterController2D controller)
    {
        var knockback = new Knockback(Parameters);
        knockback.HandleKnockback(controller, _velocity, gameObject);

        var projectile = GetComponent<PathedProjectile>();
        if (projectile != null)
            projectile.DestroyObject();
    }

    private IEnumerator RectifyVelocity(CharacterController2D controller, PathedProjectile projectile)
    {
        yield return new WaitForEndOfFrame();

        if (_velocity.sqrMagnitude == 0)
            Parameters.Model = Knockback.KnockBackModel.Constant;

        HandleKnockback(controller);
    }
}