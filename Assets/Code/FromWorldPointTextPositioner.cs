using System;
using UnityEngine;

public class FromWorldPointTextPositioner: IFloatingTextPositioner
{
    private readonly Camera _camera;
    private readonly Vector3 _worldPosition;
    private readonly float _speed;
    private float _timeToLive;
    private float _yOffset;
    
    public FromWorldPointTextPositioner(Camera camera, Vector3 worldPosition, float timeToLive, float speed)
    {
        _camera = camera;
        _worldPosition = worldPosition;
        _timeToLive = timeToLive;
        _speed = speed;
        _yOffset = 0;
    }
   
    public bool GetPosition(ref Vector2 position, GUIContent content, Vector2 size)
    {
        if (_timeToLive < Time.deltaTime)
            return false;

        _timeToLive -= Time.deltaTime;
        var screenPosition = _camera.WorldToScreenPoint(_worldPosition);
        position.x = screenPosition.x - (size.x / 2);
        position.y = Screen.height - screenPosition.y - _yOffset;

        _yOffset += Time.deltaTime * _speed;
        return true;
    }
}
