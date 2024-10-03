using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PolygonController : MonoBehaviour
{
    public enum PolygonType
    {
        Hexagon, Triangle, UpsideDownTriangle
    }

    public enum Rule
    {
        RotateCCW, RotateCW, FlipHorizontal, FlipVertical, None
    }

    [Header("Level Designer Variables")]
    public Vector2 triangularPosition;
    public Rule rule;
    public List<LaserInfo> laserInfo;
    public List<RuleConnector> ruleConnectors; 

    [Header("Game Developer Variables")]
    public PolygonRenderer polyRenderer;
    public PolygonCollider2D polygonCollider;

    public PolygonType polygonType;

    private int numPoints;
    
    public float ruleDuration;
    public float destroyDuration;

    private float angleBtwPoints;
    public Vector3[] points;
    private bool flipped;

    private List<Transform> pointTransforms;

    [Range(0.0f, 0.5f)]
    public float capFraction;
    [Range(0.0f, 0.5f)]
    public float gutterFraction;

    public PolygonLaserRenderer laserRenderer;

    public Vector2[] pointOffsets;

    private Dictionary<int, LaserConnection> laserConnected;
    private Dictionary<int, Vector2> laserTriangularCoords;

    public float startRotationOffset = 90;

    public List<SpriteRenderer> sprites;
    public Sprite[] icons;

    public LayerMask polygonLayer;

    private int gizmoRadius = 1;

    private HashSet<PolygonController> visitedPolygons;
    bool isChainRoot;

    public GameObject explosionParticles;
    public bool isBeingDestroyed;

    public bool wasSelectedToApply;

    private void Awake()
    {
        hoverState = false;
    }

    // Start is called before the first frame update
    public void InitPolygon()
    {
        numPoints = (polygonType == PolygonType.Hexagon) ? 6 : 3;
        polyRenderer.SetUpMesh(numPoints, TriangularCoordinates.instance.radius, ref angleBtwPoints, ref points, ruleConnectors, this);
        CreateCollider();
        gameObject.layer = 6; // Polygon Layer
        flipped = false;

        laserRenderer.CreateLaserRendererList();

        laserConnected = new Dictionary<int, LaserConnection>();
        for(int i = 0; i < laserInfo.Count; i++)
        {
            // Set up laser renderers
            laserRenderer.InitRendererSetup(laserInfo[i].pointIndex,
                points[points.Length - 1], 
                points[laserInfo[i].pointIndex],
                points[((laserInfo[i].pointIndex - 1) == -1) ? (points.Length - 2) : (laserInfo[i].pointIndex - 1)],
                points[(laserInfo[i].pointIndex + 1) % (points.Length - 1)],
                laserInfo[i].colorData.laserColor,
                capFraction);
            polyRenderer.EditMeshForLaserCap(laserInfo[i].pointIndex, capFraction + gutterFraction);
            LaserConnection laserConnection = new LaserConnection();
            laserConnection.colorData = laserInfo[i].colorData;
            laserConnection.connectedPolygon = null;
            laserConnection.correctColors = false;
            laserConnection.crossingIndex = -1; // This means it isn't crossing a laser of an incorrect color
            laserConnected[laserInfo[i].pointIndex] = laserConnection;
        }

        this.transform.eulerAngles = Vector3.forward * (startRotationOffset + ((polygonType == PolygonType.UpsideDownTriangle) ? 180 : 0));

        // Set up positions using triangular coordinates
        laserTriangularCoords = new Dictionary<int, Vector2>();
        this.transform.position = TriangularCoordinates.instance.triangleToEuclidean((int)triangularPosition.x, (int)triangularPosition.y);
        foreach(var laser in laserInfo)
        {
            laserTriangularCoords[laser.pointIndex] = triangularPosition + pointOffsets[laser.pointIndex];
            //Debug.Log(polygonType + " at " + triangularPosition + ": " + laserConnected[laser.pointIndex].colorData.name + " laser at index " + laser.pointIndex + " is at position " + laserTriangularCoords[laser.pointIndex]);
        }

        // Place a transform on each point
        pointTransforms = new List<Transform>();

        GameObject pointParent = new GameObject("Point Transforms");
        pointParent.transform.parent = this.transform;
        pointParent.transform.localPosition = Vector3.zero;

        for (int i = 0; i < points.Length - 1; i++)
        {
            GameObject newPoint = new GameObject("Point " + i);
            newPoint.transform.parent = pointParent.transform;
            Vector3 newPointTriPos = triangularPosition + pointOffsets[i];
            newPoint.transform.position = TriangularCoordinates.instance.triangleToEuclidean((int)newPointTriPos.x, (int)newPointTriPos.y);
            pointTransforms.Add(newPoint.transform);
        }

        // Set Up Sprites
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.transform.localRotation = Quaternion.AngleAxis(-startRotationOffset, Vector3.forward);
            if(rule != Rule.None)
            {
                sprite.sprite = icons[(int)rule];
                sprite.color = Color.white;
            }
            else
            {
                sprite.color = Color.clear;
            }
            
            // Fixes alignment of vertical flip rule
            if(polygonType != PolygonType.Hexagon && rule == Rule.FlipVertical)
            {
                sprite.transform.position += Vector3.up * (TriangularCoordinates.instance.radius / 8.0f) * ((polygonType == PolygonType.UpsideDownTriangle) ? -1 : 1);
            }

            if (polygonType == PolygonType.UpsideDownTriangle) sprite.transform.Rotate(Vector3.forward, 180);

            sprite.material.renderQueue = 5000;
        }

        isChainRoot = false;

        foreach(var connector in ruleConnectors)
        {
            connector.Subscribe(this);
        }

        isBeingDestroyed = false;

        wasSelectedToApply = false;

        LoadLevelManager.PolygonSetupComplete(this);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLaserPhysicalCollisions();
    }

    public void UpdateLaserPhysicalCollisions(bool storeData = false)
    {
        //Debug.Log("Updating collisions on " + gameObject.name);
        foreach(var laser in laserInfo)
        {
            Vector3 direction = (pointTransforms[laser.pointIndex].position - this.transform.position).normalized;
            Vector3 laserStart = transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(laserStart, direction, Mathf.Infinity, polygonLayer);
            Debug.DrawLine(laserStart, laserStart + direction);
            // hits[0] is the emitting polygon. if hits[1] exists, that's the first polygon being hit
            //Debug.Log(gameObject.name + "'s " + laser.colorData.name + " laser is hitting " + (hits.Length) + " objects");
            if (hits.Length > 1)
            {
                GameObject collidingObject = hits[1].collider.gameObject;
                laserRenderer.CutOffLaser(laser.pointIndex, hits[1].distance - (TriangularCoordinates.instance.radius * (1 - capFraction)));
                if(storeData)
                {
                    //Debug.Log("storeData = " + storeData + " on " + gameObject);
                    LaserConnection laserConnection = laserConnected[laser.pointIndex];
                    laserConnection.connectedPolygon = collidingObject.GetComponent<PolygonController>();
                    laserConnection.correctColors = false;
                    laserConnection.crossingIndex = -1;
                    foreach (var otherLaser in laserConnection.connectedPolygon.laserInfo)
                    {
                        if (TriangularCoordinates.instance.isInLine(laserConnection.connectedPolygon.laserTriangularCoords[otherLaser.pointIndex],
                                                                    laserConnection.connectedPolygon.laserTriangularCoords[otherLaser.pointIndex] - laserConnection.connectedPolygon.triangularPosition,
                                                                    laserTriangularCoords[laser.pointIndex]))
                        {
                            //Debug.Log(gameObject + " and " + laserConnection.connectedPolygon.gameObject + " lasers connecting. Colors: " + laser.colorData.laserColor + " and " + otherLaser.colorData.laserColor);
                            // If two lasers coincide, check the colors
                            if (laser.colorData == otherLaser.colorData)
                            {
                                // If the colors are the same, then they are properly connected
                                laserConnection.correctColors = true;

                                LaserConnection otherConnection = laserConnection.connectedPolygon.laserConnected[otherLaser.pointIndex];
                                otherConnection.connectedPolygon = this;
                                otherConnection.correctColors = true;
                                laserConnection.connectedPolygon.laserConnected[otherLaser.pointIndex] = otherConnection;
                                otherConnection.crossingIndex = -1;
                                laserRenderer.UnCrossLaser(laser.pointIndex);
                                laserConnection.connectedPolygon.laserRenderer.UnCrossLaser(otherLaser.pointIndex);
                                //Debug.Log(name + " hit " + hits[0].collider.gameObject.name + " and then " + hits[1].collider.gameObject.name);
                                //Debug.Log(name + "'s " + laser.pointIndex + ": " + laserConnection.connectedPolygon.gameObject.name + "'s " + otherLaser.pointIndex + " match colors.");
                            }
                            else
                            {
                                // If the colors are not the same, then they are not properly connected. Update other polygon as well.
                                LaserConnection otherConnection = laserConnection.connectedPolygon.laserConnected[otherLaser.pointIndex];
                                otherConnection.connectedPolygon = this;
                                otherConnection.correctColors = false;
                                otherConnection.crossingIndex = laser.pointIndex;
                                laserConnection.connectedPolygon.laserConnected[otherLaser.pointIndex] = otherConnection;

                                laserConnection.crossingIndex = otherLaser.pointIndex;

                                // Cross lasers
                                //Debug.Log("Crossing Lasers");
                                laserRenderer.CrossLaser(laser.pointIndex, laserConnection.connectedPolygon.laserRenderer, otherLaser.pointIndex);
                                //laserConnection.connectedPolygon.laserRenderer.CrossLaser(otherLaser.pointIndex);
                                //Debug.Log(name + " hit " + hits[0].collider.gameObject.name + " and then " + hits[1].collider.gameObject.name);
                                //Debug.Log(name + "'s " + laser.pointIndex + ": " + laserConnection.connectedPolygon.gameObject.name + "'s " + otherLaser.pointIndex + " do not match colors.");
                            }
                            break;
                        }
                    }
                    laserConnected[laser.pointIndex] = laserConnection;
                }
            }
            else
            {
                laserRenderer.ResetLaserLength(laser.pointIndex);

                // Reset laser connection
                LaserConnection laserConnection = laserConnected[laser.pointIndex];
                laserConnection.connectedPolygon = null;
                laserConnection.correctColors = false;
                laserConnected[laser.pointIndex] = laserConnection;
            }
        }
    }

    public void OnRuleApplied()
    {
        CheckForChainAndDestroy();
    }

    public void ApplyRule()
    {
        switch (rule)
        {
            case Rule.FlipHorizontal:
                foreach (var laser in laserInfo)
                {
                    laserRenderer.ResetLaserLength(laser.pointIndex);
                }
                UnCrossLasers();
                StartCoroutine(Flip(true));
                flipped = !flipped;
                break;
            case Rule.FlipVertical:
                foreach (var laser in laserInfo)
                {
                    laserRenderer.ResetLaserLength(laser.pointIndex);
                }
                UnCrossLasers();
                StartCoroutine(Flip(false));
                flipped = !flipped;
                break;
            case Rule.RotateCCW:
                UnCrossLasers();
                StartCoroutine(Rotate(false));
                break;
            case Rule.RotateCW:
                UnCrossLasers();
                StartCoroutine(Rotate(true));
                break;
            case Rule.None:
                StartCoroutine(NoneTimeWaster());
                break;
        }
    }

    public bool HasFullyConnectedChain(bool atStart = false)
    {
        if (IsFullyConnected())
        {
            visitedPolygons = new HashSet<PolygonController>();
            Queue<PolygonController> bfsQueue = new Queue<PolygonController>();

            //string polysToTakeOut = gameObject.name + ": ";

            // Create a set of polygons to be a chain.
            bfsQueue.Enqueue(this);
            while(bfsQueue.Count > 0)
            {
                visitedPolygons.Add(bfsQueue.Peek());
                //Debug.Log(this + ": " + bfsQueue.Peek());
                foreach(var laserConnection in bfsQueue.Peek().laserConnected)
                {
                    // Add all non-visited polygons
                    if((laserConnection.Value.connectedPolygon != null) && !visitedPolygons.Contains(laserConnection.Value.connectedPolygon))
                    {
                        bfsQueue.Enqueue(laserConnection.Value.connectedPolygon);
                    }
                }
                bfsQueue.Dequeue();
            }
            // Go through each visited polygon to see if they're all fully connected.

            
            foreach(var polygon in visitedPolygons)
            {
                if(polygon.IsFullyConnected())
                {
                    if(!atStart) // For some reason 3-8 breaks when this is run at the start and 2-6 breaks when it isn't run at all so...
                        polygon.UpdateLaserPhysicalCollisions(true);
                    //polysToTakeOut += polygon.gameObject.name + ", ";
                    continue;
                }
                else
                {
                    return false;
                }
            }
            //Debug.Log(polysToTakeOut);
            // If we get here, the chain is fully connected.
            return true;
        }
        return false;
    }

    public bool IsFullyConnected()
    {
        // Returns true if all the lasers on this polygon are connected to the same colored laser on other polygons
        //Debug.Log("Checking Connections for " + this.gameObject.name + ": " + laserConnected.Count + " connections");
        bool result = true;
        foreach (var laserConnection in laserConnected)
        {
            //Debug.Log(laserConnection.Value.connectedPolygon);
            if ((laserConnection.Value.connectedPolygon == null) | (!laserConnection.Value.correctColors))
            {
                result = false;
            }
        }
        //Debug.Log(result);
        polyRenderer.TintConnection(result);
        return result;
    }

    public Queue<PolygonController> GetRuleConnectedPolygons()
    {
        Queue<PolygonController> polyQueue = new Queue<PolygonController>();

        polyQueue.Enqueue(this);
        foreach(var connector in ruleConnectors)
        {
            Queue<PolygonController> currentQueue = connector.GetQueue();
            while (currentQueue.Count > 0)
            {
                if (!polyQueue.Contains(currentQueue.Peek()))
                {
                    polyQueue.Enqueue(currentQueue.Dequeue());
                }
                else
                {
                    currentQueue.Dequeue();
                }
            }
        }

        return polyQueue;
    }

    IEnumerator NoneTimeWaster()
    {
        float t = 0.0f;
        while (t < ruleDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        LoadLevelManager.RuleApplicationComplete(this);
    }

    IEnumerator Rotate(bool clockwise)
    {
        // Recalc points
        if (clockwise)
        {
            foreach (var laser in laserInfo)
            {
                Vector2 currentOffset = laserTriangularCoords[laser.pointIndex] - triangularPosition;
                // Multiply vector by Matrix
                // [ 0 -1]
                // [ 1  1]
                for (int i = 0; i < (polygonType == PolygonType.Hexagon ? 1 : 2); i++)
                {
                    Vector2 newOffset = Vector2.zero;
                    newOffset.x = -currentOffset.y;
                    newOffset.y = currentOffset.x + currentOffset.y;
                    currentOffset = newOffset;
                }
                laserTriangularCoords[laser.pointIndex] = triangularPosition + currentOffset;
            }
        }
        else
        {
            foreach (var laser in laserInfo)
            {
                Vector2 currentOffset = laserTriangularCoords[laser.pointIndex] - triangularPosition;
                // Multiply vector by Matrix
                // [ 1  1]
                // [-1  0]
                for (int i = 0; i < (polygonType == PolygonType.Hexagon ? 1 : 2); i++)
                {
                    Vector2 newOffset = Vector2.zero;
                    newOffset.x = currentOffset.x + currentOffset.y;
                    newOffset.y = -currentOffset.x;
                    currentOffset = newOffset;
                }
                laserTriangularCoords[laser.pointIndex] = triangularPosition + currentOffset;
            }
        }

        // Lerp a rotation around the z axis with the angle being the angle btw the points on the polygon
        float startRotation = Mathf.Round(transform.eulerAngles.z);
        float endRotation = clockwise ? Mathf.Round(transform.eulerAngles.z - angleBtwPoints) : Mathf.Round(transform.eulerAngles.z + angleBtwPoints);

        float spriteStartRotation = 0;
        float spriteEndRotation = 0;

        // If sprites are on the polygon, lerp their rotation backwards so they do not appear to rotate
        if (sprites.Count > 0)
        {
            spriteStartRotation = Mathf.Round(sprites[0].transform.localRotation.eulerAngles.z);
            spriteEndRotation = clockwise ? Mathf.Round(sprites[0].transform.localRotation.eulerAngles.z + angleBtwPoints) : Mathf.Round(sprites[0].transform.localRotation.eulerAngles.z - angleBtwPoints);
        }
        float t = 0.0f;
        while (t < ruleDuration)
        {
            t += Time.deltaTime;
            Quaternion angleChange = Quaternion.AngleAxis(Mathf.LerpAngle(startRotation, endRotation, Mathf.SmoothStep(0.0f, 1.0f, t / ruleDuration)), Vector3.forward);
            transform.rotation = angleChange;
            if (sprites.Count > 0) {
                Quaternion spriteAngleChange = Quaternion.AngleAxis(Mathf.LerpAngle(spriteStartRotation, spriteEndRotation, Mathf.SmoothStep(0.0f, 1.0f, t / ruleDuration)), Vector3.forward);
                sprites[0].transform.localRotation = spriteAngleChange;
                sprites[1].transform.localRotation = spriteAngleChange;
            }
            yield return null;
        }

        LoadLevelManager.RuleApplicationComplete(this);
    }
    IEnumerator Flip(bool horizontal)
    {
        // Recalc points
        if (horizontal)
        {
            foreach (var laser in laserInfo)
            {
                Vector2 currentOffset = laserTriangularCoords[laser.pointIndex] - triangularPosition;
                if (currentOffset.y != 0)
                {
                    // Multiply vector by Matrix
                    // [ 1  1]
                    // [ 0 -1]
                    currentOffset.x = currentOffset.x + currentOffset.y;
                    currentOffset.y = -currentOffset.y;
                }
                laserTriangularCoords[laser.pointIndex] = triangularPosition + currentOffset;
            }
        }
        else
        {
            foreach (var laser in laserInfo)
            {
                Vector2 currentOffset = laserTriangularCoords[laser.pointIndex] - triangularPosition;
                // Multiply vector by Matrix
                // [-1 -1]
                // [ 0  1]
                currentOffset.x = -currentOffset.x - currentOffset.y;
                laserTriangularCoords[laser.pointIndex] = triangularPosition + currentOffset;
            }
        }

        // Lerp a 180 degree rotation around the x or y axis depending on flip axis
        float startRotation = flipped ? 180 : 0;
        float endRotation = startRotation + 180;
        float t = 0.0f;
        while (t < ruleDuration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.AngleAxis(Mathf.LerpAngle(startRotation, endRotation, Mathf.SmoothStep(0.0f, 1.0f, t/ruleDuration)), horizontal ? Vector3.up : Vector3.right) * Quaternion.AngleAxis(startRotationOffset + ((polygonType == PolygonType.UpsideDownTriangle) ? 180 : 0), Vector3.forward);
            yield return null;
        }
        
        LoadLevelManager.RuleApplicationComplete(this);
    }

    void CreateCollider()
    {
        // Get triangles and vertices from mesh
        int[] triangles = polyRenderer.meshFilter.mesh.triangles;
        Vector3[] vertices = polyRenderer.meshFilter.mesh.vertices;

        // Get just the outer edges from the mesh's triangles (ignore or remove any shared edges)
        Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int e = 0; e < 3; e++)
            {
                int vert1 = triangles[i + e];
                int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
                string edge = Mathf.Min(vert1, vert2) + ":" + Mathf.Max(vert1, vert2);
                if (edges.ContainsKey(edge))
                {
                    edges.Remove(edge);
                }
                else
                {
                    edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
                }
            }
        }

        // Create edge lookup (Key is first vertex, Value is second vertex, of each edge)
        Dictionary<int, int> lookup = new Dictionary<int, int>();
        foreach (KeyValuePair<int, int> edge in edges.Values)
        {
            if (lookup.ContainsKey(edge.Key) == false)
            {
                lookup.Add(edge.Key, edge.Value);
            }
        }

        // Create empty polygon collider
        polygonCollider.pathCount = 0;

        // Loop through edge vertices in order
        int startVert = 0;
        int nextVert = startVert;
        int highestVert = startVert;
        List<Vector2> colliderPath = new List<Vector2>();
        while (true)
        {
            // Add vertex to collider path
            colliderPath.Add(vertices[nextVert]);

            // Get next vertex
            nextVert = lookup[nextVert];

            // Store highest vertex (to know what shape to move to next)
            if (nextVert > highestVert)
            {
                highestVert = nextVert;
            }

            // Shape complete
            if (nextVert == startVert)
            {

                // Add path to polygon collider
                polygonCollider.pathCount++;
                polygonCollider.SetPath(polygonCollider.pathCount - 1, colliderPath.ToArray());
                colliderPath.Clear();

                // Go to next shape if one exists
                if (lookup.ContainsKey(highestVert + 1))
                {

                    // Set starting and next vertices
                    startVert = highestVert + 1;
                    nextVert = startVert;

                    // Continue to next loop
                    continue;
                }

                // No more verts
                break;
            }
        }
    }

    public bool CheckForChainAndDestroy(bool atStart = false)
    {
        UpdateLaserPhysicalCollisions(true);
        if (HasFullyConnectedChain(atStart))
        {
            // Destroy all polygons in the chain except this one.
            string debugString = "";
            foreach (var polygon in visitedPolygons)
            {
                if (polygon != this)
                {
                    // This might cause problems
                    LoadLevelManager.AddPolygonToDestroyList(polygon);
                    if (polygon != null && polygon.gameObject.activeSelf)
                    {
                        polygon.StartCoroutine(polygon.DestroySequence());
                    }
                }
                debugString += polygon.name + " ";
            }
            //Debug.Log(this + " is fully connected to " + visitedPolygons.Count + " polygons: " + debugString + ". atStart = " + atStart);
            // Now destroy this one
            AudioManager.instance.PlayDelete();
            isChainRoot = true;
            LoadLevelManager.AddPolygonToDestroyList(this);
            StartCoroutine(DestroySequence());
            return true;
        }
        return false;
    }

    public void ShootStartingLaser()
    {
        UpdateLaserPhysicalCollisions(true);
        CheckForChainAndDestroy(true);
    }

    private void UnCrossLasers()
    {
        foreach(var laser in laserInfo)
        {
            if((laserConnected[laser.pointIndex].connectedPolygon != null) && (laserConnected[laser.pointIndex].crossingIndex != -1))
            {
                laserConnected[laser.pointIndex].connectedPolygon.laserRenderer.UnCrossLaser(laserConnected[laser.pointIndex].crossingIndex);
                laserRenderer.UnCrossLaser(laser.pointIndex);
            }
        }
    }

    private void OnDestroy()
    {
        if (isBeingDestroyed)
        {
            LoadLevelManager.RemovePolygon(this);
            if (isChainRoot)
            {
                LoadLevelManager.CheckPolygons();
            }
        }
        foreach (var connector in ruleConnectors)
        {
            connector.Unsubscribe(this);
        }
    }

    IEnumerator DestroySequence()
    {
        isBeingDestroyed = true;

        foreach (var connector in ruleConnectors)
        {
            connector.Unsubscribe(this);
        }
        ruleConnectors.Clear();
        polyRenderer.TurnOffConnections();

        float timer = 0;
        while (timer < destroyDuration)
        {
            timer = Mathf.Clamp(timer + Time.deltaTime, 0, destroyDuration);
            polyRenderer.LerpColorTo(LoadLevelManager.polygonDestroyColor, timer/destroyDuration);
            laserRenderer.LerpLaserColorsTo(LoadLevelManager.polygonDestroyColor, timer/destroyDuration);
            yield return null;
        }
        GameObject deathParticles = Instantiate(explosionParticles, this.transform.position, Quaternion.identity);
        Destroy(deathParticles, deathParticles.GetComponent<ParticleSystem>().main.startLifetime.constant);
        Destroy(gameObject);
    }

    [System.Serializable]
    public struct LaserInfo
    {
        public int pointIndex;
        public LaserColorData colorData;
    }

    public struct LaserConnection
    {
        public bool correctColors;
        public LaserColorData colorData;
        public PolygonController connectedPolygon;
        public int crossingIndex;
    }

    bool hoverState;

    public void EnsureNotHovering()
    {
        if(hoverState)
        {
            SetHoverState(false);
        }
    }

    public void SetHoverState(bool hoverOn)
    {
        hoverState = hoverOn;
        polyRenderer.meshRenderer.material.SetInt("_isHovered", hoverOn ? 1 : 0);
        foreach(var connector in ruleConnectors)
        {
            connector.SetHoverState(hoverOn);
        }
    } 

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // If not in play mode, draw the polygons and colored spheres for each laser
        if (!EditorApplication.isPlaying)
        {
            Mesh mesh = new Mesh();

            int pointAmount = (polygonType == PolygonType.Hexagon) ? 6 : 3;

            angleBtwPoints = 360.0f / (float)pointAmount;

            Vector3[] vertices = new Vector3[pointAmount + 1];
            Vector3[] normals = new Vector3[pointAmount + 1];
            int[] tris = new int[pointAmount * 3];
            Vector2[] uvs = new Vector2[pointAmount + 1];

            for (int i = 0; i < pointAmount; i++)
            {
                // Points
                Vector3 newPoint = new Vector3();
                float pointAngle = angleBtwPoints * i + startRotationOffset + (polygonType == PolygonType.UpsideDownTriangle ? 180 : 0);
                newPoint.x = gizmoRadius * Mathf.Cos(pointAngle * Mathf.Deg2Rad);
                newPoint.y = gizmoRadius * Mathf.Sin(pointAngle * Mathf.Deg2Rad);
                newPoint.z = 0;
                vertices[i] = newPoint;

                // Normals
                normals[i] = -Vector3.forward;

                // UV
                Vector2 newUvPoint = new Vector2();
                newUvPoint.x = Mathf.Cos(pointAngle * Mathf.Deg2Rad) / (2 * gizmoRadius) + 0.5f;
                newUvPoint.y = Mathf.Sin(pointAngle * Mathf.Deg2Rad) / (2 * gizmoRadius) + 0.5f;
                uvs[i] = newUvPoint;

                // triangles
                tris[3 * i] = pointAmount;
                tris[3 * i + 2] = i;
                tris[3 * i + 1] = (i + 1) % pointAmount;

                foreach (var laser in laserInfo)
                {
                    if (laser.colorData != null && laser.pointIndex == i)
                    {
                        Gizmos.color = laser.colorData.laserColor;
                        Gizmos.DrawSphere(newPoint + this.transform.position, 0.15f);
                    }
                }
            }
            vertices[pointAmount] = new Vector3(0, 0, 0);
            normals[pointAmount] = -Vector3.forward;
            uvs[pointAmount] = new Vector2(0.5f, 0.5f);

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = tris;

            float ruleConnectorCircleRad = 0.3f;
            foreach (var connector in ruleConnectors)
            {
                if (connector != null)
                {
                    Gizmos.color = connector.ruleColor;
                    Gizmos.DrawWireSphere(transform.position, ruleConnectorCircleRad);
                    ruleConnectorCircleRad += 0.2f;
                }
            }
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawMesh(mesh, this.transform.position);

            Handles.Label(this.transform.position, rule.ToString());
        }
    }
#endif
}
