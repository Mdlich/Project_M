using UnityEngine;
using System.Collections;
using Unity.RemoteConfig;

public class RemoteConfigManager : MonoBehaviour
{
	public static RemoteConfigManager instance { get; private set; }

	public struct userAttributes { };
	public struct appAtributes { };

	public static float[] MaxLavaSpeed { private set; get; } = new float[3];
	public static float[] LavaStartingSpeed { private set; get; } = new float[3];
	public static float[] LavaSpeedUpTime { private set; get; } = new float[3];

	private void Awake()
	{
		if (instance)
		{
			Destroy( gameObject );
			return;
		}
		instance = this;
		ConfigManager.FetchCompleted += FetchData;
		ConfigManager.FetchConfigs<userAttributes, appAtributes>( new userAttributes(), new appAtributes() );
	}

	private void FetchData(ConfigResponse response )
	{
		for (int i = 0; i < MaxLavaSpeed.Length; i++)
		{
			MaxLavaSpeed[i] = ConfigManager.appConfig.GetFloat( $"MaxLavaSpeedLevel{i}", 30 );
		}
		for (int i = 0; i < LavaStartingSpeed.Length; i++)
		{
			LavaStartingSpeed[i] = ConfigManager.appConfig.GetFloat( $"LavaStartSpeedLevel{i}", 8 );
		}
		for (int i = 0; i < LavaSpeedUpTime.Length; i++)
		{
			LavaSpeedUpTime[i] = ConfigManager.appConfig.GetFloat( $"LavaMaxSpeedTime{i}", 180 );
		}
	}
}
