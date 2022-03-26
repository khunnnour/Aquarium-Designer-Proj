using UnityEngine;

public static class PlayerPrefInterface
{
	private const string PREFKEY_TANKDIM_X = "TankWidth";
	private const string PREFKEY_TANKDIM_Y = "TankHeight";
	private const string PREFKEY_TANKDIM_Z = "TankDepth";
	
	private const string PREFKEY_GLASS_THICKNESS = "GlassThick";

	private const string PREFKEY_SUBSTRATE_TYPE = "SubstrateType";
	private const string PREFKEY_SUBSTRATE_THICK = "SubstrateThick";

	private const string PREFKEY_WATER_OFFSET = "WaterOffset";

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

	public static void SetInsideSpecs(float sT, float wO)
	{
		PlayerPrefs.SetFloat(PREFKEY_SUBSTRATE_THICK, sT);

		PlayerPrefs.SetFloat(PREFKEY_WATER_OFFSET, wO);

		SavePreferences();
	}


	// get the level status
	public static Vector3 GetTankDimensions()
	{
		Vector3 tD=new Vector3(
			   PlayerPrefs.GetFloat(PREFKEY_TANKDIM_X),
			   PlayerPrefs.GetFloat(PREFKEY_TANKDIM_Y),
			   PlayerPrefs.GetFloat(PREFKEY_TANKDIM_Z)
		   );
		
		if (tD.sqrMagnitude < 0.1f)
			return Vector3.one*0.2032f;

		return tD;
	}

	public static float GetGlassThickness()
	{
		float gT = PlayerPrefs.GetFloat(PREFKEY_GLASS_THICKNESS);

		if (gT <= 0f)
			return 0.003175f;

		return gT;
	}

	public static float GetSubstrateThickness()
	{
		return PlayerPrefs.GetFloat(PREFKEY_SUBSTRATE_THICK);
	}

	public static float GetWaterOffset()
	{
		return PlayerPrefs.GetFloat(PREFKEY_WATER_OFFSET);
	}


	// save all changes to player prefences to disk
	public static void SavePreferences()
	{
		PlayerPrefs.Save();
	}
}
