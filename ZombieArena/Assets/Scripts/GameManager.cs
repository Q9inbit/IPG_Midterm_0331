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

    [Header("Blood Moon Settings")]
    public float bloodMoonStatMultiplier = 1.5f;
    public float bloodMoonScaleMultiplier = 1.2f;
    public Material bloodMoonBlinkMaterial;
    public float bloodMoonBlinkSpeed = 5f;
    public GameObject redVignetteUI;

    private bool isBloodMoon = false;
    public bool IsBloodMoon => isBloodMoon;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        zombieTypes[ZombieType.Regular] = new ZombieStats(ZombieHealth[0], ZombieSpeed[0], ZombieDamage[0], new Vector3(1f, 1f, 1f));
        zombieTypes[ZombieType.Fast] = new ZombieStats(ZombieHealth[1], ZombieSpeed[1], ZombieDamage[1], new Vector3(0.7f, 1f, 0.7f));
        zombieTypes[ZombieType.Tank] = new ZombieStats(ZombieHealth[2], ZombieSpeed[2], ZombieDamage[2], new Vector3(1.5f, 1.5f, 1.5f));


    }

    private void Start()
    {
        startScreen.SetActive(true);
        gameOverScreen.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 0;
        StartCoroutine(SpawnZombiesRoutine());

        StartCoroutine(BloodMoonRoutine());

        redVignetteUI.SetActive(false);
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
        int maxAttempts = 20;
        float checkRadius = 1f;

        Camera mainCam = Camera.main;

        while (!validSpawn && attempts < maxAttempts)
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            spawnPosition = new Vector3(randomX, 0f, randomZ);

            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCam);
            Bounds bounds = new Bounds(spawnPosition, Vector3.one * 1.5f);
            bool inView = GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);

            Collider[] colliders = Physics.OverlapSphere(spawnPosition, checkRadius);
            validSpawn = !inView;
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

    private IEnumerator BloodMoonRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(8f, 25f);
            yield return new WaitForSeconds(waitTime);

            isBloodMoon = true;
            if (redVignetteUI != null)
                redVignetteUI.SetActive(true);

            yield return new WaitForSeconds(10f);

            isBloodMoon = false;
            if (redVignetteUI != null)
                redVignetteUI.SetActive(false);
        }
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
