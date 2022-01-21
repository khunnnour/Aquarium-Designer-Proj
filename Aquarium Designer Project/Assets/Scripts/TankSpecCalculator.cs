using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankSpecCalculator : MonoBehaviour
{
    // 5ft2 / gal
    private const float BSA_PER_GAL = 0.464515f; // m2 / gal
    private const float GLASS_TENSILE_STR = 19f; // MPa
    private const float GLASS_MOD_ELASTICITY = 69000f; // MPa
    private const float ACRYLIC_TENSILE_STR = 64.8f; // MPa
    private const float ACRYLIC_MOD_ELASTICITY = 2760f; // MPa


    /// <summary>
    /// Calculates the safety factor of the provided tank
    /// </summary>
    /// <param name="tankDims">Ext. dimensions of the tank (m)</param>
    /// <param name="thick">Thickness of tank material (m)</param>
    /// <param name="acrylic">You are using acrylic instead of glass</param>
    /// <returns>Safety factor (dimensionless)</returns>
    public static float CalculateSafetyFactor(Vector3 tankDims, float thick, bool acrylic)
    {
        // calc L/H ratio; clamped between 0.5 and 3
        float lhRatio = Mathf.Max(0.5f, Mathf.Min(tankDims.x / tankDims.y, 3f));
        // calculate beta value for glass safety factor
        float beta = -0.00474f + 0.17064f * lhRatio + 0.01594f * lhRatio * lhRatio -
                     0.01049f * lhRatio * lhRatio * lhRatio;

        // convert to mm
        float t = thick * 1000f; // thickness
        float h = tankDims.y * 1000f; // height of tank

        // calculate the safety factor ---
        if (acrylic)
            return (100000 * ACRYLIC_TENSILE_STR * t * t) / (beta * h * h * h);
        else
            return (100000 * GLASS_TENSILE_STR * t * t) / (beta * h * h * h);
    }

    /// <summary>
    /// Calculates the deflection of the provided tank
    /// </summary>
    /// <param name="tankDims">Ext. dimensions of the tank (m)</param>
    /// <param name="thick">Thickness of tank material (m)</param>
    /// <param name="acrylic">You are using acrylic instead of glass</param>
    /// <returns>Deflection (mm)</returns>
    public static float CalculateDeflection(Vector3 tankDims, float thick, bool acrylic)
    {
        // Deflection = (alpha x Water Pressure (p) x 0.000001 x Height^4) / (Modulus of Elasticity (E) x Thickness^3)
        //      water pressure (p) = Height * 9.81

        // calc L/H ratio; clamped between 0.5 and 3
        float lhRatio = Mathf.Max(0.5f, Mathf.Min(tankDims.x / tankDims.y, 3f));
        // calculate beta value for glass safety factor
        float alpha = -0.0199f + 0.0445f * lhRatio + 0.0001f * lhRatio * lhRatio -
                      0.0018f * lhRatio * lhRatio * lhRatio;

        // convert to mm
        float t = thick * 1000f; // thickness
        float h = tankDims.y * 1000f; // height of tank
        float p = h * 9.81f;

        // calculate the safety factor ---
        if (acrylic)
            return alpha * p * 0.000001f * Mathf.Pow(h, 4) / (ACRYLIC_MOD_ELASTICITY * Mathf.Pow(t, 3));
        else
            return alpha * p * 0.000001f * Mathf.Pow(h, 4) / (GLASS_MOD_ELASTICITY * Mathf.Pow(t, 3));
    }
}
