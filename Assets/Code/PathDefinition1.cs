using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathDefinition1 : MonoBehaviour
{

    public Transform[] points;

    public IEnumerator<Transform> GetPathEnumerator()
    {
        //throw new NotImplementedException();

        if (points == null || points.Length < 1)
        {
            yield break;
        }

        var direction = 1;
        var index = 0;

        while (true)
        {
            yield return points[index];

            if (points.Length == 1)
                continue;

            if (index <= 0)
                direction = 1;

            if (index >= points.Length - 1)
                direction = -1;

            index += direction;
        }
    }

    public void OnDrawGizmosSelected()
    {
        var Points = new List<Transform>();

        for (int i = 0; i < points.Length; i++)
        {
            if(points[i] != null)
            {
                Points.Add(points[i]);
            }
        }

        points = Points.ToArray();

        if (points == null || points.Length < 2)
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
