using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowPath : MonoBehaviour
{

    public enum TypeOfMovement
    {
        MoveTowards,
        Lerp
    }

    public TypeOfMovement Type = TypeOfMovement.MoveTowards;

    public PathDefinition1 path;
    public float speed = 1;
    public float maxDistanceToGoal = 0.1f;

    private IEnumerator<Transform> _currentPoint;

    public void Start()
    {
        if (path == null)
        {
            Debug.Log("Path is null", gameObject);
            return;
        }

        _currentPoint = path.GetPathEnumerator();
        _currentPoint.MoveNext();

        if (_currentPoint.Current == null)
            return;

        transform.position = _currentPoint.Current.position;
    }

    public void Update()
    {
        if (path == null)
            return;
        
        if (_currentPoint == null || _currentPoint.Current == null)
        {
            return;
        }

        if (Type == TypeOfMovement.MoveTowards)
        {
            transform.position = Vector3.MoveTowards(transform.position, _currentPoint.Current.position, Time.deltaTime * speed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _currentPoint.Current.position, Time.deltaTime * speed);
        }

        //var camera = FindObjectOfType<Camera>();
        //if (camera)
        //{
        //    var position = camera.WorldToScreenPoint(transform.position);
        //    Debug.Log(position);
        //}

        var distanceSquared = (transform.position - _currentPoint.Current.position).sqrMagnitude;

        if (distanceSquared < maxDistanceToGoal * maxDistanceToGoal)
        {
            _currentPoint.MoveNext();
        }

    }

}
