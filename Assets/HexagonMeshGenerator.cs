using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HexagonMeshGenerator : MonoBehaviour
{
    [Range(0.1f, 10f)]
    public float size = 1f;
    
    // If you want to adjust the height of the hexagon
    public float height = 0.1f;
    
    // Set this to true to regenerate the mesh
    public bool regenerateMesh = false;

    private Mesh hexMesh;

    private void Awake()
    {
        GenerateHexagonMesh();
    }

    private void OnValidate()
    {
        if (regenerateMesh)
        {
            GenerateHexagonMesh();
            regenerateMesh = false;
        }
    }

    public void GenerateHexagonMesh()
    {
        // Create a new mesh or clear the existing one
        if (hexMesh == null)
        {
            hexMesh = new Mesh();
            hexMesh.name = "HexagonMesh";
            GetComponent<MeshFilter>().mesh = hexMesh;
        }
        else
        {
            hexMesh.Clear();
        }

        // Create vertices for a regular hexagon
        Vector3[] vertices = new Vector3[7]; // 6 corners + center
        
        // Center vertex
        vertices[0] = Vector3.zero;
        
        // Create the 6 corners of the hexagon
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI / 3f; // 60 degrees in radians
            float x = size * Mathf.Cos(angle);
            float z = size * Mathf.Sin(angle);
            vertices[i + 1] = new Vector3(x, 0, z);
        }

        // Create triangles (indices)
        int[] triangles = new int[18]; // 6 triangles, 3 indices each
        
        // Create 6 triangles from center to each edge
        for (int i = 0; i < 6; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex] = 0; // center
            triangles[triIndex + 1] = i + 1; // current corner
            triangles[triIndex + 2] = i < 5 ? i + 2 : 1; // next corner (wrap back to 1 for last triangle)
        }

        // Create UVs
        Vector2[] uvs = new Vector2[7];
        uvs[0] = new Vector2(0.5f, 0.5f); // Center
        
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI / 3f;
            float u = 0.5f + 0.5f * Mathf.Cos(angle);
            float v = 0.5f + 0.5f * Mathf.Sin(angle);
            uvs[i + 1] = new Vector2(u, v);
        }

        // Assign values to the mesh
        hexMesh.vertices = vertices;
        hexMesh.triangles = triangles;
        hexMesh.uv = uvs;
        
        // Recalculate normals and bounds
        hexMesh.RecalculateNormals();
        hexMesh.RecalculateBounds();
    }

    public float GetSize()
    {
        return size;
    }

    public Mesh GetMesh()
    {
        return hexMesh;
    }
}