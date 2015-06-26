using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class PathDefinition : MonoBehaviour
{

    public Transform[] points;

    public IEnumerator<Transform> GetPathEnumerator()
    {
        throw new NotImplementedException();

    }

    public void OnDrawGizmos()
    {
        if (points.Length < 2)
            return;

        for (var i = 1; i < points.Length; i++)
        {
            Gizmos.DrawLine(points[i - 1].position, points[i].position);
        }
    }
}





//// Use this for initialization
//void Start () {

//}

//// Update is called once per frame
//void Update () {

//}
