using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	private GameObject mario;

	void Start()
	{
		var marioObj = FindObjectOfType<Mario>();
		if (marioObj == null)
		{
			var endlessMarioObj = FindObjectOfType<EndlessMario>();
			if (endlessMarioObj != null)
				mario = endlessMarioObj.gameObject;
		}
		else
		{
			mario = marioObj.gameObject;
		}
	}

	void Update()
	{
		if (mario == null) return;
		// update spawn pos if Player passes checkpoint
		if (mario.transform.position.x >= transform.position.x)
		{
			GameStateManager t_GameStateManager = FindObjectOfType<GameStateManager>();
			t_GameStateManager.spawnPointIdx = Mathf.Max(t_GameStateManager.spawnPointIdx, gameObject.transform.GetSiblingIndex());
			gameObject.SetActive(false);
		}
	}
}
