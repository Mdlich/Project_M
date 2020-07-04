using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
	private const string locked = "locked";
	[SerializeField] private Button[] levelStartButtons;
	[SerializeField] private Button[] deathmatchButtons;

	private void Start()
	{
		if (GameManager.GameComplete == 0 && !GameManager.TestMode)
		{
			levelStartButtons[1].interactable = false;
			levelStartButtons[2].interactable = false;
			levelStartButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = locked;
			levelStartButtons[2].GetComponentInChildren<TextMeshProUGUI>().text = locked;
		}
		if (GameManager.DeathMatchAvailable == 0 && !GameManager.TestMode)
		{
			foreach (var b in deathmatchButtons)
			{
				b.interactable = false;
				b.GetComponentInChildren<TextMeshProUGUI>().text = locked;
			}
		}
	}
	public void StartLevel(int levelID )
	{
		GameManager.LoadLevel( levelID );
	}
}
