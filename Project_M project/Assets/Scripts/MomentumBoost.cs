using UnityEngine;
using System.Collections;

[CreateAssetMenu( fileName = "Momentum boost", menuName = "Pickup effects/MomentumBoost", order = 1 )]
public class MomentumBoost : PickupEffect
{
	[SerializeField]
	private float boostPercent = 0.2f;
	public override void ApplyEffect( PlayerController player )
	{
		player.Momentum += boostPercent;
	}
}
