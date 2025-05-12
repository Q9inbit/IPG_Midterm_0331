using UnityEngine;

public struct ZombieStats
{
    public float health;
    public float speed;
    public float damage;

    public ZombieStats(float health, float speed, float damage)
    {
        this.health = health;
        this.speed = speed;
        this.damage = damage;
    }
}
