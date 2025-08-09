using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class LevelStartScreen : MonoBehaviour
{
	private GameStateManager t_GameStateManager;
	private float loadScreenDelay = 2;

	public Text WorldTextHUD;
	public Text ScoreTextHUD;
	public Text CoinTextHUD;
	public Text WorldTextMain;
	public Text livesText;

	// Use this for initialization
	// void Start () {
	// 	Time.timeScale = 1;

	// 	t_GameStateManager = FindObjectOfType<GameStateManager> ();
	// 	string worldName = t_GameStateManager.sceneToLoad;

	// 	WorldTextHUD.text = Regex.Split (worldName, "World ")[1];
	// 	ScoreTextHUD.text = t_GameStateManager.scores.ToString ("D6");
	// 	CoinTextHUD.text = "x" + t_GameStateManager.coins.ToString ("D2");
	// 	WorldTextMain.text = worldName.ToUpper ();
	// 	livesText.text = t_GameStateManager.lives.ToString ();

	// 	StartCoroutine (LoadSceneDelayCo (t_GameStateManager.sceneToLoad, loadScreenDelay));

	// 	Debug.Log (this.name + " Start: current scene is " + SceneManager.GetActiveScene ().name);
	// }
	void Start()
	{
		Time.timeScale = 1;

		t_GameStateManager = FindObjectOfType<GameStateManager>();
		string worldName = t_GameStateManager.sceneToLoad;

		// Fix: Handle Endless Mode or any non-"World" scene
		if (worldName.Contains("World "))
		{
			WorldTextHUD.text = Regex.Split(worldName, "World ")[1];
		}
		else
		{
			WorldTextHUD.text = worldName.ToUpper();
		}

		ScoreTextHUD.text = t_GameStateManager.scores.ToString("D6");
		CoinTextHUD.text = "x" + t_GameStateManager.coins.ToString("D2");
		WorldTextMain.text = worldName.ToUpper();
		livesText.text = t_GameStateManager.lives.ToString();

		StartCoroutine(LoadSceneDelayCo(t_GameStateManager.sceneToLoad, loadScreenDelay));

		Debug.Log(this.name + " Start: current scene is " + SceneManager.GetActiveScene().name);
	}

	// IEnumerator LoadSceneDelayCo(string sceneName, float delay)
	// {
	// 	yield return new WaitForSecondsRealtime(delay);
	// 	SceneManager.LoadScene(sceneName);
	// }
	IEnumerator LoadSceneDelayCo(string sceneName, float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		Debug.Log("Loading scene: " + sceneName); // Add this line
		SceneManager.LoadScene(sceneName);
	}
}
