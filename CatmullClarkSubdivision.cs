using System.Collections.Generic;
using UnityEngine;

public class CatmullClarkSubdivision : MonoBehaviour
{
    private static int GetEdgeIndex(int v1, int v2)
    {
        /**
            * Returns a unique integer index for the edge connecting two vertices.
            * The index is determined by the order of the vertices: if v1 is less than
            * v2, the index is computed as v1 * 65536 + v2, otherwise it is computed as
            * v2 * 65536 + v1.
        *
            * @param v1 the index of the first vertex
            * @param v2 the index of the second vertex
            * @return the index of the edge connecting the two vertices
        */
        return v1 < v2 ? v1 * 65536 + v2 : v2 * 65536 + v1;
    }

    private static Vector3 GetEdgeVertex(Vector3 v1, Vector3 v2)
    {
        // This function returns the midpoint of the edge between v1 and v2
        return (v1 + v2) / 2;
    }

    private static Vector3 GetFaceVertex(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // This function returns the average of the three input vectors
        return (v1 + v2 + v3) / 3;
    }

    private static List<int> GetConnectedEdges(int vertexIndex, Mesh mesh)
    {
        // This function returns a list of edge indices connected to the vertex
        List<int> connectedEdges = new List<int>();
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                if (triangles[j] == vertexIndex || triangles[j + 1] == vertexIndex || triangles[j + 2] == vertexIndex)
                {
                    int e1 = GetEdgeIndex(triangles[j], triangles[j + 1]);
                    int e2 = GetEdgeIndex(triangles[j + 1], triangles[j + 2]);
                    int e3 = GetEdgeIndex(triangles[j + 2], triangles[j]);
                    if (!connectedEdges.Contains(e1))
                    {
                        connectedEdges.Add(e1);
                    }
                    if (!connectedEdges.Contains(e2))
                    {
                        connectedEdges.Add(e2);
                    }
                    if (!connectedEdges.Contains(e3))
                    {
                        connectedEdges.Add(e3);
                    }
                }
            }
        }
        return connectedEdges;
    }

    private static float GetAlpha(int n)
    {
        // This function returns the alpha value based on the number of edges connected to a vertex
        return 1.0f / n * ((4.0f / 5.0f) - (Mathf.Pow(3.0f + 2.0f * Mathf.Cos(2.0f * Mathf.PI / n), 2.0f) / 20.0f));
    }

    private static Vector3 GetFaceCentroid(int faceIndex, Mesh mesh)
    {
        // This function returns the centroid of a face
        Vector3 v1 = mesh.vertices[mesh.triangles[faceIndex * 3]];
        Vector3 v2 = mesh.vertices[mesh.triangles[faceIndex * 3 + 1]];
        Vector3 v3 = mesh.vertices[mesh.triangles[faceIndex * 3 + 2]];
        return (v1 + v2 + v3) / 3.0f;
    }


    // Create the initial quad mesh
    public static void Subdivide(Mesh mesh, int subdivisions)
    {
        for (int i = 0; i < subdivisions; i++)
        {
            Dictionary<int, Vector3> newEdgeVertices = FindNewEdgeVertices(mesh);
            Dictionary<int, Vector3> newFaceVertices = FindNewFaceVertices(mesh);
            Dictionary<int, Vector3> newVertices = FindNewVertices(mesh, newEdgeVertices, newFaceVertices);
            UpdateMesh(mesh, newVertices);
            //Step 5: Recalculate normals and tangents
            mesh.normals = CalculateNormals(mesh);
            mesh.tangents = CalculateTangents(mesh);
            UpdateUVs(mesh);
            CleanUpVertices(mesh);
        }
    }

    // Step 1: Find the new edge vertices
    private static Dictionary<int, Vector3> FindNewEdgeVertices(Mesh mesh)
    {
        Dictionary<int, Vector3> newEdgeVertices = new Dictionary<int, Vector3>();
        for (int j = 0; j < mesh.subMeshCount; j++)
        {
            int[] triangles = mesh.GetTriangles(j);
            for (int k = 0; k < triangles.Length; k += 3)
            {
                int v1 = triangles[k];
                int v2 = triangles[k + 1];
                int v3 = triangles[k + 2];

                int e1 = GetEdgeIndex(v1, v2);
                int e2 = GetEdgeIndex(v2, v3);
                int e3 = GetEdgeIndex(v3, v1);

                Vector3 edgeVertex1 = GetEdgeVertex(mesh.vertices[v1], mesh.vertices[v2]);
                Vector3 edgeVertex2 = GetEdgeVertex(mesh.vertices[v2], mesh.vertices[v3]);
                Vector3 edgeVertex3 = GetEdgeVertex(mesh.vertices[v3], mesh.vertices[v1]);

                if (!newEdgeVertices.ContainsKey(e1))
                {
                    newEdgeVertices.Add(e1, edgeVertex1);
                }

                if (!newEdgeVertices.ContainsKey(e2))
                {
                    newEdgeVertices.Add(e2, edgeVertex2);
                }

                if (!newEdgeVertices.ContainsKey(e3))
                {
                    newEdgeVertices.Add(e3, edgeVertex3);
                }
            }
        }
        return newEdgeVertices;
    }

    // Step 2: Find the new vertices for each face
    private static Dictionary<int, Vector3> FindNewFaceVertices(Mesh mesh)
    {
        Dictionary<int, Vector3> newFaceVertices = new Dictionary<int, Vector3>();
        for (int j = 0; j < mesh.triangles.Length; j += 3)
        {
            int v1 = mesh.triangles[j];
            int v2 = mesh.triangles[j + 1];
            int v3 = mesh.triangles[j + 2];

            Vector3 faceVertex = GetFaceVertex(mesh.vertices[v1], mesh.vertices[v2], mesh.vertices[v3]);

            newFaceVertices.Add(v1, faceVertex);
            newFaceVertices.Add(v2, faceVertex);
            newFaceVertices.Add(v3, faceVertex);
        }
        return newFaceVertices;
    }

    private static Dictionary<int, Vector3> FindNewVertices(Mesh mesh, Dictionary<int, Vector3> newEdgeVertices, Dictionary<int, Vector3> newFaceVertices)
    {
        Dictionary<int, Vector3> newVertices = new Dictionary<int, Vector3>();
        for (int j = 0; j < mesh.vertices.Length; j++)
        {
            List<int> connectedEdges = GetConnectedEdges(j, mesh);
            List<int> connectedFaces = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] triangles = mesh.GetTriangles(i);
                for (int k = 0; k < triangles.Length; k += 3)
                {
                    if (triangles[k] == j || triangles[k + 1] == j || triangles[k + 2] == j)
                    {
                        connectedFaces.Add(k / 3);
                    }
                }
            }

            int n = connectedEdges.Count;
            float alpha = GetAlpha(n);

            Vector3 Q = alpha * mesh.vertices[j];

            foreach (int e in connectedEdges)
            {
                if (newEdgeVertices.TryGetValue(e, out Vector3 ev))
                {
                    Q += (1 - alpha) * 0.5f * (mesh.vertices[GetEdgeIndex(e, 0)] + mesh.vertices[GetEdgeIndex(e, 1)]) + alpha * ev;
                }
                else
                {
                    Q += (1 - alpha) * 0.5f * (mesh.vertices[GetEdgeIndex(e, 0)] + mesh.vertices[GetEdgeIndex(e, 1)]);
                }
            }

            foreach (int f in connectedFaces)
            {
                if (new FaceVertices.TryGetValue(f, out Vector3 fv))
                {
                    Q += (1 - alpha) * (1.0f / connectedFaces.Count) * fv;
                }
                else
                {
                    int[] triangles = mesh.GetTriangles(0);
                    Vector3 v1 = mesh.vertices[triangles[f * 3]];
                    Vector3 v2 = mesh.vertices[triangles[f * 3 + 1]];
                    Vector3 v3 = mesh.vertices[triangles[f * 3 + 2]];
                    Vector3 faceCenter = (v1 + v2 + v3) / 3.0f;
                    Q += (1 - alpha) * (1.0f / connectedFaces.Count) * faceCenter;
                }
            }
            newVertices.Add(j, Q);
        }
        return newVertices;
    }


    // Step 4: Update the mesh
    private static void UpdateMesh(Mesh mesh, Dictionary<int, Vector3> newVertices)
    {
        // Update vertices
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (newVertices.TryGetValue(i, out Vector3 vertex))
            {
                mesh.vertices[i] = vertex;
            }
        }

        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // Step 5: Recalculate normals and tangents
    private static Vector3[] CalculateNormals(Mesh mesh)
    {
        Vector3[] normals = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                Vector3 v1 = mesh.vertices[triangles[j]];
                Vector3 v2 = mesh.vertices[triangles[j + 1]];
                Vector3 v3 = mesh.vertices[triangles[j + 2]];

                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

                normals[triangles[j]] += normal;
                normals[triangles[j + 1]] += normal;
                normals[triangles[j + 2]] += normal;
            }
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        return normals;
    }

    private static Vector4[] CalculateTangents(Mesh mesh)
    {
        Vector4[] tangents = new Vector4[mesh.vertices.Length];
        Vector3[] normals = mesh.normals;

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                Vector3 v1 = mesh.vertices[triangles[j]];
                Vector3 v2 = mesh.vertices[triangles[j + 1]];
                Vector3 v3 = mesh.vertices[triangles[j + 2]];

                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

                Vector2 uv1 = mesh.uv[triangles[j]];
                Vector2 uv2 = mesh.uv[triangles[j + 1]];
                Vector2 uv3 = mesh.uv[triangles[j + 2]];

                Vector3 deltaPos1 = v2 - v1;
                Vector3 deltaPos2 = v3 - v1;

                Vector2 deltaUV1 = uv2 - uv1;
                Vector2 deltaUV2 = uv3 - uv1;

                float r = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV1.y * deltaUV2.x);
                Vector4 tangent = (deltaPos1 * deltaUV2.y - deltaPos2 * deltaUV1.y) * r;
                tangent.w = 0.0f;

                tangents[triangles[j]] += tangent;
                tangents[triangles[j + 1]] += tangent;
                tangents[triangles[j + 2]] += tangent;
            }
        }

        for (int i = 0; i < tangents.Length; i++)
        {
            Vector3 n = normals[i];
            Vector3 t = tangents[i];
            tangents[i] = new Vector4(t.x - n.x * Vector3.Dot(n, t),
                                      t.y - n.y * Vector3.Dot(n, t),
                                      t.z - n.z * Vector3.Dot(n, t),
                                      tangents[i].w).normalized;
        }

        return tangents;
    }

    //Step 6: Update the mesh's triangles
    // Step 6: Update UVs
    private static void UpdateUVs(Mesh mesh)
    {
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                Vector2 uv1 = mesh.uv[triangles[j]];
                Vector2 uv2 = mesh.uv[triangles[j + 1]];
                Vector2 uv3 = mesh.uv[triangles[j + 2]];

                Vector2 newUV1 = (uv1 + uv2) / 2f;
                Vector2 newUV2 = (uv2 + uv3) / 2f;
                Vector2 newUV3 = (uv3 + uv1) / 2f;

                uvs[triangles[j]] = newUV1;
                uvs[triangles[j + 1]] = newUV2;
                uvs[triangles[j + 2]] = newUV3;
            }
        }

        mesh.uv = uvs;
    }
    //Step 7: Update UVs and other mesh data (optional)

    //Step 8: Clean up
    //newEdgeVertices.Clear();
    //        newFaceVertices.Clear();
    //        newVertices.Clear();
    // Step 6: Clean up extra vertices
    private static void CleanUpVertices(Mesh mesh)
    {
        // Step 6.1: Remove extra edge vertices
        int[] triangles = mesh.triangles;
        int[] newTriangles = new int[triangles.Length];
        int triangleIndex = 0;
        Dictionary<int, int> edgeMap = new Dictionary<int, int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int v1 = triangles[i + j];
                int v2 = triangles[i + (j + 1) % 3];
                int edgeIndex = GetEdgeIndex(v1, v2);
                int edgeValue;
                if (!edgeMap.TryGetValue(edgeIndex, out edgeValue))
                {
                    edgeValue = mesh.vertices.Length;
                    edgeMap.Add(edgeIndex, edgeValue);
                    mesh.vertices = ResizeArray(mesh.vertices, edgeValue + 1);
                    mesh.vertices[edgeValue] = GetEdgeVertex(mesh.vertices[v1], mesh.vertices[v2]);
                }
                newTriangles[triangleIndex + j] = v1;
                newTriangles[triangleIndex + 3 + j] = edgeValue;
            }
            newTriangles[triangleIndex + 6] = newTriangles[triangleIndex + 3];
            triangleIndex += 9;
        }
        mesh.triangles = newTriangles;

        // Step 6.2: Remove extra face vertices
        int[] vertexMap = new int[mesh.vertices.Length];
        int newIndex = 0;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (mesh.normals[i] != Vector3.zero)
            {
                mesh.vertices[newIndex] = mesh.vertices[i];
                mesh.normals[newIndex] = mesh.normals[i];
                mesh.tangents[newIndex] = mesh.tangents[i];
                vertexMap[i] = newIndex;
                newIndex++;
            }
        }
        mesh.vertices = ResizeArray(mesh.vertices, newIndex);
        mesh.normals = ResizeArray(mesh.normals, newIndex);
        mesh.tangents = ResizeArray(mesh.tangents, newIndex);
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            mesh.triangles[i] = vertexMap[mesh.triangles[i]];
        }

        // Step 6.3: Remove extra vertices
        mesh.vertices = RemoveUnusedVertices(mesh.vertices, mesh.triangles);
    }

    // Helper method to resize an array
    private static T[] ResizeArray<T>(T[] original, int size)
    {
        T[] newArray = new T[size];
        int elementsToCopy = Mathf.Min(original.Length, size);
        for (int i = 0; i < elementsToCopy; i++)
        {
            newArray[i] = original[i];
        }
        return newArray;
    }

    // Helper method to remove unused vertices from a mesh
    private static Vector3[] RemoveUnusedVertices(Vector3[] vertices, int[] triangles)
    {
        bool[] usedVertices = new bool[vertices.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            usedVertices[triangles[i]] = true;
        }
        int newIndex = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (usedVertices[i])
            {
                vertices[newIndex] = vertices[i];
                newIndex++;
            }
        }
        return ResizeArray(vertices, newIndex);
    }


}