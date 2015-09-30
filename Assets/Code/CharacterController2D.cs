using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{

    private const float SkinWidth = 0.2f;
    private const int TotalHorizontalRays = 8;
    private const int TotalVerticalRays = 4;
    private static readonly float SlopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);

    public LayerMask PlatformMask;
    public ControllerParameters2D DefaultParameters;
    public Vector2 Velocity
    {
        get
        {
            return _velocity;
        }
    }

    public bool CanJump
    {
        get
        {
            if (Parameters.JumpRestrictions == ControllerParameters2D.JumpBehaviour.CanJumpAnywhere)
            {
                return _jumpIn < 0;
            }
            if (Parameters.JumpRestrictions == ControllerParameters2D.JumpBehaviour.CanJumpOnGround)
            {
                // Note that isGrounded is not always false as we've now implemented the MoveVertically() method.
                return State.IsGrounded;
            }
            return false;
        }
    }
    public ControllerState2D State { get; private set; }
    public bool HandleCollisions { get; set; }
    public ControllerParameters2D Parameters
    {
        get
        {
            if (_overrideParameters == null)
            {
                return DefaultParameters;
            }
            else
            {
                return _overrideParameters;
            }
        }
    }
    public GameObject StandingOn { get; private set; }
    public Vector3 PlatformVelocity { get; private set; }

    private Vector2 _velocity;
    private Transform _transform;
    private BoxCollider2D _boxCollider;
    private Vector3 _localScale;
    private float _verticalDistanceBetweenRays, _horizontalDistanceBetweenRays;
    private ControllerParameters2D _overrideParameters;

    private Vector3 _raycastTopLeft;
    private Vector3 _raycastBottomRight;
    private Vector3 _raycastBottomLeft;

    private float _jumpIn;

    private Vector3 _activeLocalTransformPoint;
    private Vector3 _activeGlobalTransformPoint;
    private GameObject _lastStandingOn;

    //private float _deltaYMovingDown;

    public void Awake()
    {
        State = new ControllerState2D();
        _transform = transform;
        _localScale = transform.localScale;
        _boxCollider = GetComponent<BoxCollider2D>();

        var colliderWidth = _boxCollider.size.x * Mathf.Abs(_localScale.x) - 2 * SkinWidth;
        _horizontalDistanceBetweenRays = colliderWidth / (TotalVerticalRays - 1);

        var colliderHeight = _boxCollider.size.y * Mathf.Abs(_localScale.y) - 2 * SkinWidth;
        _verticalDistanceBetweenRays = colliderHeight / (TotalHorizontalRays - 1);

        HandleCollisions = true;
    }

    public void AddForce(Vector2 Force)
    {
        _velocity += Force;
    }

    public void SetForce(Vector2 Force)
    {
        _velocity = Force;
    }

    public void SetHorizontalForce(float x)
    {
        _velocity.x = x;
    }

    public void SetVerticalForce(float y)
    {
        _velocity.y = y;
    }

    public void Jump()
    {
        AddForce(new Vector2(0, Parameters.JumpMagnitude));
        _jumpIn = Parameters.JumpFrequency;

        if (StandingOn == null)
            return;

        if (!StandingOn.GetComponent<Rigidbody2D>())
            return;

        AddForce(new Vector2(PlatformVelocity.x, 0));

        if (PlatformVelocity.y > 0)
            AddForce(new Vector2(0, PlatformVelocity.y));
    }

    public void JumpWhileMovingDown()
    {
        AddForce(new Vector2(0, -_velocity.y));

        if (State.IsMovingDownSlope)
        {
            State.IsMovingDownSlope = false;

        }
        else if (State.IsMovingUpSlope)
        {
            State.IsMovingUpSlope = false;
        }
    }

    public void LateUpdate()
    {
        _jumpIn -= Time.deltaTime;
        // To calculate whether its safe to jump now.

        _velocity.y += Parameters.Gravity * Time.deltaTime;
        // Parameters.Gravity is negative by default


        Move(_velocity * Time.deltaTime);
    }

    private void Move(Vector2 deltaMovement)
    {
        var wasGrounded = State.IsGrounded;
        State.Reset();

        if (HandleCollisions)
        {
            HandlePlatforms();
            CalculateRayOrigins();

            if (deltaMovement.y < 0 && wasGrounded)
            {
                HandleVerticalSlope(ref deltaMovement);
            }

            // Handles horizontal Movement checking for collisions.
            if (Mathf.Abs(deltaMovement.x) > 0.001f)
            {
                MoveHorizontally(ref deltaMovement);
            }

            // Always has to check for Vertical Movement as it is always present. (Gravity)
            var isGoingUp = deltaMovement.y > 0;
            MoveVertically(ref deltaMovement, isGoingUp);
            CorrectVerticalPlacementMovingPlatforms(ref deltaMovement, isGoingUp);

            CorrectHorizontalPlacement(ref deltaMovement, true);
            CorrectHorizontalPlacement(ref deltaMovement, false);


        }

        _transform.Translate(deltaMovement, Space.World);

        if (Time.deltaTime > 0)
        {
            _velocity = deltaMovement / Time.deltaTime;
        }

        _velocity.x = Mathf.Min(_velocity.x, Parameters.MaxVelocity.x);
        _velocity.y = Mathf.Min(_velocity.y, Parameters.MaxVelocity.y);

        if (State.IsMovingUpSlope)
        {
            _velocity.y = 0;

        }

        // TODO: Additional Moving Platforms code
        if (StandingOn != null)
        {
            _activeGlobalTransformPoint = _transform.position;
            _activeLocalTransformPoint = StandingOn.transform.InverseTransformPoint(_transform.position);

            if (_lastStandingOn != StandingOn)
            {
                if (_lastStandingOn != null)
                {
                    _lastStandingOn.SendMessage("ControllerExit2D", this, SendMessageOptions.DontRequireReceiver);
                    StandingOn.SendMessage("ControllerEnter2D", this, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    StandingOn.SendMessage("ControllerEnter2D", this, SendMessageOptions.DontRequireReceiver);
                }
                _lastStandingOn = StandingOn;
            }
            else
            {
                StandingOn.SendMessage("ControllerStay2D", this, SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            if (_lastStandingOn != null)
            {
                _lastStandingOn.SendMessage("ControllerExit2D", this, SendMessageOptions.DontRequireReceiver);
                _lastStandingOn = null;
            }
        }

    }

    private void HandlePlatforms()
    {
        // Remember that StandingOn is in the previous frame, within the MoveVertically Method.
        if (StandingOn != null)
        {
            var newGlobalTransformPoint = StandingOn.transform.TransformPoint(_activeLocalTransformPoint);
            var moveDistance = newGlobalTransformPoint - _activeGlobalTransformPoint;

            if (moveDistance != Vector3.zero)
            {
                _transform.Translate(moveDistance, Space.World);
            }

            PlatformVelocity = (newGlobalTransformPoint - _activeGlobalTransformPoint) / Time.deltaTime;
        }
        else
        {
            PlatformVelocity = Vector3.zero;
        }

        StandingOn = null;
    }

    private void CorrectHorizontalPlacement(ref Vector2 deltaMovement, bool isRight)
    {
        var halfSize = _boxCollider.size.x * _localScale.x / 2f;
        var rayOrigin = isRight ? _raycastBottomRight : _raycastBottomLeft;

        if (isRight)
        {
            rayOrigin.x += (SkinWidth - halfSize);
        }
        else
        {
            rayOrigin.x += (halfSize - SkinWidth);
        }

        var rayDirection = isRight ? Vector2.right : -Vector2.right;
        var offset = 0f;

        for (var i = 1; i < TotalHorizontalRays - 1; i++)
        {
            var rayVector = new Vector2(rayOrigin.x + deltaMovement.x,
                rayOrigin.y + deltaMovement.y + (i * _verticalDistanceBetweenRays));

            var raycastHit = Physics2D.Raycast(rayVector, rayDirection, halfSize - 0.01f, PlatformMask);
            if (!raycastHit)
                continue;

            offset = isRight ? -(halfSize - (raycastHit.point.x - _transform.position.x)) :
                (halfSize - (_transform.position.x - raycastHit.point.x));

        }

        deltaMovement.x += offset;
    }

    private void CorrectVerticalPlacementMovingPlatforms(ref Vector2 deltaMovement, bool isUp)
    {
        if (!State.IsCollidingBelow && !State.IsCollidingAbove)
        {
            var halfSize = _boxCollider.size.y * _localScale.y * 0.5f;
            var rayOrigins = _raycastTopLeft;
            rayOrigins.y += SkinWidth;
            rayOrigins.y -= halfSize;
            var offset = 0f;
            if (isUp)
            {
                var rayDirection = -Vector2.up;

                for (var i = 1; i < TotalVerticalRays - 1; i++)
                {
                    var rayVector = new Vector2
                        (rayOrigins.x + deltaMovement.x + (i * _horizontalDistanceBetweenRays), rayOrigins.y + deltaMovement.y);

                    // Debug.DrawRay(rayVector, rayDirection * halfSize, Color.magenta);

                    var raycastHit = Physics2D.Raycast(rayVector, rayDirection, halfSize - 0.01f, PlatformMask);
                    if (!raycastHit)
                        continue;
                    //Debug.Log("" + offset + " " + deltaMovement.y + " ");

                    offset = rayVector.y - raycastHit.point.y;
                    StandingOn = raycastHit.collider.gameObject;
                    State.IsCollidingBelow = true;
                }

            }
            else
            {
                var rayDirection = Vector2.up;

                for (var i = 1; i < TotalVerticalRays - 1; i++)
                {
                    var rayVector = new Vector2
                        (rayOrigins.x + deltaMovement.x + (i * _horizontalDistanceBetweenRays), rayOrigins.y + deltaMovement.y);

                    //Debug.DrawRay(rayVector, rayDirection * halfSize, Color.magenta);

                    var raycastHit = Physics2D.Raycast(rayVector, rayDirection, halfSize - .01f, PlatformMask);
                    if (!raycastHit)
                        continue;

                    //Debug.Log("BEEP BOOP");

                    offset = rayVector.y + halfSize - raycastHit.point.y;
                    State.IsCollidingAbove = true;
                }
                _transform.Translate(new Vector3(0, -offset, 0));
                //State.IsCollidingAbove = true;
            }
        }

    }

    private void CalculateRayOrigins()
    {
        var size = new Vector2(_boxCollider.size.x * Mathf.Abs(_localScale.x), _boxCollider.size.y * Mathf.Abs(_localScale.y)) / 2;
        var center = new Vector2(_boxCollider.center.x * _localScale.x, _boxCollider.center.y * _localScale.y);

        // Position Vectors for topLeft, BottomRight, and BottomLeft, taking the SkinWidth into consideration.
        _raycastTopLeft = _transform.position + new Vector3(center.x - size.x + SkinWidth, center.y + size.y - SkinWidth, 0);
        _raycastBottomRight = _transform.position + new Vector3(center.x + size.x - SkinWidth, center.y - size.y + SkinWidth, 0);
        _raycastBottomLeft = _transform.position + new Vector3(center.x - size.x + SkinWidth, center.y - size.y + SkinWidth, 0);
    }

    private void MoveHorizontally(ref Vector2 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + SkinWidth;
        var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;

        var rayOrigins = isGoingRight ? _raycastBottomRight : _raycastBottomLeft;

        for (var i = 0; i < TotalHorizontalRays; i++)
        {
            var rayVector = new Vector2(rayOrigins.x, rayOrigins.y + (i * _verticalDistanceBetweenRays));
            // rayVectors get the position vectors of the origins of the Rays which are about to be rayCasted.
            // We do not consider the distance the actual casted ray has to travel here.

            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.yellow);
            var rayCastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);
            // rayDirection is a unit Vector. rayDistance is a scalar.
            if (!rayCastHit)
            {
                continue;
            }

            // HandleHorizontalSlope is called if we move up a slope. Or atleast, we try moving up a slope.
            if (i == 0 && HandleHorizontalSlope(ref deltaMovement, Vector2.Angle(Vector2.up, rayCastHit.normal), isGoingRight))
            {
                break;
            }

            deltaMovement.x = rayCastHit.point.x - rayVector.x;
            rayDistance = Mathf.Abs(deltaMovement.x);

            // The point of the code below is that we need to lessen the magnitude of the x component of DeltaMovement. 
            // By any means necessary.
            if (isGoingRight)
            {
                deltaMovement.x -= SkinWidth;
                State.IsCollidingRight = true;
            }
            else
            {
                deltaMovement.x += SkinWidth;
                State.IsCollidingLeft = true;
            }

            // The below code accounts for collisions that have happened, where deltaMovement is restricted to below 0.001f
            // Meaning that Collision has been fully established.
            // Meaning that there's no more need to continue with this code.
            // Comment it out to see the difference.
            // Performance boost mainly.
            if (rayDistance < SkinWidth + 0.001f)
            {
                break;
            }
        }
    }

    private void MoveVertically(ref Vector2 deltaMovement)
    {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + SkinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var rayOrigins = isGoingUp ? _raycastTopLeft : _raycastBottomLeft;

        rayOrigins.x += deltaMovement.x;
        // The reason we're doing this is because we call this method after MoveVertically, but we don't call CalculateRayOrigins() again.

        var standingOnDistance = float.MaxValue;

        for (var i = 0; i < TotalVerticalRays; i++)
        {
            // Pretty much a copy of Horizontal Movement. 
            // Note that Gravity is assumed to be taken into account previous to the invocation of this method.
            var rayVector = new Vector2(rayOrigins.x + i * _horizontalDistanceBetweenRays, rayOrigins.y);
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);
            if (!raycastHit)
                continue;

            if (!isGoingUp)
            {
                var verticalDistancetoHit = transform.position.y - raycastHit.point.y;
                if (verticalDistancetoHit < standingOnDistance)
                {
                    standingOnDistance = verticalDistancetoHit;
                    StandingOn = raycastHit.collider.gameObject;
                    // to keep track of what we're standing on.
                }
            }

            deltaMovement.y = raycastHit.point.y - rayVector.y;
            rayDistance = Mathf.Abs(deltaMovement.y);

            if (isGoingUp)
            {
                deltaMovement.y -= SkinWidth;
                State.IsCollidingAbove = true;
            }
            else
            {
                deltaMovement.y += SkinWidth;
                State.IsCollidingBelow = true;
            }

            // Slopes stuff. Talk later.
            if (!isGoingUp && deltaMovement.y > 0.0001f)
            {
                State.IsMovingUpSlope = true;
            }

            if (rayDistance < SkinWidth + 0.001f)
                break;
        }

    }

    private void MoveVertically(ref Vector2 deltaMovement, bool isGoingUp)
    {
        var rayDistance = Mathf.Abs(deltaMovement.y) + SkinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var rayOrigins = isGoingUp ? _raycastTopLeft : _raycastBottomLeft;

        rayOrigins.x += deltaMovement.x;
        // The reason we're doing this is because we call this method after MoveVertically, but we don't call CalculateRayOrigins() again.

        var standingOnDistance = float.MaxValue;

        for (var i = 0; i < TotalVerticalRays; i++)
        {
            // Pretty much a copy of Horizontal Movement. 
            // Note that Gravity is assumed to be taken into account previous to the invocation of this method.
            var rayVector = new Vector2(rayOrigins.x + i * _horizontalDistanceBetweenRays, rayOrigins.y);
            Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);

            var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);
            if (!raycastHit)
                continue;

            if (!isGoingUp)
            {
                var verticalDistancetoHit = transform.position.y - raycastHit.point.y;
                if (verticalDistancetoHit < standingOnDistance)
                {
                    standingOnDistance = verticalDistancetoHit;
                    StandingOn = raycastHit.collider.gameObject;
                    // to keep track of what we're standing on.
                }
            }

            deltaMovement.y = raycastHit.point.y - rayVector.y;
            rayDistance = Mathf.Abs(deltaMovement.y);

            if (isGoingUp)
            {
                deltaMovement.y -= SkinWidth;
                State.IsCollidingAbove = true;
            }
            else
            {
                deltaMovement.y += SkinWidth;
                State.IsCollidingBelow = true;
            }

            // Slopes stuff. Talk later.
            if (!isGoingUp && deltaMovement.y > 0.0001f)
            {
                State.IsMovingUpSlope = true;
            }

            if (rayDistance < SkinWidth + 0.001f)
                break;
        }

    }

    private void HandleVerticalSlope(ref Vector2 deltaMovement)
    {
        // Called when the player is moving down.
        var center = (_raycastBottomLeft.x + _raycastBottomRight.x) / 2;
        var direction = -Vector2.up;

        var slopeDistance = SlopeLimitTangent * (_raycastBottomRight.x - center);
        // SlopeLimitTangent = Tan(75 degrees). Hence, SlopeDistance is a vertical distance that's drawn from the right Triangle consisting
        // of the center and the bottom right point of the Collider as the base.
        var slopeRayVector = new Vector2(center, _raycastBottomRight.y);

        var raycastHit = Physics2D.Raycast(slopeRayVector, direction, slopeDistance, PlatformMask);

        if (!raycastHit)
            return;

        var isGoingDownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);

        if (!isGoingDownSlope)
            return;

        var angle = Vector2.Angle(raycastHit.normal, Vector2.up);
        if (Mathf.Abs(angle) < 0.001f)
        {
            return;
        }

        State.IsMovingDownSlope = true;
        State.SlopeAngle = angle;
        deltaMovement.y = raycastHit.point.y - slopeRayVector.y;

        Debug.DrawRay(slopeRayVector, direction * slopeDistance, Color.yellow);
        // THe above ray will just illustrate how far the collision detection will go and for how much of an angle will the player actually slikde.
        // Any angle above that will force the player to fall.

        //_deltaYMovingDown = deltaMovement.y;


    }

    private bool HandleHorizontalSlope(ref Vector2 deltaMovement, float Angle, bool IsGoingRight)
    {
        // Called when the player wants to move up a slope.
        // This hence first checks whether that situation is valid.
        if (Mathf.RoundToInt(Angle) == 90)
        {
            return false;
        }

        // Angle should not be greater than the SlopeLimit.
        if (Angle > Parameters.SlopeLimit)
        {
            deltaMovement.x = 0;
            return true;
        }

        // If We're actually moving up before we even arrive here.
        if (deltaMovement.y > 0.07f)
        {
            return true;
        }

        // NO CONSERVATION OF MOMENTUM BEING ESTABLISHED HERE. THE Y SPEED IS ALL EXTRA.

        // Debug.Log("BEEP " + Mathf.Abs(Mathf.Cos(Angle * Mathf.Deg2Rad)));

        // deltaMovement.x += IsGoingRight ? -SkinWidth : SkinWidth;
        // I Commented out the above line cause I couldn't understand why this was being implemented.
        // The code in MoveHorizontally method does not happen the way I look at it.

        // I need to model deltaMovement such that going up steeper slopes takes more effort and consequently lower speed
        deltaMovement.x *= Mathf.Abs(Mathf.Cos((Angle - 20) * Mathf.Deg2Rad));

        deltaMovement.y = Mathf.Abs(Mathf.Tan(Angle * Mathf.Deg2Rad) * deltaMovement.x);
        State.IsMovingUpSlope = true;
        State.IsCollidingBelow = true;
        return true;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        var volume = other.gameObject.GetComponent<ControllerPhysicsVolumes>();
        if (volume == null)
            return;

        _overrideParameters = volume.Parameters;
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        var volume = other.gameObject.GetComponent<ControllerPhysicsVolumes>();
        if (volume == null)
            return;

        _overrideParameters = null;
    }

}
