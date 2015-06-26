using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform Player;
    public Vector2 Smoothing;
    public Vector2 Margin;
    public BoxCollider2D Bounds;
    public bool IsFollowing { get; set; }

    private Vector2 _min, _max;

    public void Start()
    {
        _max = Bounds.bounds.max;
        _min = Bounds.bounds.min;
        IsFollowing = true;
    }

    public void Update()
    {
        var x = transform.position.x;
        var y = transform.position.y;

        if (IsFollowing)
        {
            if (Mathf.Abs(x - Player.position.x) > Margin.x)
            {
                x = Mathf.Lerp(x, Player.position.x, Smoothing.x * Time.deltaTime);
            }

            if (Mathf.Abs(y - Player.position.y) > Margin.y)
            {
                y = Mathf.Lerp(y, Player.position.y, Smoothing.y * Time.deltaTime);
            }

            var cameraHalfWidth = camera.orthographicSize * ((float)Screen.width / (float)Screen.height);

            x = Mathf.Clamp(x, _min.x + cameraHalfWidth, _max.x - cameraHalfWidth);
            y = Mathf.Clamp(y, _min.y + camera.orthographicSize, _max.y - camera.orthographicSize);

            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
