using UnityEngine;

class CenteredFloatingTextPositioner : IFloatingTextPositioner
{
    public readonly float _speed;
    public float _textPosition;
    private float _timeToLive;

    public CenteredFloatingTextPositioner(float speed, float timeToLive)
    {
        _speed = speed;
        _timeToLive = timeToLive;
        _textPosition = 0;
    }


    public bool GetPosition(ref Vector2 position, GUIContent content, Vector2 size)
    {
        if (_timeToLive < Time.deltaTime)
            return false;

        _timeToLive -= Time.deltaTime;
        position.x = Screen.width / 2 - (size.x / 2);
        position.y = Screen.height - Screen.height / 2 - _textPosition;
        _textPosition += _speed * Time.deltaTime;
        return true;
    }
}
