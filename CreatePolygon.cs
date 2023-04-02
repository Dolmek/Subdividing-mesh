using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CreatePolygon : MonoBehaviour
{
    [SerializeField] private int meshSize = 1;
    [SerializeField, Range(0,5) ] private int subdivisions = 0;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    void Start()
    {
        // Add the required components
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        // Define the vertices and triangles of the quad
        vertices[0] = new Vector3(-meshSize / 2f, 0f, -meshSize / 2f);
        vertices[1] = new Vector3(meshSize / 2f, 0f, -meshSize / 2f);
        vertices[2] = new Vector3(-meshSize / 2f, 0f, meshSize / 2f);
        vertices[3] = new Vector3(meshSize / 2f, 0f, meshSize / 2f);

        // Set the Y-component of the vertices to be greater than zero to make the mesh face upward
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, 0.0f, vertices[i].z);
        }

        // Define the UV coordinates
        uv[0] = new Vector2(0f, 0f);
        uv[1] = new Vector2(1f, 0f);
        uv[2] = new Vector2(1f, 1f);
        uv[3] = new Vector2(0f, 1f);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        // Set the mesh properties
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        // Recalculate the normals and bounds of the mesh
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Assign the mesh to the MeshFilter component
        meshFilter.mesh = mesh;

        // Assign the mesh to the MeshCollider component
        meshCollider.sharedMesh = mesh;

        // Print debugging information
        Debug.Log("Vertices: " + mesh.vertices.Length);
        Debug.Log("Triangles: " + mesh.triangles.Length / 3);
        Debug.Log("Connected vertices: " + mesh.vertexCount);
    }
}