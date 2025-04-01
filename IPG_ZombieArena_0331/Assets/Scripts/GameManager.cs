using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum ZombieType { Regular, Fast, Tank }
    private Dictionary<ZombieType, ZombieStats> zombieTypes = new Dictionary<ZombieType, ZombieStats>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        zombieTypes[ZombieType.Regular] = new ZombieStats(100f, 3.5f, 10f);
        zombieTypes[ZombieType.Fast] = new ZombieStats(60f, 5.5f, 5f);
        zombieTypes[ZombieType.Tank] = new ZombieStats(200f, 2.0f, 20f);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
