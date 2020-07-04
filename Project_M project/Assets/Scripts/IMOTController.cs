using System;
using UnityEngine;

public interface IMOTController
{
	Vector2 GetDirection();
	bool GetInControllerRange();
}