using UnityEngine;

public struct ZombieStats
{
    public float health;
    public float speed;
    public float damage;
    public Vector3 scaleOverride;

    public ZombieStats(float h, float s, float d, Vector3 scale = default)
    {
        health = h;
        speed = s;
        damage = d;
        scaleOverride = scale == default ? Vector3.zero : scale;
    }
}
