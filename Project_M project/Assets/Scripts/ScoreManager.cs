using UnityEngine;
using System.Collections;
using System;

public class ScoreManager
{
	private const string AmetystCaveScene = "Amethyst cave";
	private const string SapphireCaveScene = "Sapphire cave";
	private const string EmeraldCaveScene = "Emerald cave";
	private const string AmetystCaveDeathmatchScene = "Amethyst cave deathmatch";
	private const string SapphireCaveDeathmatchScene = "Sapphire cave deathmatch";
	private const string EmeraldCaveDeathmatchScene = "Emerald cave deathmatch";

	public static ScoreManager instance;
	public ScoreManager()
	{
		instance = this;
	}

	public static int GetScore()
	{
		return (int) Mathf.Round(Camera.main.transform.position.y);
	}

	public static void UpdateAchievements( string sceneName )
	{
		if (GameManager.LivesLeft == GameManager.MaxLives)
		{
			GameServicesManager.UnlockAchievement( SlimeAscentResources.achievement_in_one_go );
		}
		if (sceneName == AmetystCaveScene)
		{
			GameServicesManager.UnlockAchievement( SlimeAscentResources.achievement_amethyst_caves_complete );
		}
		else if (sceneName == SapphireCaveScene)
		{
			GameServicesManager.UnlockAchievement( SlimeAscentResources.achievement_sapphire_cave_complete );
		}
		else if (sceneName == EmeraldCaveScene)
		{
			GameServicesManager.UnlockAchievement( SlimeAscentResources.achievement_emerald_cave_complete );
			GameServicesManager.UnlockAchievement( SlimeAscentResources.achievement_game_complete );
		}
		else if (sceneName == AmetystCaveDeathmatchScene)
		{
			GameServicesManager.SubmitScore( SlimeAscentResources.leaderboard_amethyst_cave_top_height, GetScore() );
		}
		else if (sceneName == SapphireCaveDeathmatchScene)
		{
			GameServicesManager.SubmitScore( SlimeAscentResources.leaderboard_sapphire_cave_top_height, GetScore() );
		}
		else if (sceneName == EmeraldCaveDeathmatchScene)
		{
			GameServicesManager.SubmitScore( SlimeAscentResources.leaderboard_emerald_cave_top_height, GetScore() );
		}
	}
}
