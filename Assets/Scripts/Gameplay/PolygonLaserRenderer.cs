using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonLaserRenderer : MonoBehaviour
{
    Dictionary<int, LaserRenderer> laserRenderers;

    private int setupFrameDelay = 2;
    private bool startSetup = false;

    public Material triangleMat;

    public Material lineMat;
    public float lineWidth;

    public float deactivatedWidth;

    public float maxLength;

    public Texture2D dottedTexture;

    public void CreateLaserRendererList()
    {
        // Start with an empty list
        laserRenderers = new Dictionary<int, LaserRenderer>();
    }

    private void Update()
    {
        if(startSetup)
        {
            if (setupFrameDelay-- <= 0)
            {
                foreach(var laserRenderer in laserRenderers)
                {
                    Vector3 lineStartPoint = laserRenderer.Value.startPosition;
                    Vector3 lineDirection = laserRenderer.Value.directionVector;
                    Vector3 lineEndPoint = (lineDirection * maxLength) + lineStartPoint;
                    laserRenderer.Value.lineRenderer.positionCount = 2;
                    laserRenderer.Value.lineRenderer.SetPosition(0, lineStartPoint);
                    laserRenderer.Value.lineRenderer.SetPosition(1, lineEndPoint);
                }
                startSetup = false;
            }
        }
    }

    public void InitRendererSetup(int index, Vector3 polygonCenter, Vector3 point, Vector3 pointBefore, Vector3 pointAfter, Color color, float fraction)
    {
        SetUpRenderer(index, polygonCenter, point, pointBefore, pointAfter, color, fraction);
        startSetup = true;
    }

    public void SetUpRenderer(int index, Vector3 polygonCenter, Vector3 point, Vector3 pointBefore, Vector3 pointAfter, Color color, float fraction)
    {
        GameObject newTriangle = new GameObject("Laser" + index);
        newTriangle.transform.parent = this.transform;
        newTriangle.transform.localPosition = Vector3.zero;

        // Create renderers
        LaserRenderer laserRenderer = new LaserRenderer();
        laserRenderer.meshRenderer = newTriangle.AddComponent<MeshRenderer>();
        laserRenderer.meshFilter = newTriangle.AddComponent<MeshFilter>();
        laserRenderer.lineRenderer = newTriangle.AddComponent<LineRenderer>();

        // Calculate scaled points
        Vector3 scaledPointBefore = ((pointBefore - point) * fraction) + point;
        Vector3 scaledPointAfter = ((pointAfter - point) * fraction) + point;

        // Create mesh
        Vector3[] vertices = {scaledPointBefore, point, scaledPointAfter};
        Vector3[] normals = { -Vector3.forward, -Vector3.forward, -Vector3.forward};
        int[] tris = { 2, 1, 0 };
        Vector2[] uvs = {new Vector2(0,0), new Vector2(1, 1), new Vector2(0, 1)};

        Mesh triangleMesh = new Mesh();
        triangleMesh.vertices = vertices;
        triangleMesh.normals = normals;
        triangleMesh.uv = uvs;
        triangleMesh.triangles = tris;

        laserRenderer.meshFilter.mesh = triangleMesh;

        laserRenderer.meshRenderer.material = triangleMat;
        laserRenderer.meshRenderer.material.renderQueue = 4000;
        laserRenderer.meshRenderer.material.SetColor("_BaseColor", color);

        // Set up line renderer
        Vector3 lineStartPoint = (scaledPointAfter + scaledPointBefore) / 2.0f;
        Vector3 lineDirection = (lineStartPoint - polygonCenter).normalized;
        Vector3 lineEndPoint = (lineDirection * maxLength) + lineStartPoint;
        //Debug.Log(transform.parent.gameObject.name + "'s " + gameObject.name + "'s direction: " + lineDirection);

        laserRenderer.lineRenderer.material = lineMat;
        laserRenderer.lineRenderer.material.color = color;
        //laserRenderer.lineRenderer.positionCount = 2;
        //laserRenderer.lineRenderer.SetPosition(0, lineStartPoint);
        //laserRenderer.lineRenderer.SetPosition(1, lineEndPoint);
        laserRenderer.directionVector = lineDirection;
        laserRenderer.startPosition = lineStartPoint;
        laserRenderer.lineRenderer.startWidth = laserRenderer.lineRenderer.endWidth = lineWidth;
        laserRenderer.lineRenderer.useWorldSpace = false;

        laserRenderers[index] = laserRenderer;
    }

    public void CutOffLaser(int index, float length)
    {
        laserRenderers[index].lineRenderer.SetPosition(1, laserRenderers[index].startPosition + (laserRenderers[index].directionVector * length));
    }

    public void ResetLaserLength(int index)
    {
        Vector3 newEndPoint = ((laserRenderers[index].lineRenderer.GetPosition(1) - laserRenderers[index].lineRenderer.GetPosition(0)).normalized * maxLength) + laserRenderers[index].lineRenderer.GetPosition(0);
        laserRenderers[index].lineRenderer.SetPosition(1, newEndPoint);
    }

    public void SetLaserActive(bool active)
    {
        foreach(var laser in laserRenderers)
        {
            laserRenderers[laser.Key].lineRenderer.enabled = active;
        }
    }

    public void CrossLaser(int laserIndex, PolygonLaserRenderer otherLaser, int otherIndex)
    {
        // Changes lineRenderer of laserRenderer[laserIndex] to use LevelManager.laserCrossColor
        // and changes the lineRenderer to a dotted line.

        otherLaser.laserRenderers[otherIndex].lineRenderer.material.color = Color.clear;

        laserRenderers[laserIndex].lineRenderer.material.color = LoadLevelManager.laserCrossColor;

        laserRenderers[laserIndex].lineRenderer.material.mainTexture = dottedTexture;
        laserRenderers[laserIndex].lineRenderer.textureMode = LineTextureMode.Tile;
        laserRenderers[laserIndex].lineRenderer.startWidth = laserRenderers[laserIndex].lineRenderer.endWidth = deactivatedWidth;
        float width = laserRenderers[laserIndex].lineRenderer.startWidth;
        laserRenderers[laserIndex].lineRenderer.material.mainTextureScale = new Vector2(1f / width, 1.0f);

    }

    public void UnCrossLaser(int laserIndex)
    {
        // Changes lineRenderer of laserRenderer[laserIndex] to use the original laser color
        // and changes the lineRenderer to a solid, not dotted, line.

        laserRenderers[laserIndex].lineRenderer.material.color = laserRenderers[laserIndex].meshRenderer.material.GetColor("_BaseColor");

        laserRenderers[laserIndex].lineRenderer.material.mainTexture = null;
        laserRenderers[laserIndex].lineRenderer.startWidth = laserRenderers[laserIndex].lineRenderer.endWidth = lineWidth;
        laserRenderers[laserIndex].lineRenderer.textureMode = LineTextureMode.Stretch;
        laserRenderers[laserIndex].lineRenderer.material.mainTextureScale = Vector2.one;
    }

    public void LerpLaserColorsTo(Color colorToLerpTo, float fraction)
    {
        foreach(var laser in laserRenderers)
        {
            Color newColor = Color.Lerp(laser.Value.lineRenderer.material.color, colorToLerpTo, fraction);
            laser.Value.lineRenderer.material.color = newColor;
            laser.Value.meshRenderer.material.SetColor("_BaseColor", newColor);
        }
    }

    public struct LaserRenderer
    {
        public LineRenderer lineRenderer;
        public Vector3 directionVector;
        public Vector3 startPosition;
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
    }
}
