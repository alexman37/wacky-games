using UnityEngine;

/// <summary>
/// Calculates the current face value of a dice based on its orientation.
/// </summary>
public static class DiceValueCalculator
{
    // Normal vectors for each face of the dice
    // These vectors point outward from the center of the dice
    // Because of how I drew the dots, the face values are as follows:
    private static readonly Vector3[] faceNormals = new Vector3[6]
    {
        Vector3.up,      // 4 (Y+)
        Vector3.right,   // 6 (X+)
        Vector3.forward, // 5 (Z+)
        Vector3.back,    // 2 (Z-)
        Vector3.left,    // 3 (X-)
        Vector3.down     // 1 (Y-)
    };
    // Note that a standard dice goes 1,2,3,4,5,6, but I drew it differently (wrong)

    private static readonly int[] faceValues = new int[6] { 4, 6, 5, 2, 3, 1 };

    public static int GetTopFaceValue(GameObject dice)
    {
        if (dice == null) return -1;

        // The up vector in world space
        Vector3 worldUp = Vector3.up;

        // Get the rotation of the dice
        // This accounts for if the rotation spawns differently.
        Quaternion diceRotation = dice.transform.rotation;

        // Find which face normal has the highest dot product with world up
        int topFaceIndex = 0;
        float highestDotProduct = -1f; // Start with lowest possible value

        for (int i = 0; i < faceNormals.Length; i++)
        {
            // Transform the face normal from local dice space to world space
            Vector3 faceNormalWorld = diceRotation * faceNormals[i];

            // Calculate dot product with world up
            float dotProduct = Vector3.Dot(faceNormalWorld, worldUp);

            // If this face is more aligned with up than previous faces
            if (dotProduct > highestDotProduct)
            {
                highestDotProduct = dotProduct;
                topFaceIndex = i;
            }
        }

        return faceValues[topFaceIndex];
    }
}