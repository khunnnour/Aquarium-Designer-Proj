using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public int points = 6;
    public int width = 1;
    public int height = 1;


    private Mesh mesh;
    private Vector3[] vertices;

    public void Start()
    {
        Generate();
    }

    private void Generate()
    {
        WaitForSeconds wait = new WaitForSeconds(0.05f);
        
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";
        
        // Generate the vertices
        vertices = new Vector3[(width + 1) * (height + 1)];
        for (int y = 0, i=0; y < height; y++)
        {
            for (int x = 0; x < width; x++,i++)
            {
                vertices[i] = new Vector3(x, y, 0);
            }
        }
        mesh.vertices = vertices;
        
        // Generate the triangles
        int[] triangles = new int[width * height * 6];
        for (int ti = 0, vi = 0, y = 0; y < height; y++, vi++) {
            for (int x = 0; x < width; x++, ti += 6, vi++) {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
                triangles[ti + 5] = vi + width + 2;
            }
        }
        mesh.triangles = triangles;
        
        // Recalculate normals
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (vertices == null) 
            return;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}
