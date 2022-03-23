using UnityEngine;

public static class PlayerPrefInterface
{
	private const string PREFKEY_TANKDIM_X = "TankWidth";
	private const string PREFKEY_TANKDIM_Y = "TankHeight";
	private const string PREFKEY_TANKDIM_Z = "TankDepth";
	private const string PREFKEY_GLASS_THICKNESS = "GlassThick";

	/// <summary>
	/// Update the tank specifications
	/// </summary>
	/// <param name="tankDims">The external dimensions of the tank</param>
	/// <param name="gT">The thickness of the glass</param>
	public static void SetTankSpecs(Vector3 tankDims, float gT)
	{
		PlayerPrefs.SetFloat(PREFKEY_TANKDIM_X, tankDims.x);
		PlayerPrefs.SetFloat(PREFKEY_TANKDIM_Y, tankDims.y);
		PlayerPrefs.SetFloat(PREFKEY_TANKDIM_Z, tankDims.z);

		PlayerPrefs.SetFloat(PREFKEY_GLASS_THICKNESS, gT);

		SavePreferences();
	}

	// get the level status
	public static Vector3 GetTankDimensions()
	{
		return new Vector3(
				PlayerPrefs.GetFloat(PREFKEY_TANKDIM_X),
				PlayerPrefs.GetFloat(PREFKEY_TANKDIM_Y),
				PlayerPrefs.GetFloat(PREFKEY_TANKDIM_Z)
			);
	}

	public static float GetGlassThickness()
	{
		return PlayerPrefs.GetFloat(PREFKEY_GLASS_THICKNESS);
	}

	// save all changes to player prefences to disk
	public static void SavePreferences()
	{
		PlayerPrefs.Save();
	}
}
