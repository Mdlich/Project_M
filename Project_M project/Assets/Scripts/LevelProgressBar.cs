using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelProgressBar : MonoBehaviour
{
	private Transform player;
	private Slider slider;
	private void Start()
	{
		if (SceneManager.GetActiveScene().buildIndex > 3)
		{
			gameObject.SetActive( false );
			return;
		}
		player = FindObjectOfType<PlayerController>().transform;
		slider = GetComponent<Slider>();
	}
	private void Update()
	{
		slider.SetValueWithoutNotify( player.position.y / GameManager.WinningDistance );
	}
}
