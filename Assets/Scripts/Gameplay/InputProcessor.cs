using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputProcessor : MonoBehaviour
{
    public LayerMask polygons;

    bool mouseClicked;

    private void Start()
    {
        mouseClicked = false;
    }

    GameObject currentHoveredPolygon;

    // Update is called once per frame
    void Update()
    {
        // Mouse clicks
        if (!LoadLevelManager.IsLevelEnd() && !DialogBox.instance.CurrentlyPrompted)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hitData = Physics2D.Raycast(new Vector2(worldPosition.x, worldPosition.y), Vector2.zero, 0, polygons);
            if(hitData)
            {
                if(hitData.transform.gameObject != currentHoveredPolygon)
                {
                    currentHoveredPolygon = hitData.transform.gameObject;
                    currentHoveredPolygon.GetComponent<PolygonController>().SetHoverState(true);
                }
            }
            else
            {
                if (currentHoveredPolygon != null)
                {
                    currentHoveredPolygon.GetComponent<PolygonController>().SetHoverState(false);
                    currentHoveredPolygon = null;
                }
                else
                {
                    LoadLevelManager.EnsureNoPolygonsHovering();
                }
            }

            if (hitData && Input.GetMouseButtonDown(0) && !mouseClicked)
            {
                LoadLevelManager.QueueRuleApplication(hitData.transform.gameObject.GetComponent<PolygonController>());
                mouseClicked = true;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                mouseClicked = false;
            }
        }
    }
}
