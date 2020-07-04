using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OptionButton : CustomButton
{
	[SerializeField]
	private Toggle ppToggle;
	[SerializeField]
	private Toggle tutorialToggle;
	[SerializeField]
	private Toggle fpsToggle;

	private new void Start()
	{
		var toggle = GetComponent<Toggle>();
		if (toggle)
		{
			toggle.onValueChanged.AddListener( ( bool _ ) => OnClick() );
		}
		else
			base.Start();
	}

	private void Update()
	{
		if (ppToggle)
			ppToggle.SetIsOnWithoutNotify( GameManager.PPActive == 1 );
		if (tutorialToggle)
			tutorialToggle.SetIsOnWithoutNotify( GameManager.TutorialActive == 1 );
		if (fpsToggle)
			fpsToggle.SetIsOnWithoutNotify( GameManager.ShowFPS == 1 );
	}
	public void ToggleTutorial(bool active )
	{
		GameManager.SetTutorialActive( active ? 1 : 0 );
	}

	public void TogglePP(bool active )
	{
		GameManager.PPActive = active ? 1: 0;
	}
	public void ToggleFPS( bool active )
	{
		GameManager.ShowFPS = active ? 1 : 0;
	}
}
