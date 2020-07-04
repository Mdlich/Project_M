using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{

	protected void Start()
	{
		GetComponent<Button>().onClick.AddListener( OnClick );
	}

	protected void OnClick()
	{
		SoundManager.PlaySound( SoundManager.Sound.ButtonClick );
	}
}
