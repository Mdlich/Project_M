using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager instance;
	public SoundClip[] sounds;
	private AudioSource audioSource;

	public enum Sound { Bounce, ObstacleCollision, TentacleSwing, LavaCollision, Pickup, ButtonClick, Defeat, Victory};

	[System.Serializable]
	public class SoundClip
	{
		public Sound sound;
		public AudioClip audioClip;
	}

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
		}
		else if( this != instance )
		{
			Destroy( gameObject );
			return;
		}

		audioSource = GetComponent<AudioSource>();
		if (!audioSource)
		{
			Debug.LogWarning( "SoundManager doesn't have access to audioSource" );
		}
	}

	public static void PlaySound(Sound sound )
	{
		if (!instance || !instance.audioSource)
			return;

		foreach (var s in instance.sounds)
		{
			if (sound == s.sound)
			{
				instance.audioSource.PlayOneShot( s.audioClip );
				return;
			}
		}
	}
}
