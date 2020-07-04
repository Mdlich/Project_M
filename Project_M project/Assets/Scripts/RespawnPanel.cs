using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RespawnPanel : MonoBehaviour
{
	[SerializeField] [TextArea] private string respawnDescription;
	[SerializeField] [TextArea] private string respawnButtonText;
	[SerializeField] [TextArea] private string restartDesription;
	[SerializeField] [TextArea] private string restartButtonText;

	[SerializeField] private Text respawnText;
	[SerializeField] private Text respawnButton;
	[SerializeField] private Text respawnsLeftText;

	private void OnEnable()
	{
		if (GameManager.LivesLeft > 0)
		{
			respawnText.text = respawnDescription;
			respawnButton.text = respawnButtonText;
			respawnsLeftText.text = $"Respawns left: {GameManager.LivesLeft}";
			respawnsLeftText.gameObject.SetActive( true );
		}
		else
		{
			respawnsLeftText.gameObject.SetActive( false );
			respawnText.text = restartDesription;
			respawnButton.text = restartButtonText;
		}
	}

	public void Respawn()
	{
		gameObject.SetActive( false );
		GameManager.Respawn();
	}
}
