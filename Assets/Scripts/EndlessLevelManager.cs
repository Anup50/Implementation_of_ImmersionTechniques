using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndlessLevelManager : LevelManager
{

    public float timeSurvived;
    private int timeSurvivedInt;
    public AudioClip endlessMusic;

    void Awake()
    {
        Time.timeScale = 1;
    }

    void Start()
    {
        timeSurvived = 0f;
        SetHudCoin();
        SetHudScore();
        SetHudTime();
        musicSource.volume = PlayerPrefs.GetFloat("musicVolume");
        soundSource.volume = PlayerPrefs.GetFloat("soundVolume");
        pauseSoundSource.volume = PlayerPrefs.GetFloat("soundVolume");
        ChangeMusic(endlessMusic);
    }

    void Update()
    {
        if (!gamePaused)
        {
            timeSurvived += Time.deltaTime;
            SetHudTime();
        }
        // Pause logic (optional)
        if (Input.GetButtonDown("Pause"))
        {
            if (!gamePaused)
            {
                StartCoroutine(PauseGameCo());
            }
            else
            {
                StartCoroutine(UnpauseGameCo());
            }
        }
    }

    public void SetHudCoin()
    {
        coinText.text = "x" + coins.ToString("D2");
    }

    public void SetHudScore()
    {
        scoreText.text = scores.ToString("D6");
    }

    public void SetHudTime()
    {
        timeSurvivedInt = Mathf.FloorToInt(timeSurvived);
        timeText.text = timeSurvivedInt.ToString("D3");
    }

    public void AddCoin()
    {
        coins++;
        soundSource.PlayOneShot(coinSound);
        if (coins == 100)
        {
            AddLife();
            coins = 0;
        }
        SetHudCoin();
        AddScore(coinBonus);
    }

    public void AddLife()
    {
        lives++;
        soundSource.PlayOneShot(oneUpSound);
    }

    public void AddScore(int bonus)
    {
        scores += bonus;
        SetHudScore();
    }

    public void ChangeMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    IEnumerator PauseGameCo()
    {
        gamePaused = true;
        Time.timeScale = 0;
        musicSource.Pause();
        musicPaused = true;
        soundSource.Pause();
        pauseSoundSource.Play();
        yield return new WaitForSecondsRealtime(pauseSoundSource.clip.length);
    }

    IEnumerator UnpauseGameCo()
    {
        pauseSoundSource.Play();
        yield return new WaitForSecondsRealtime(pauseSoundSource.clip.length);
        musicPaused = false;
        musicSource.UnPause();
        soundSource.UnPause();
        Time.timeScale = 1;
        gamePaused = false;
    }

    public void CreateFloatingText(string text, Vector3 spawnPos)
    {
        GameObject textEffect = Instantiate(FloatingTextEffect, spawnPos, Quaternion.identity);
        textEffect.GetComponentInChildren<TextMesh>().text = text.ToUpper();
    }

    // --- Enemy kill logic ---
    public void MarioStompEnemy(Enemy enemy, Rigidbody2D marioRb, Transform marioTransform)
    {
        marioRb.linearVelocity = new Vector2(marioRb.linearVelocity.x + stompBounceVelocity.x, stompBounceVelocity.y);
        enemy.StompedByMario();
        soundSource.PlayOneShot(stompSound);
        AddScore(enemy.stompBonus, marioTransform.position);
        Debug.Log("EndlessLevelManager: MarioStompEnemy called on " + enemy.gameObject.name);
    }

    public void MarioStarmanTouchEnemy(Enemy enemy, Transform marioTransform)
    {
        enemy.TouchedByStarmanMario();
        soundSource.PlayOneShot(kickSound);
        AddScore(enemy.starmanBonus, marioTransform.position);
        Debug.Log("EndlessLevelManager: MarioStarmanTouchEnemy called on " + enemy.gameObject.name);
    }

    public void RollingShellTouchEnemy(Enemy enemy, Transform marioTransform)
    {
        enemy.TouchedByRollingShell();
        soundSource.PlayOneShot(kickSound);
        AddScore(enemy.rollingShellBonus, marioTransform.position);
        Debug.Log("EndlessLevelManager: RollingShellTouchEnemy called on " + enemy.gameObject.name);
    }

    public void BlockHitEnemy(Enemy enemy, Transform marioTransform)
    {
        enemy.HitBelowByBlock();
        AddScore(enemy.hitByBlockBonus, marioTransform.position);
        Debug.Log("EndlessLevelManager: BlockHitEnemy called on " + enemy.gameObject.name);
    }

    public void FireballTouchEnemy(Enemy enemy, Transform marioTransform)
    {
        enemy.HitByMarioFireball();
        soundSource.PlayOneShot(kickSound);
        AddScore(enemy.fireballBonus, marioTransform.position);
        Debug.Log("EndlessLevelManager: FireballTouchEnemy called on " + enemy.gameObject.name);
    }

    public void AddScore(int bonus, Vector3 spawnPos)
    {
        scores += bonus;
        SetHudScore();
        if (bonus > 0)
        {
            CreateFloatingText(bonus.ToString(), spawnPos);
        }
    }
}
