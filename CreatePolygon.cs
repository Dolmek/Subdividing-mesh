using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CreatePolygon : MonoBehaviour
{

    // Declare a constant for the size of the mesh
    [SerializeField] public const int meshSize = 10;
    [HideInInspector, Range(0, 6)] public int subdivisions = 0; // Number of subdivisions

    // Declare arrays to hold the vertices and triangles for the mesh
    private Vector3[] vertices = new Vector3[4];
    private int[] triangles = new int[6];

    Mesh mesh;

    private MeshFilter meshFilter; // Reference to the MeshFilter component
    private MeshRenderer meshRenderer; // Reference to the MeshRenderer component
    private MeshCollider meshCollider; // Reference to the MeshCollider component

    // Start is called before the first frame update
    void Start()
    {
        // Get the required components from the GameObject this script is attached to
        meshFilter = GetComponent<MeshFilter>(); // Gets the MeshFilter component
        meshRenderer = GetComponent<MeshRenderer>(); // Gets the MeshRenderer component
        meshCollider = GetComponent<MeshCollider>(); // Gets the MeshCollider component

        // Create a new mesh using the CreateMesh() method
        mesh = CreateMesh();

        // If the 'subdivisions' variable is greater than 0, call the Subdivision() method
        if (subdivisions > 0)
        {
            mesh = Subdivision(mesh, subdivisions); // Subdivide the mesh 'subdivisions' number of times
            vertices = mesh.vertices;
            triangles = mesh.triangles; // Update the triangles array with the new triangles
        }

        // Update the vertices and triangles arrays with the new vertices and triangles
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        // Assign the newly created mesh to the MeshFilter component
        meshFilter.mesh = mesh;

        // Assign the newly created mesh to the MeshCollider component
        meshCollider.sharedMesh = mesh;

        // Create a Unity Standard Material and assign it to the mesh renderer
        Material material = new Material(Shader.Find("Standard"));
        meshRenderer.material = material;

        // Print debugging information about the mesh
        //Debug.LogFormat("Vertices: {0}", mesh.vertices.Length); // Print the number of vertices in the mesh
        //Debug.LogFormat("Triangles: {0}", mesh.triangles.Length / 3); // Print the number of triangles in the mesh
        //Debug.LogFormat("Connected vertices: {0}", mesh.vertexCount); // Print the number of connected vertices in the mesh
    }

    public Mesh CreateChunk(int externalSubdivision) 
    {

        // Get the required components from the GameObject this script is attached to
        meshFilter = GetComponent<MeshFilter>(); // Gets the MeshFilter component
        meshRenderer = GetComponent<MeshRenderer>(); // Gets the MeshRenderer component
        meshCollider = GetComponent<MeshCollider>(); // Gets the MeshCollider component

        // Create a new mesh using the CreateMesh() method
        mesh = CreateMesh();

        // If the 'subdivisions' variable is greater than 0, call the Subdivision() method
        if (subdivisions > 0)
        {
            mesh = Subdivision(mesh, externalSubdivision); // Subdivide the mesh 'subdivisions' number of times
            vertices = mesh.vertices;
            triangles = mesh.triangles; // Update the triangles array with the new triangles
        }

        // Update the vertices and triangles arrays with the new vertices and triangles
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        // Assign the newly created mesh to the MeshFilter component
        meshFilter.mesh = mesh;

        // Assign the newly created mesh to the MeshCollider component
        meshCollider.sharedMesh = mesh;

        // Print debugging information about the mesh
        //Debug.LogFormat("Vertices: {0}", mesh.vertices.Length); // Print the number of vertices in the mesh
        //Debug.LogFormat("Triangles: {0}", mesh.triangles.Length / 3); // Print the number of triangles in the mesh
        //Debug.LogFormat("Connected vertices: {0}", mesh.vertexCount); // Print the number of connected vertices in the mesh

        return mesh;
    }

    // Function to create a new mesh
    public Mesh CreateMesh()
    {
        // Create a new Mesh object to hold the mesh data
        Mesh mesh = new Mesh();

        //vertices order (0,0), (0,1), (1,0), (1,1)
        // Define the vertices of the mesh
        // First vertex
        Vector3 vert00 = new Vector3(-meshSize / 2f, 0f, -meshSize / 2f);

        // Second vertex
        Vector3 vert01 = new Vector3(-meshSize / 2f, 0f, meshSize / 2f);

        // Third vertex
        Vector3 vert10 = new Vector3(meshSize / 2f, 0f, -meshSize / 2f);

        // Fourth vertex
        Vector3 vert11 = new Vector3(meshSize / 2f, 0f, meshSize / 2f);

        vertices = new Vector3[] 
        { 
            vert00,
            vert01,
            vert10,
            vert11
        };

        //trinagles order  0, 1, 2   1, 3, 2   OR   0, 1, 2   2, 1, 3

        // Define the triangles of the mesh
        triangles = new int[]
        {
            0, 1, 2,
            1, 3, 2
        };

        // Assign the vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Calculate and assign the normals for the mesh
        mesh.normals = CalculateNormals(vertices, triangles);

        // Calculate the bounds of the mesh
        Vector3 minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < vertices.Length; i++)
        {
            minBounds = Vector3.Min(minBounds, vertices[i]);
            maxBounds = Vector3.Max(maxBounds, vertices[i]);
        }
        Vector3 meshBoundsCenter = (maxBounds + minBounds) / 2f;
        Vector3 meshBoundsSize = maxBounds - minBounds;
        Bounds meshBounds = new Bounds(meshBoundsCenter, meshBoundsSize);

        // Set the bounds of the mesh
        mesh.bounds = meshBounds;

        // Return the finished mesh
        return mesh;
    }

    // Calculates the surface normals for a mesh with the given vertices and triangles.
    // Returns an array of normals, where each element corresponds to a vertex in the mesh.
    // Function to calculate the normals of the mesh based on its vertices and triangles
    // Calculate the normals of the mesh
    Vector3[] CalculateNormals(Vector3[] vertices, int[] triangles)
    {
        // Create a new array to hold the normals
        Vector3[] normals = new Vector3[vertices.Length];

        // Loop through each triangle in the mesh
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Get the three vertices of the current triangle
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            // Calculate the normal of the current triangle
            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            // Add the normal to the normals of each vertex in the triangle
            normals[triangles[i]] += normal;
            normals[triangles[i + 1]] += normal;
            normals[triangles[i + 2]] += normal;
        }

        // Normalize the normals
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = normals[i].normalized;
        }

        // Return the normals
        return normals;
    }

    private Mesh Subdivision(Mesh mesh, int subdivisions)
    {
        // Initialize the new mesh with the original vertices and triangles
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;

        // Loop through each subdivision level
        for (int level = 0; level < subdivisions; level++)
        {
            // Create lists to store the new vertices and triangles
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();

            // Loop through each face in the mesh
            for (int faceIndex = 0; faceIndex < newMesh.triangles.Length; faceIndex += 3)
            {
                // Get the three vertices that make up the face
                Vector3 v0 = newMesh.vertices[newMesh.triangles[faceIndex]];
                Vector3 v1 = newMesh.vertices[newMesh.triangles[faceIndex + 1]];
                Vector3 v2 = newMesh.vertices[newMesh.triangles[faceIndex + 2]];

                // Calculate the midpoints of each edge
                Vector3 m01 = (v0 + v1) / 2f;
                Vector3 m12 = (v1 + v2) / 2f;
                Vector3 m20 = (v2 + v0) / 2f;

                // Add the new vertices to the list of vertices
                int i0 = newVertices.Count;
                newVertices.Add(v0);
                int i1 = newVertices.Count;
                newVertices.Add(v1);
                int i2 = newVertices.Count;
                newVertices.Add(v2);
                int i01 = newVertices.Count;
                newVertices.Add(m01);
                int i12 = newVertices.Count;
                newVertices.Add(m12);
                int i20 = newVertices.Count;
                newVertices.Add(m20);

                // Add the new triangles to the list of triangles
                newTriangles.Add(i0);
                newTriangles.Add(i01);
                newTriangles.Add(i20);

                newTriangles.Add(i01);
                newTriangles.Add(i1);
                newTriangles.Add(i12);

                newTriangles.Add(i20);
                newTriangles.Add(i12);
                newTriangles.Add(i2);

                newTriangles.Add(i01);
                newTriangles.Add(i12);
                newTriangles.Add(i20);
            }

            // Update the new mesh with the new vertices and triangles
            newMesh.vertices = newVertices.ToArray();
            newMesh.triangles = newTriangles.ToArray();
            newMesh.normals = CalculateNormals(newMesh.vertices, newMesh.triangles);
        }

        return newMesh;
    }
}
