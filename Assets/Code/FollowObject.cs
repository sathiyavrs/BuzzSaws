using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour
{
    private Vector2 _offset;
    public Transform Following;
    [Range(0, 1)]
    public float Smoothing;
    public FollowPath.TypeOfMovement Type = FollowPath.TypeOfMovement.Lerp;

    public void Start()
    {
        _offset = (Vector2)(transform.position - Following.position);
    }

    public void Update()
    {
        if (Type == FollowPath.TypeOfMovement.Lerp)
        {
            var vectorTo = Following.position + (Vector3)_offset;
            transform.position = Vector3.Lerp(transform.position, vectorTo, Smoothing);
        }
        else
        {
            transform.position = Following.position + (Vector3)_offset;
        }
    }
}
