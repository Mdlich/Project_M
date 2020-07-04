using UnityEngine;
using System.Collections;

[CreateAssetMenu( fileName = "Shield boost", menuName = "Pickup effects/ShieldBoost", order = 1 )]
public class ShieldBoost : PickupEffect
{
	public int bonus = 1;
	public override void ApplyEffect( PlayerController player )
	{
		player.BoostShield( bonus );
	}
}
