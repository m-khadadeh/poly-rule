using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PolygonRenderer : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    private int sides;

    private Vector3[] mainPoints;
    private int[] tris;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uvs;

    private List<LineRenderer> ruleConnections;
    public Material connectionMaterial;
    public float connectionSeparation = 0.1f;
    public float connectionWidth = 0.1f;

    Color mainColor;
    public float tintFraction = 0.25f;

    void Awake()
    {
        ruleConnections = new List<LineRenderer>();
        mainColor = Color.black;
    }

    public void SetColor(Color color)
    {
        meshRenderer.material.SetColor("_BaseColor", color);
        if (mainColor == Color.black) mainColor = color;
    }

    public void TintConnection(bool connected)
    {
        Color newColor = mainColor;
        if(connected)
        {
            newColor = mainColor + ((Color.white - mainColor) * tintFraction);
        }
        meshRenderer.material.SetColor("_BaseColor", newColor);
    }

    public void SetUpMesh(int numPoints, float radius, ref float angleBtwPoints, ref Vector3[] points, List<RuleConnector> connectors, PolygonController polygonController)
    {
        foreach (var connection in connectors)
        {
            GameObject newConnectionObject = new GameObject("Connection " + connection.ToString());
            newConnectionObject.transform.parent = transform;
            newConnectionObject.transform.localPosition = Vector3.zero;
            LineRenderer newLine = newConnectionObject.AddComponent<LineRenderer>();
            newLine.material = connectionMaterial;
            newLine.material.color = connection.ruleColor;
            newLine.useWorldSpace = false;
            newLine.positionCount = numPoints + 1;
            newLine.startWidth = newLine.endWidth = connectionWidth;
            newLine.textureMode = LineTextureMode.RepeatPerSegment;
            newLine.material.mainTextureScale = new Vector2(1.0f / newLine.startWidth, 1.0f);
            ruleConnections.Add(newLine);
            connection.LinkRenderer(polygonController, newLine);

        }

        angleBtwPoints = 360.0f / (float)numPoints;

        sides = numPoints;

        Mesh mesh = new Mesh();

        points = new Vector3[numPoints + 1];
        vertices = new Vector3[numPoints + 1];
        normals = new Vector3[numPoints + 1];
        tris = new int[numPoints * 3];
        uvs = new Vector2[numPoints + 1];
        for (int i = 0; i < numPoints; i++)
        {
            // Points
            Vector3 newPoint = new Vector3();
            float pointAngle = angleBtwPoints * i;
            newPoint.x = radius * Mathf.Cos(pointAngle * Mathf.Deg2Rad);
            newPoint.y = radius * Mathf.Sin(pointAngle * Mathf.Deg2Rad);

            // Rule Connection lasers
            float connectionOffset = connectionSeparation;
            foreach(var connection in ruleConnections)
            {
                connection.SetPosition(i, new Vector3((radius + connectionOffset) * Mathf.Cos(pointAngle * Mathf.Deg2Rad), (radius + connectionOffset) * Mathf.Sin(pointAngle * Mathf.Deg2Rad),0.0f));
                connectionOffset += connectionSeparation;
            }

            newPoint.z = 0;
            vertices[i] = newPoint;
            points[i] = newPoint;

            // Normals
            normals[i] = -Vector3.forward;

            // UV
            Vector2 newUvPoint = new Vector2();
            newUvPoint.x = Mathf.Cos(pointAngle * Mathf.Deg2Rad)/(2 * radius) + 0.5f;
            newUvPoint.y = Mathf.Sin(pointAngle * Mathf.Deg2Rad)/(2 * radius) + 0.5f;
            uvs[i] = newUvPoint;

            // triangles
            tris[3 * i] = numPoints;
            tris[3 * i + 2] = i;
            tris[3 * i + 1] = (i + 1) % numPoints;
        }
        foreach (var connection in ruleConnections)
        {
            connection.SetPosition(numPoints, connection.GetPosition(0));
            
        }

        points[numPoints] = new Vector3(0, 0, 0);
        vertices[numPoints] = new Vector3(0, 0, 0);
        normals[numPoints] = -Vector3.forward;
        uvs[numPoints] = new Vector2(0.5f, 0.5f);

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mainPoints = points;
        meshFilter.mesh = mesh;

        meshRenderer.material.SetInt("_isTriangle", (polygonController.polygonType == PolygonController.PolygonType.Hexagon) ? 0 : 1);
        meshRenderer.material.SetFloat("_Radius", radius);
    }

    public void EditMeshForLaserCap(int index, float fraction)
    {
        Mesh newMesh = new Mesh();

        // Set first point to proper position
        List<Vector3> meshVerts = meshFilter.mesh.vertices.ToList();
        Vector3 pointPrev = ((index - 1 == -1) ? mainPoints[sides - 1] : mainPoints[index - 1]);
        Vector3 pointAfter = mainPoints[(index + 1 ) % sides];

        meshVerts[index] = (((pointPrev - mainPoints[index]) * fraction) + mainPoints[index]);
        
        // Set a new point to be the one after this point and add it to the list
        Vector3 newPoint = ((pointAfter - mainPoints[index]) * fraction) + mainPoints[index];
        meshVerts.Add(newPoint);

        newMesh.vertices = meshVerts.ToArray();

        // Add new uv (double up on the main indexed point's uv) TODO MAYBE: Change this to be proper.
        List<Vector2> meshUvs = meshFilter.mesh.uv.ToList();
        meshUvs[index] = Vector2.Lerp(uvs[index], uvs[(index - 1 == -1) ? sides - 1 : index - 1], fraction);
        meshUvs.Add(Vector2.Lerp(uvs[index], uvs[(index + 1) % sides], fraction));
        newMesh.uv = meshUvs.ToArray();

        // Add new normal;
        List<Vector3> meshNormals = meshFilter.mesh.normals.ToList();
        meshNormals.Add(-Vector3.forward);
        newMesh.normals = meshNormals.ToArray();

        // Create new triangle for the list of tris
        List<int> meshTris = meshFilter.mesh.triangles.ToList();
        meshTris.Add(index);
        meshTris.Add(sides);
        meshTris.Add(meshVerts.Count - 1);
        
        meshTris.Add(meshVerts.Count - 1);
        meshTris.Add(sides);
        meshTris.Add((index + 1) % sides);
        
        newMesh.triangles = meshTris.ToArray();

        meshFilter.mesh = newMesh;
    }

    public void TurnOffConnections()
    {
        foreach(var connection in ruleConnections)
        {
            connection.gameObject.SetActive(false);
        }
    }

    public void LerpColorTo(Color colorToLerpTo, float fraction)
    {
        meshRenderer.material.SetColor("_BaseColor", Color.Lerp(meshRenderer.material.GetColor("_BaseColor"), colorToLerpTo, fraction));
    }
}
