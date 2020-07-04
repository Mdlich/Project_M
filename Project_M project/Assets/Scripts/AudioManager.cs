using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[SerializeField] private AudioSource music;
	[SerializeField] private AudioSource sounds;

	[SerializeField] private float musicFadeTime = 0.5f;
	[SerializeField] private AudioClip[] musicClips;

	public static float MusicVolume { get; private set; }
	public static float SoundVolume { get; private set; }
	private void Awake()
	{
		if (Instance)
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		Init();
	}

	private void Start()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}
	private void Init()
	{
		ChangeMusicVolume( PlayerPrefs.GetFloat( "musicVolume", 0.5f ) );
		ChangeSoundsVolume( PlayerPrefs.GetFloat( "soundVolume", 0.5f ) );
	}

	private void OnSceneLoaded( Scene s, LoadSceneMode mode )
	{
		int index = s.buildIndex;
		if (index > 3)
			index -= 3;
		StartCoroutine( CrossfadeMusic( musicClips[index]) );
	}


	private IEnumerator CrossfadeMusic(AudioClip clip )
	{
		float t = 0f;
		music.volume = 0f;
		music.clip = clip;
		music.Play();
		while (t < 1f)
		{
			music.volume = Mathf.Lerp( 0f, MusicVolume, t );
			t += Time.unscaledDeltaTime / musicFadeTime;
			yield return null;
		}
	}

	public static void ChangeMusicVolume(float volume )
	{
		Instance.music.volume = volume;
		MusicVolume = volume;
		PlayerPrefs.SetFloat( "musicVolume", volume );
	}

	public static void ChangeSoundsVolume(float volume )
	{
		Instance.sounds.volume = volume;
		SoundVolume = volume;
		PlayerPrefs.SetFloat( "soundVolume", volume );
	}
}
