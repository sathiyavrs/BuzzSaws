using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public Player Player { get; private set; }
    public CameraController Camera { get; private set; }
    public TimeSpan RunningTime { get { return (DateTime.UtcNow - _started); } }
    public bool IsAtLastCheckpoint { get; private set; }

    public int CurrentTimeBonus
    {
        get
        {
            var timeDifference = BonusCutoffSeconds - (int)RunningTime.TotalSeconds;
            return Mathf.Max(0, timeDifference) * BonusSecondMultiplier;
        }
    }

    //public List<PointStar> Stars { get { return _stars; } }

    private DateTime _started;
    private List<PointStar> _stars;
    private List<Checkpoint> _checkpoints;
    private List<SimpleEnemyAI> _enemies;
    private List<GiveHealth> _healthPacks;
    private List<SimpleEnemyAI> _allEnemies;
    private int _currentCheckpointIndex;
    private int _savedPoints;

    public Checkpoint DebugSpawn = null;
    public int BonusCutoffSeconds = 12;
    public int BonusSecondMultiplier = 7;
    public bool HardMode = false;
    public int CheckpointDisplaySeconds = 1;
    public FloatingTextParameters EndingTextParameters;

    public void Awake()
    {
        Instance = this;
        _stars = new List<PointStar>();
        _enemies = new List<SimpleEnemyAI>();
        _healthPacks = new List<GiveHealth>();
        _savedPoints = GameManager.Instance.Points;
        _allEnemies = FindObjectsOfType<SimpleEnemyAI>().ToList<SimpleEnemyAI>();

    }

    public void Start()
    {
        var checkpoints = FindObjectsOfType<Checkpoint>();
        _checkpoints = checkpoints.ToList();

        for (var i = 0; i < _checkpoints.Count; i++)
        {
            for (var j = 0; j < _checkpoints.Count; j++)
            {
                var one = _checkpoints[i].transform.position.x;
                var two = _checkpoints[j].transform.position.x;

                if (two > one)
                {
                    var checkpoint = _checkpoints[j];
                    _checkpoints[j] = _checkpoints[i];
                    _checkpoints[i] = checkpoint;
                }
            }
        }

        _currentCheckpointIndex = _checkpoints.Count > 0 ? 0 : -1;
        _started = DateTime.UtcNow;

        Player = FindObjectOfType<Player>();
        Camera = FindObjectOfType<CameraController>();

#if UNITY_EDITOR
        if (DebugSpawn != null)
        {
            DebugSpawn.SpawnPlayer(Player);
        }
        else if (_currentCheckpointIndex != -1)
        {
            _checkpoints[_currentCheckpointIndex].SpawnPlayer(Player);
        }
#else
        if(_currentCheckpointIndex != -1)
        {
             _checkpoints[_currentCheckpointIndex].SpawnPlayer(Player);
        }
#endif
    }

    public void Update()
    {
        // Do horrible things if the Player gets out
        if (HardMode)
        {
            if (BonusCutoffSeconds - RunningTime.TotalSeconds < 0)
            {
                if (!Player.IsDead)
                    KillPlayer();
            }
        }

        var isOnLastCheckpoint = _currentCheckpointIndex >= _checkpoints.Count - 1;
        if (isOnLastCheckpoint)
        {
            IsAtLastCheckpoint = true;

            // Destroy the Stars at the LastCheckpoint. That means that the player cannot move further.
            // DestroyStars();
            return;
        }
        var distanceToNextCheckpoint =
            _checkpoints[_currentCheckpointIndex + 1].transform.position.x - Player.transform.position.x;

        if (distanceToNextCheckpoint > 0)
            return;

        _checkpoints[_currentCheckpointIndex].PlayerLeftCheckpoint();
        _currentCheckpointIndex++;
        _checkpoints[_currentCheckpointIndex].PlayerHitCheckpoint();

        // TODO: Time Bonus
        GameManager.Instance.AddPoints(CurrentTimeBonus);
        _started = DateTime.UtcNow;
        _savedPoints = GameManager.Instance.Points;

    }

    public void KillPlayer()
    {
        StartCoroutine(KillPlayerCo());
    }

    public void AddStar(PointStar star)
    {
        _stars.Add(star);
    }

    public void DestroyStars()
    {
        foreach (PointStar star in _stars)
        {
            star.DestroyStar();

        }

        _stars = new List<PointStar>();
    }

    private void RespawnStars()
    {
        foreach (PointStar star in _stars)
        {
            star.Respawn();

        }

        _stars = new List<PointStar>();
    }

    public void AddHealthPack(GiveHealth healthPack)
    {
        _healthPacks.Add(healthPack);
    }

    public void DestroyHealthPacks()
    {
        foreach (GiveHealth pack in _healthPacks)
        {
            Destroy(pack.gameObject);
        }

        _healthPacks = new List<GiveHealth>();
    }

    private void RespawnHealthPacks()
    {
        foreach (GiveHealth pack in _healthPacks)
        {
            pack.Respawn();
        }

        _healthPacks = new List<GiveHealth>();
    }

    public void DestroyEnemies()
    {
        foreach (SimpleEnemyAI ai in _enemies)
        {
            Destroy(ai.gameObject);
        }

        _enemies = new List<SimpleEnemyAI>();
    }

    private void RespawnEnemies()
    {
        foreach (SimpleEnemyAI enemy in _enemies)
        {
            enemy.Respawn();
        }
        _enemies = new List<SimpleEnemyAI>();
    }

    public void AddEnemy(SimpleEnemyAI Enemy)
    {
        _enemies.Add(Enemy);
    }

    public void DisplayCheckpoint()
    {
        var gameHud = GetComponent<GameHud>();
        if (!gameHud)
            return;

        gameHud.CheckpointReached = true;
    }

    public void RemoveDisplayCheckpoint()
    {
        var gameHud = GetComponent<GameHud>();
        if (!gameHud)
            return;

        gameHud.CheckpointReached = false;
    }

    public void GoToNextLevel(string LevelName)
    {
        StartCoroutine(GoToNextLevelCo(LevelName));
    }

    private void ResetEnemyHealth()
    {
        foreach(SimpleEnemyAI enemy in _allEnemies)
        {
            if (enemy == null)
                continue;

            enemy.ResetHeath();
        }
    }

    public IEnumerator GoToNextLevelCo(string levelName)
    {
        Player.FinishLevel();

        GameManager.Instance.AddPoints(CurrentTimeBonus);
        FloatingText.Show("Level Complete!", "EndLevelText",
            new CenteredFloatingTextPositioner(EndingTextParameters.Speed, EndingTextParameters.TimeToLive));

        yield return new WaitForSeconds(2f);

        FloatingText.Show(string.Format("{0} points!", GameManager.Instance.Points),
            "EndLevelText", new CenteredFloatingTextPositioner(EndingTextParameters.Speed, EndingTextParameters.TimeToLive));

        yield return new WaitForSeconds(4f);

        if (string.IsNullOrEmpty(levelName))
            Application.LoadLevel("StartScreen");
        else
            Application.LoadLevel(levelName);

        yield break;
    }

    private IEnumerator KillPlayerCo()
    {
        Player.Kill();
        Camera.IsFollowing = false;

        yield return new WaitForSeconds(2f);

        RespawnStars();
        RespawnEnemies();
        RespawnHealthPacks();
        ResetEnemyHealth();
        Camera.IsFollowing = true;
        if (_currentCheckpointIndex != -1)
        {
            _checkpoints[_currentCheckpointIndex].SpawnPlayer(Player);
        }

        // TODO: GameManager Stuff.
        _started = DateTime.UtcNow;
        GameManager.Instance.ResetPoints(_savedPoints);
    }

}
