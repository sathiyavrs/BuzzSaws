using System;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    public Transform[] Backgrounds;
    public float ParallaxScale;
    public float ParallaxReductionFactor;
    public float Smoothing;

    private Vector2 _lastPosition;

    public void Start()
    {
        _lastPosition = transform.position;
    }

    public void Update()
    {
        var parallax = (transform.position.x - _lastPosition.x) * ParallaxScale;
        
        for(var i = 0; i < Backgrounds.Length; i++)
        {
            var backgroundTargetPosition = Backgrounds[i].position.x + parallax * (i * ParallaxReductionFactor);
            var toMove = new Vector3(backgroundTargetPosition, Backgrounds[i].position.y, Backgrounds[i].position.z);

            Backgrounds[i].position = Vector3.Lerp(Backgrounds[i].position, toMove, Smoothing * Time.deltaTime);
        }

        _lastPosition = transform.position;
    }
}
