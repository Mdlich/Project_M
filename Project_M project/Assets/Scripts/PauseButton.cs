using UnityEngine;
using System.Collections;

public class PauseButton : CustomButton
{
	public void Pause(bool pause)
	{
		GameManager.SetPauseGame( pause );
	}
}
