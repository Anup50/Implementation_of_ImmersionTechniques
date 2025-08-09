using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class LevelManager : MonoBehaviour
{
	private float scoreAccumulator = 0f;
	public enum GameMode { Classic, Endless }
	public GameMode gameMode = GameMode.Classic;
	public float timeSurvived;
	private int timeSurvivedInt;
	private const float loadSceneDelay = 1f;

	public bool hurryUp;
	public int marioSize;
	public int lives;
	public int coins;
	public int scores;
	public float timeLeft;
	private int timeLeftInt;

	private bool isRespawning;
	private bool isPoweringDown;

	public bool isInvinciblePowerdown;
	public bool isInvincibleStarman;
	private float MarioInvinciblePowerdownDuration = 2;
	private float MarioInvincibleStarmanDuration = 12;
	private float transformDuration = 1;

	private GameStateManager t_GameStateManager;
	private Mario mario;
	private Animator mario_Animator;
	private Rigidbody2D mario_Rigidbody2D;

	public Text scoreText;
	public Text coinText;
	public Text timeText;
	public GameObject FloatingTextEffect;
	private const float floatingTextOffsetY = 2f;

	public AudioSource musicSource;
	public AudioSource soundSource;
	public AudioSource pauseSoundSource;

	public AudioClip levelMusic;
	public AudioClip levelMusicHurry;
	public AudioClip starmanMusic;
	public AudioClip starmanMusicHurry;
	public AudioClip levelCompleteMusic;
	public AudioClip castleCompleteMusic;

	public AudioClip oneUpSound;
	public AudioClip bowserFallSound;
	public AudioClip bowserFireSound;
	public AudioClip breakBlockSound;
	public AudioClip bumpSound;
	public AudioClip coinSound;
	public AudioClip deadSound;
	public AudioClip fireballSound;
	public AudioClip flagpoleSound;
	public AudioClip jumpSmallSound;
	public AudioClip jumpSuperSound;
	public AudioClip kickSound;
	public AudioClip pipePowerdownSound;
	public AudioClip powerupSound;
	public AudioClip powerupAppearSound;
	public AudioClip stompSound;
	public AudioClip warningSound;

	public int coinBonus = 200;
	public int powerupBonus = 1000;
	public int starmanBonus = 1000;
	public int oneupBonus = 0;
	public int breakBlockBonus = 50;

	public Vector2 stompBounceVelocity = new Vector2(0, 15);

	public bool gamePaused;
	public bool timerPaused;
	public bool musicPaused;


	void Awake()
	{
		Time.timeScale = 1;
	}

	void Start()
	{
		if (gameMode == GameMode.Endless)
		{
			timeSurvived = 0f;
		}
		t_GameStateManager = FindObjectOfType<GameStateManager>();
		RetrieveGameState();
		if (gameMode == GameMode.Endless)
		{
			lives = 1;
			t_GameStateManager.lives = 1; // keep GameStateManager in sync
		}

		var marioObj = FindObjectOfType<Mario>();
		if (marioObj == null)
		{
			var endlessMarioObj = FindObjectOfType<EndlessMario>();
			if (endlessMarioObj != null)
			{
				mario = null;
				mario_Animator = endlessMarioObj.GetComponent<Animator>();
				mario_Rigidbody2D = endlessMarioObj.GetComponent<Rigidbody2D>();
				endlessMarioObj.SendMessage("UpdateSize", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				mario_Animator = null;
				mario_Rigidbody2D = null;
			}
		}
		else
		{
			mario = marioObj;
			mario_Animator = mario.gameObject.GetComponent<Animator>();
			mario_Rigidbody2D = mario.gameObject.GetComponent<Rigidbody2D>();
			mario.UpdateSize();
		}

		// Sound volume
		musicSource.volume = PlayerPrefs.GetFloat("musicVolume");
		soundSource.volume = PlayerPrefs.GetFloat("soundVolume");
		pauseSoundSource.volume = PlayerPrefs.GetFloat("soundVolume");

		// HUD
		SetHudCoin();
		SetHudScore();
		SetHudTime();
		if (hurryUp)
		{
			ChangeMusic(levelMusicHurry);
		}
		else
		{
			ChangeMusic(levelMusic);
		}

		Debug.Log(this.name + " Start: current scene is " + SceneManager.GetActiveScene().name);
	}

	void RetrieveGameState()
	{
		marioSize = t_GameStateManager.marioSize;
		lives = t_GameStateManager.lives;
		coins = t_GameStateManager.coins;
		scores = t_GameStateManager.scores;
		timeLeft = t_GameStateManager.timeLeft;
		hurryUp = t_GameStateManager.hurryUp;
	}


	/****************** Timer */
	void Update()
	{
		if (gameMode == GameMode.Endless)
		{
			if (!gamePaused)
			{
				timeSurvived += Time.deltaTime;
				SetHudTime();

				float multiplier = Mathf.Min(1.5f + timeSurvived / 60f, 5f);
				float scoreGain = Time.deltaTime * 1f * multiplier;
				scoreAccumulator += scoreGain;
				int scoreToAdd = Mathf.FloorToInt(scoreAccumulator);
				if (scoreToAdd > 0)
				{
					scores += scoreToAdd;
					scoreAccumulator -= scoreToAdd;
					SetHudScore();
				}
				Debug.Log($"Endless mode: timeSurvived={timeSurvived}, multiplier={multiplier}, scoreGain={scoreGain}, scores={scores}");
			}
		}
		else
		{
			if (!timerPaused)
			{
				timeLeft -= Time.deltaTime / .4f; // 1 game sec ~ 0.4 real time sec
				SetHudTime();
			}

			if (timeLeftInt < 100 && !hurryUp)
			{
				hurryUp = true;
				PauseMusicPlaySound(warningSound, true);
				if (isInvincibleStarman)
				{
					ChangeMusic(starmanMusicHurry, warningSound.length);
				}
				else
				{
					ChangeMusic(levelMusicHurry, warningSound.length);
				}
			}

			if (timeLeftInt <= 0)
			{
				MarioRespawn(true);
			}
		}

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


	/****************** Game pause */
	List<Animator> unscaledAnimators = new List<Animator>();
	float pauseGamePrevTimeScale;
	bool pausePrevMusicPaused;

	IEnumerator PauseGameCo()
	{
		gamePaused = true;
		pauseGamePrevTimeScale = Time.timeScale;

		Time.timeScale = 0;
		pausePrevMusicPaused = musicPaused;
		musicSource.Pause();
		musicPaused = true;
		soundSource.Pause();

		// Set any active animators that use unscaled time mode to normal
		unscaledAnimators.Clear();
		foreach (Animator animator in FindObjectsOfType<Animator>())
		{
			if (animator.updateMode == AnimatorUpdateMode.UnscaledTime)
			{
				unscaledAnimators.Add(animator);
				animator.updateMode = AnimatorUpdateMode.Normal;
			}
		}

		pauseSoundSource.Play();
		yield return new WaitForSecondsRealtime(pauseSoundSource.clip.length);
		Debug.Log(this.name + " PauseGameCo stops: records prevTimeScale=" + pauseGamePrevTimeScale.ToString());
	}

	IEnumerator UnpauseGameCo()
	{
		pauseSoundSource.Play();
		yield return new WaitForSecondsRealtime(pauseSoundSource.clip.length);

		musicPaused = pausePrevMusicPaused;
		if (!musicPaused)
		{
			musicSource.UnPause();
		}
		soundSource.UnPause();

		// Reset animators
		foreach (Animator animator in unscaledAnimators)
		{
			animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		unscaledAnimators.Clear();

		Time.timeScale = pauseGamePrevTimeScale;
		gamePaused = false;
		Debug.Log(this.name + " UnpauseGameCo stops: resume prevTimeScale=" + pauseGamePrevTimeScale.ToString());
	}


	/****************** Invincibility */
	public bool isInvincible()
	{
		return isInvinciblePowerdown || isInvincibleStarman;
	}

	public void MarioInvincibleStarman()
	{
		StartCoroutine(MarioInvincibleStarmanCo());
		AddScore(starmanBonus, mario.transform.position);
	}

	IEnumerator MarioInvincibleStarmanCo()
	{
		isInvincibleStarman = true;
		mario_Animator.SetBool("isInvincibleStarman", true);
		mario.gameObject.layer = LayerMask.NameToLayer("Mario After Starman");
		if (hurryUp)
		{
			ChangeMusic(starmanMusicHurry);
		}
		else
		{
			ChangeMusic(starmanMusic);
		}
		yield return new WaitForSeconds(MarioInvincibleStarmanDuration);
		isInvincibleStarman = false;
		mario_Animator.SetBool("isInvincibleStarman", false);
		mario.gameObject.layer = LayerMask.NameToLayer("Mario");
		if (hurryUp)
		{
			ChangeMusic(levelMusicHurry);
		}
		else
		{
			ChangeMusic(levelMusic);
		}
	}

	void MarioInvinciblePowerdown()
	{
		StartCoroutine(MarioInvinciblePowerdownCo());
	}

	IEnumerator MarioInvinciblePowerdownCo()
	{
		isInvinciblePowerdown = true;
		mario_Animator.SetBool("isInvinciblePowerdown", true);
		mario.gameObject.layer = LayerMask.NameToLayer("Mario After Powerdown");
		yield return new WaitForSeconds(MarioInvinciblePowerdownDuration);
		isInvinciblePowerdown = false;
		mario_Animator.SetBool("isInvinciblePowerdown", false);
		mario.gameObject.layer = LayerMask.NameToLayer("Mario");
	}


	/****************** Powerup / Powerdown / Die */
	public void MarioPowerUp()
	{
		soundSource.PlayOneShot(powerupSound); // should play sound regardless of size
		if (marioSize < 2)
		{
			StartCoroutine(MarioPowerUpCo());
		}
		AddScore(powerupBonus, mario.transform.position);
	}

	IEnumerator MarioPowerUpCo()
	{
		mario_Animator.SetBool("isPoweringUp", true);
		Time.timeScale = 0f;
		mario_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

		yield return new WaitForSecondsRealtime(transformDuration);
		yield return new WaitWhile(() => gamePaused);

		Time.timeScale = 1;
		mario_Animator.updateMode = AnimatorUpdateMode.Normal;

		marioSize++;
		mario.UpdateSize();
		mario_Animator.SetBool("isPoweringUp", false);
	}

	public void MarioPowerDown()
	{
		if (!isPoweringDown)
		{
			Debug.Log(this.name + " MarioPowerDown: called and executed");
			isPoweringDown = true;

			if (marioSize > 0)
			{
				StartCoroutine(MarioPowerDownCo());
				soundSource.PlayOneShot(pipePowerdownSound);
			}
			else
			{
				MarioRespawn();
			}
			Debug.Log(this.name + " MarioPowerDown: done executing");
		}
		else
		{
			Debug.Log(this.name + " MarioPowerDown: called but not executed");
		}
	}

	IEnumerator MarioPowerDownCo()
	{
		mario_Animator.SetBool("isPoweringDown", true);
		Time.timeScale = 0f;
		mario_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

		yield return new WaitForSecondsRealtime(transformDuration);
		yield return new WaitWhile(() => gamePaused);

		Time.timeScale = 1;
		mario_Animator.updateMode = AnimatorUpdateMode.Normal;
		MarioInvinciblePowerdown();

		marioSize = 0;
		mario.UpdateSize();
		mario_Animator.SetBool("isPoweringDown", false);
		isPoweringDown = false;
	}

	public void MarioRespawn(bool timeup = false)
	{
		if (!isRespawning)
		{
			isRespawning = true;

			marioSize = 0;
			lives--;

			soundSource.Stop();
			musicSource.Stop();
			musicPaused = true;
			soundSource.PlayOneShot(deadSound);

			Time.timeScale = 0f;
			if (mario != null)
			{
				mario.FreezeAndDie();
			}
			else
			{
				var endlessMario = FindObjectOfType<EndlessMario>();
				if (endlessMario != null)
					endlessMario.SendMessage("FreezeAndDie", SendMessageOptions.DontRequireReceiver);
			}

			if (timeup)
			{
				Debug.Log(this.name + " MarioRespawn: called due to timeup");
			}
			Debug.Log(this.name + " MarioRespawn: lives left=" + lives.ToString());

			if (lives > 0)
			{
				ReloadCurrentLevel(deadSound.length, timeup);
			}
			else
			{
				LoadGameOver(deadSound.length, timeup);
				Debug.Log(this.name + " MarioRespawn: all dead");
			}
		}
	}


	/****************** Kill enemy */
	public void MarioStompEnemy(Enemy enemy)
	{
		if (mario_Rigidbody2D != null)
			mario_Rigidbody2D.linearVelocity = new Vector2(mario_Rigidbody2D.linearVelocity.x + stompBounceVelocity.x, stompBounceVelocity.y);
		if (enemy != null)
		{
			enemy.StompedByMario();
			AddScore(enemy.stompBonus, enemy.gameObject.transform.position);
			Debug.Log(this.name + " MarioStompEnemy called on " + enemy.gameObject.name);
		}
		soundSource.PlayOneShot(stompSound);
	}

	public void MarioStarmanTouchEnemy(Enemy enemy)
	{
		enemy.TouchedByStarmanMario();
		soundSource.PlayOneShot(kickSound);
		AddScore(enemy.starmanBonus, enemy.gameObject.transform.position);
		Debug.Log(this.name + " MarioStarmanTouchEnemy called on " + enemy.gameObject.name);
	}

	public void RollingShellTouchEnemy(Enemy enemy)
	{
		enemy.TouchedByRollingShell();
		soundSource.PlayOneShot(kickSound);
		AddScore(enemy.rollingShellBonus, enemy.gameObject.transform.position);
		Debug.Log(this.name + " RollingShellTouchEnemy called on " + enemy.gameObject.name);
	}

	public void BlockHitEnemy(Enemy enemy)
	{
		enemy.HitBelowByBlock();
		AddScore(enemy.hitByBlockBonus, enemy.gameObject.transform.position);
		Debug.Log(this.name + " BlockHitEnemy called on " + enemy.gameObject.name);
	}

	public void FireballTouchEnemy(Enemy enemy)
	{
		enemy.HitByMarioFireball();
		soundSource.PlayOneShot(kickSound);
		AddScore(enemy.fireballBonus, enemy.gameObject.transform.position);
		Debug.Log(this.name + " FireballTouchEnemy called on " + enemy.gameObject.name);
	}

	/****************** Scene loading */
	void LoadSceneDelay(string sceneName, float delay = loadSceneDelay)
	{
		timerPaused = true;
		StartCoroutine(LoadSceneDelayCo(sceneName, delay));
	}

	IEnumerator LoadSceneDelayCo(string sceneName, float delay)
	{
		Debug.Log(this.name + " LoadSceneDelayCo: starts loading " + sceneName);

		float waited = 0;
		while (waited < delay)
		{
			if (!gamePaused)
			{ // should not count delay while game paused
				waited += Time.unscaledDeltaTime;
			}
			yield return null;
		}
		yield return new WaitWhile(() => gamePaused);

		Debug.Log(this.name + " LoadSceneDelayCo: done loading " + sceneName);

		isRespawning = false;
		isPoweringDown = false;
		SceneManager.LoadScene(sceneName);
	}

	public void LoadNewLevel(string sceneName, float delay = loadSceneDelay)
	{
		t_GameStateManager.SaveGameState();
		t_GameStateManager.ConfigNewLevel();
		t_GameStateManager.sceneToLoad = sceneName;
		LoadSceneDelay("Level Start Screen", delay);
	}

	public void LoadSceneCurrentLevel(string sceneName, float delay = loadSceneDelay)
	{
		t_GameStateManager.SaveGameState();
		t_GameStateManager.ResetSpawnPosition(); // TODO
		LoadSceneDelay(sceneName, delay);
	}

	public void LoadSceneCurrentLevelSetSpawnPipe(string sceneName, int spawnPipeIdx, float delay = loadSceneDelay)
	{
		t_GameStateManager.SaveGameState();
		t_GameStateManager.SetSpawnPipe(spawnPipeIdx);
		LoadSceneDelay(sceneName, delay);
		Debug.Log(this.name + " LoadSceneCurrentLevelSetSpawnPipe: supposed to load " + sceneName
			+ ", spawnPipeIdx=" + spawnPipeIdx.ToString() + "; actual GSM spawnFromPoint="
			+ t_GameStateManager.spawnFromPoint.ToString() + ", spawnPipeIdx="
			+ t_GameStateManager.spawnPipeIdx.ToString());
	}

	public void ReloadCurrentLevel(float delay = loadSceneDelay, bool timeup = false)
	{
		t_GameStateManager.SaveGameState();
		t_GameStateManager.ConfigReplayedLevel();
		t_GameStateManager.sceneToLoad = SceneManager.GetActiveScene().name;
		if (timeup)
		{
			LoadSceneDelay("Time Up Screen", delay);
		}
		else
		{
			LoadSceneDelay("Level Start Screen", delay);
		}
	}

	public void LoadGameOver(float delay = loadSceneDelay, bool timeup = false)
	{
		int currentHighScore = PlayerPrefs.GetInt("highScore", 0);
		if (scores > currentHighScore)
		{
			PlayerPrefs.SetInt("highScore", scores);
		}
		t_GameStateManager.timeup = timeup;
		LoadSceneDelay("Game Over Screen", delay);
	}


	/****************** HUD and sound effects */
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
		if (gameMode == GameMode.Endless)
		{
			timeSurvivedInt = Mathf.FloorToInt(timeSurvived);
			timeText.text = timeSurvivedInt.ToString("D3");
		}
		else
		{
			timeLeftInt = Mathf.RoundToInt(timeLeft);
			timeText.text = timeLeftInt.ToString("D3");
		}
	}

	public void CreateFloatingText(string text, Vector3 spawnPos)
	{
		GameObject textEffect = Instantiate(FloatingTextEffect, spawnPos, Quaternion.identity);
		textEffect.GetComponentInChildren<TextMesh>().text = text.ToUpper();
	}


	public void ChangeMusic(AudioClip clip, float delay = 0)
	{
		StartCoroutine(ChangeMusicCo(clip, delay));
	}

	IEnumerator ChangeMusicCo(AudioClip clip, float delay)
	{
		Debug.Log(this.name + " ChangeMusicCo: starts changing music to " + clip.name);
		musicSource.clip = clip;
		yield return new WaitWhile(() => gamePaused);
		yield return new WaitForSecondsRealtime(delay);
		yield return new WaitWhile(() => gamePaused || musicPaused);
		if (!isRespawning)
		{
			musicSource.Play();
		}
		Debug.Log(this.name + " ChangeMusicCo: done changing music to " + clip.name);
	}

	public void PauseMusicPlaySound(AudioClip clip, bool resumeMusic)
	{
		StartCoroutine(PauseMusicPlaySoundCo(clip, resumeMusic));
	}

	IEnumerator PauseMusicPlaySoundCo(AudioClip clip, bool resumeMusic)
	{
		string musicClipName = "";
		if (musicSource.clip)
		{
			musicClipName = musicSource.clip.name;
		}
		Debug.Log(this.name + " PausemusicPlaySoundCo: starts pausing music " + musicClipName + " to play sound " + clip.name);

		musicPaused = true;
		musicSource.Pause();
		soundSource.PlayOneShot(clip);
		yield return new WaitForSeconds(clip.length);
		if (resumeMusic)
		{
			musicSource.UnPause();

			musicClipName = "";
			if (musicSource.clip)
			{
				musicClipName = musicSource.clip.name;
			}
			Debug.Log(this.name + " PausemusicPlaySoundCo: resume playing music " + musicClipName);
		}
		musicPaused = false;

		Debug.Log(this.name + " PausemusicPlaySoundCo: done pausing music to play sound " + clip.name);
	}

	/****************** Game state */
	public void AddLife()
	{
		lives++;
		soundSource.PlayOneShot(oneUpSound);
	}

	public void AddLife(Vector3 spawnPos)
	{
		lives++;
		soundSource.PlayOneShot(oneUpSound);
		CreateFloatingText("1UP", spawnPos);
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

	public void AddCoin(Vector3 spawnPos)
	{
		coins++;
		soundSource.PlayOneShot(coinSound);
		if (coins == 100)
		{
			AddLife();
			coins = 0;
		}
		SetHudCoin();
		AddScore(coinBonus, spawnPos);
	}

	public void AddScore(int bonus)
	{
		scores += bonus;
		SetHudScore();
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


	/****************** Misc */
	public Vector3 FindSpawnPosition()
	{
		Vector3 spawnPosition;
		GameStateManager t_GameStateManager = FindObjectOfType<GameStateManager>();
		Debug.Log(this.name + " FindSpawnPosition: GSM spawnFromPoint=" + t_GameStateManager.spawnFromPoint.ToString()
			+ " spawnPipeIdx= " + t_GameStateManager.spawnPipeIdx.ToString()
			+ " spawnPointIdx=" + t_GameStateManager.spawnPointIdx.ToString());
		if (t_GameStateManager.spawnFromPoint)
		{
			spawnPosition = GameObject.Find("Spawn Points").transform.GetChild(t_GameStateManager.spawnPointIdx).transform.position;
		}
		else
		{
			spawnPosition = GameObject.Find("Spawn Pipes").transform.GetChild(t_GameStateManager.spawnPipeIdx).transform.Find("Spawn Pos").transform.position;
		}
		return spawnPosition;
	}

	public string GetWorldName(string sceneName)
	{
		string[] sceneNameParts = Regex.Split(sceneName, " - ");
		return sceneNameParts[0];
	}

	public bool isSceneInCurrentWorld(string sceneName)
	{
		return GetWorldName(sceneName) == GetWorldName(SceneManager.GetActiveScene().name);
	}

	public void MarioCompleteCastle()
	{
		timerPaused = true;
		ChangeMusic(castleCompleteMusic);
		musicSource.loop = false;
		mario.AutomaticWalk(mario.castleWalkSpeedX);
	}

	public void MarioCompleteLevel()
	{
		timerPaused = true;
		ChangeMusic(levelCompleteMusic);
		musicSource.loop = false;
	}

	public void MarioReachFlagPole()
	{
		timerPaused = true;
		PauseMusicPlaySound(flagpoleSound, false);
		mario.ClimbFlagPole();
	}
}
