using System;
using UnityEngine;

[Serializable]
public class KnockbackModelParameters
{
    public Knockback.KnockBackModel Model;
    public float RepulsiveFactor;
    public int Damage;
    public bool AddForce;
}
