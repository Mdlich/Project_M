using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MomentumIndicator : MonoBehaviour
{
	[SerializeField] private Image[] bars;
	private void Start()
	{
		PlayerController.MomentumChangedEvent += OnMomentumChanged;
	}

	private void OnDestroy()
	{
		PlayerController.MomentumChangedEvent -= OnMomentumChanged;
	}
	private void OnMomentumChanged( float current, float max)
	{
		ResetAll();
		var filledBars = current * bars.Length;
		var n = 0;
		while (filledBars > 0)
		{
			if (filledBars >= 1)
			{
				bars[n].fillAmount = 1f;
			}
			else
			{
				bars[n].fillAmount = filledBars;
			}
			n++;
			filledBars--;
		}
	}

	private void ResetAll()
	{
		foreach (var bar in bars)
		{
			bar.fillAmount = 0f;
		}
	}
}
