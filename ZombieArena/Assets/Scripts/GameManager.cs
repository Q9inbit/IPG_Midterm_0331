using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject startScreen;
    public GameObject gameOverScreen;
    public GameObject pauseMenu;

    [HideInInspector]
    public bool isPaused = false;

    public enum ZombieType { Regular, Fast, Tank }
    private Dictionary<ZombieType, ZombieStats> zombieTypes = new Dictionary<ZombieType, ZombieStats>();

    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private float spawnInterval = 1f;

    [SerializeField] private float minX = -200f;
    [SerializeField] private float maxX = 200f;
    [SerializeField] private float minZ = -200f;
    [SerializeField] private float maxZ = 200f;

    [SerializeField] private float[] ZombieHealth = new float[] { 6f, 3f, 15f };
    [SerializeField] private float[] ZombieSpeed = new float[] { 10f, 15f, 5f };
    [SerializeField] private float[] ZombieDamage = new float[] { 1f, 1f, 1f };

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        zombieTypes[ZombieType.Regular] = new ZombieStats(ZombieHealth[0], ZombieSpeed[0], ZombieDamage[0]);
        zombieTypes[ZombieType.Fast] = new ZombieStats(ZombieHealth[1], ZombieSpeed[1], ZombieDamage[1]);
        zombieTypes[ZombieType.Tank] = new ZombieStats(ZombieHealth[2], ZombieSpeed[2], ZombieDamage[2]);
    }

    private void Start()
    {
        startScreen.SetActive(true);
        gameOverScreen.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 0;
        StartCoroutine(SpawnZombiesRoutine());
    }


    private IEnumerator SpawnZombiesRoutine()
    {
        while (true)
        {
            SpawnZombie();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnZombie()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool validSpawn = false;
        int attempts = 0;
        int maxAttempts = 10;
        float checkRadius = 1f;

        while (!validSpawn && attempts < maxAttempts)
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            spawnPosition = new Vector3(randomX, 0f, randomZ);

            Collider[] colliders = Physics.OverlapSphere(spawnPosition, checkRadius);
            validSpawn = true;
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Environment"))
                {
                    validSpawn = false;
                    break;
                }
            }

            attempts++;
        }

        if (validSpawn)
        {
            GameObject newZombieObj = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);

            ZombieAIController zombieAI = newZombieObj.GetComponent<ZombieAIController>();

            int randomIndex = Random.Range(0, System.Enum.GetValues(typeof(ZombieType)).Length);
            ZombieType randomType = (ZombieType)randomIndex;

            zombieAI.zombieType = randomType;
        }
    }


    public ZombieStats GetZombieStats(ZombieType type)
    {
        return zombieTypes[type];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
        isPaused = (Time.timeScale == 0);
    }

    public void StartGame()
    {
        startScreen.SetActive(false);
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
}
