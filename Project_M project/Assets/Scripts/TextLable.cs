using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextLable : MonoBehaviour
{
	public string postFix;
	private Text textLable;
	private void Start()
	{
		textLable = GetComponent<Text>();
	}
	public void UpdateText( string text )
	{
		textLable.text = $"{text}{postFix}";
	}
}
