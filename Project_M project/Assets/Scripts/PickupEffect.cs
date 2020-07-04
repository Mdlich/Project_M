using UnityEngine;
using System.Collections;

public abstract class PickupEffect: ScriptableObject
{
	public string graphicsObjectName;
	public abstract void ApplyEffect( PlayerController player );
}