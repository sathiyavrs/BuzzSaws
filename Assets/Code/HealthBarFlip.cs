using UnityEngine;

public class HealthBarFlip : MonoBehaviour
{
    public Transform BadFish;

    private Vector3 _initialPosition;

    public void Start()
    {
        _initialPosition = transform.position + BadFish.transform.position;
    }

    public void Update()
    {
        var requiredValue = BadFish.localScale.x > 0 ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x);
        
        transform.localScale = new Vector3(requiredValue, transform.localScale.y, transform.localScale.z);
    }
}
