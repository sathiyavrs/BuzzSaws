using System;
using UnityEngine;

public class Knockback
{
    public enum KnockBackModel
    {
        Constant,
        Variable,
        MoveDirection,
        Positional
    }

    public KnockbackModelParameters Parameters;

    public Knockback(KnockbackModelParameters parameters)
    {
        Parameters = parameters;
    }

    public void HandleKnockback(CharacterController2D controller, Vector2 velocity, GameObject owner)
    {
        if (Parameters.Model == Knockback.KnockBackModel.Constant)
        {
            var direction = -(controller.Velocity - velocity) / Mathf.Sqrt(Vector2.SqrMagnitude(controller.Velocity - velocity));
            var magnitude = Parameters.Damage * Parameters.RepulsiveFactor;
            controller.SetForce(magnitude * direction);
        }
        else if (Parameters.Model == Knockback.KnockBackModel.Variable)
        {
            var vector = 2 * velocity - controller.Velocity; // -v(player wrt enemy) will be the velocity of player wrt enemy.
            vector *= Parameters.RepulsiveFactor * Parameters.Damage;

            controller.SetForce(vector);
        }
        else if (Parameters.Model == Knockback.KnockBackModel.MoveDirection)
        {
            if (velocity.sqrMagnitude == 0)
                return;
            var direction = velocity / Mathf.Sqrt(Vector2.SqrMagnitude(velocity));
            var magnitude = Parameters.Damage * Parameters.RepulsiveFactor;

            if (Parameters.AddForce)
                controller.AddForce(magnitude * direction);
            else
                controller.SetForce(magnitude * direction);

        }
        else if (Parameters.Model == Knockback.KnockBackModel.Positional)
        {
            var playerPosition = controller.transform.position;
            var enemyPosition = owner.transform.position;

            var direction = (playerPosition - enemyPosition) / Mathf.Sqrt(Vector3.SqrMagnitude(playerPosition - enemyPosition));
            var magnitude = Parameters.Damage * Parameters.RepulsiveFactor;

            controller.SetForce(magnitude * direction);
        }

    }
}
