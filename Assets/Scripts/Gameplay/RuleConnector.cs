using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rule Connector", fileName = "Rule Connector")]
public class RuleConnector : ScriptableObject
{
    private HashSet<PolygonController> subscribedPolys;

    public Color ruleColor;

    private Dictionary<PolygonController, LineRenderer> lineRenderers;

    private Dictionary<PolygonController, Coroutine> hoverCoroutines;

    private void Awake()
    {
        subscribedPolys = new HashSet<PolygonController>();
        lineRenderers = new Dictionary<PolygonController, LineRenderer>();
        hoverCoroutines = new Dictionary<PolygonController, Coroutine>();
    }

    public void SetHoverState(bool hoverOn)
    {
        if (hoverCoroutines == null) 
        {
            hoverCoroutines = new Dictionary<PolygonController, Coroutine>();
        }
        if(hoverOn)
        {
            foreach(var renderer in lineRenderers)
            {
                if (renderer.Key != null)
                {
                    if(hoverCoroutines.ContainsKey(renderer.Key))
                    {
                        renderer.Key.StopCoroutine(hoverCoroutines[renderer.Key]);
                    }
                    hoverCoroutines[renderer.Key] = renderer.Key.StartCoroutine(OnHover(renderer.Value));
                }
            }
        }
        else
        {
            foreach(var coroutine in hoverCoroutines)
            {
                coroutine.Key.StopCoroutine(coroutine.Value);
            }
        }
    }

    IEnumerator OnHover(LineRenderer renderer)
    {
        while (true)
        {
            renderer.material.mainTextureOffset = renderer.material.mainTextureOffset + new Vector2(0.05f, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void LinkRenderer(PolygonController poly, LineRenderer renderer)
    {
        if(lineRenderers == null)
        {
            lineRenderers = new Dictionary<PolygonController, LineRenderer>();
        }
        lineRenderers.Add(poly, renderer);
    }

    public void Subscribe(PolygonController poly)
    {
        if(subscribedPolys == null)
        {
            subscribedPolys = subscribedPolys = new HashSet<PolygonController>();
        }
        subscribedPolys.Add(poly);
    }

    public void Unsubscribe(PolygonController poly)
    {
        foreach(var polygon in subscribedPolys)
        {
            if (hoverCoroutines.ContainsKey(polygon))
            {
                polygon.StopCoroutine(hoverCoroutines[polygon]);
            }
            hoverCoroutines.Remove(polygon);
        }
        if (subscribedPolys == null)
        {
            subscribedPolys = subscribedPolys = new HashSet<PolygonController>();
        }
        if (lineRenderers == null)
        {
            lineRenderers = new Dictionary<PolygonController, LineRenderer>();
        }
        if (hoverCoroutines == null)
        {
            hoverCoroutines = new Dictionary<PolygonController, Coroutine>();
        }
        lineRenderers.Remove(poly);
        subscribedPolys.Remove(poly);
    }

    public Queue<PolygonController> GetQueue()
    {
        if (subscribedPolys == null)
        {
            subscribedPolys = subscribedPolys = new HashSet<PolygonController>();
        }
        Queue<PolygonController> retQueue = new Queue<PolygonController>();
        foreach(var poly in subscribedPolys)
        {
            retQueue.Enqueue(poly);
        }
        return retQueue;
    }
}
