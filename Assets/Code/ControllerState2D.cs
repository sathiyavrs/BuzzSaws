using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class ControllerState2D
{
    public bool IsCollidingRight { get; set; }
    public bool IsCollidingLeft { get; set; }
    public bool IsCollidingAbove { get; set; }
    public bool IsCollidingBelow { get; set; }
    public bool IsMovingDownSlope { get; set; }
    public bool IsMovingUpSlope { get; set; }
    public bool IsGrounded { get { return IsCollidingBelow; } }

    public float SlopeAngle { get; set; }
    public bool HasCollisions { get { return IsCollidingRight || IsCollidingLeft || IsCollidingAbove || IsCollidingBelow; } }

    public void Reset()
    {
        IsMovingUpSlope = IsMovingDownSlope = IsCollidingLeft = IsCollidingRight = IsCollidingAbove = IsCollidingBelow = false;
        SlopeAngle = 0;
    }

}
