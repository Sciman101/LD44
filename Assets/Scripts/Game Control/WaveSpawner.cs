using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    private static readonly string WAVE_TEMPLATE = "Wave {0}\n<size=20>{1} Enemies Left</size>";
    private static readonly WaitForSeconds WAVE_SPAWN_DELAY = new WaitForSeconds(1.5f);

    public static WaveSpawner instance;

    /// <summary>
    /// Refrence to the player, to tell enemies what to target
    /// </summary>
    [SerializeField]
    private Transform player;

    /// <summary>
    /// Everything that can be spawned
    /// </summary>
    [Header("Spawning properties"),SerializeField]
    private SpawnType[] spawnables = null;
    [SerializeField]
    private Transform[] spawnPoints = null;
    [SerializeField]
    private ReadyRegion readyRegion = null;

    [Header("UI"), SerializeField]
    private TextMeshProUGUI waveLabel;
    [SerializeField]
    private Image fadeIn;

    [System.Serializable]
    public class SpawnType
    {
        /// <summary>
        /// Enemy prefab
        /// </summary>
        public GameObject prefab;
        /// <summary>
        /// How rare is the enemy (0-1)
        /// </summary>
        public float rarity;
        /// <summary>
        /// The earliest wave the enemy can appear during
        /// </summary>
        public int minWave;
    }

    /// <summary>
    /// Current wave
    /// </summary>
    private int waveNum = 0;

    /// <summary>
    /// Are we in an active wave rn?
    /// </summary>
    private bool waveActive = false;

    /// <summary>
    /// How many enemies remain for this wave?
    /// </summary>
    private int enemiesRemaining = 0;

    /// <summary>
    /// Set instance
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Initialize wave spawner
    /// </summary>
    private void Start()
    {
        fadeIn.gameObject.SetActive(true);
        fadeIn.CrossFadeAlpha(1, 0, false);
        fadeIn.CrossFadeAlpha(0, 1, false);

        readyRegion.onPlayerEnter.AddListener(StartNextWave);

        OnEndWave();
    }

    /// <summary>
    /// Called when an enemy is killed
    /// </summary>
    public void OnEnemyKilled()
    {
        if (waveActive)
        {
            enemiesRemaining--;
            if (enemiesRemaining <= 0)
            {
                OnEndWave();
            }
            else
            {
                UpdateWaveLabel();
            }
        }
    }

    public int GetEnemyCount()
    {
        return enemiesRemaining;
    }

    public int GetWaveNum()
    {
        if (waveActive)
        {
            return waveNum - 1;
        }
        else
        {
            return waveNum;
        }
    }

    public void Retry()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    /// <summary>
    /// Spawn all enemies in a wave
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnEnemies()
    {
        int enemiesToSpawn = enemiesRemaining;

        for (int i=0;i< enemiesToSpawn; i++)
        {
            SpawnEnemy();
            yield return WAVE_SPAWN_DELAY;
        }   
    }

    /// <summary>
    /// Spawn a single enemy
    /// </summary>
    void SpawnEnemy()
    {
        //Generate random value and allocate space for thing to spawn
        float chance = Random.value;
        SpawnType selected = null;
        //Loop
        foreach (SpawnType type in spawnables)
        {
            //Is the wave high enough?
            if (type.minWave <= waveNum)
            {
                if (type.rarity >= chance)
                {
                    //Make sure that this thing is the rarest of them all
                    if (selected == null || (selected != null && selected.rarity > type.rarity))
                    {
                        selected = type;
                    }
                }
            }
        }
        //Create
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        (Instantiate(selected.prefab, point.position, Quaternion.identity) as GameObject).GetComponent<EnemyController>().target = player;
    }

    /// <summary>
    /// Called when a wave is completed
    /// </summary>
    void OnEndWave()
    {
        waveLabel.text = "Ready for next wave";
        readyRegion.gameObject.SetActive(true);
        waveActive = false;
    }

    /// <summary>
    /// Start the next wave
    /// </summary>
    void StartNextWave()
    {
        //Calculate wave stats
        waveNum++;
        enemiesRemaining = waveNum * 2 + 3;

        waveActive = true;

        //Hide ready region
        readyRegion.gameObject.SetActive(false);

        UpdateWaveLabel();

        //Start spawning coroutine
        StartCoroutine(SpawnEnemies());
    }

    /// <summary>
    /// Update the wave label to reflect the current stats
    /// </summary>
    void UpdateWaveLabel()
    {
        waveLabel.text = string.Format(WAVE_TEMPLATE, waveNum, enemiesRemaining);
    }
}
