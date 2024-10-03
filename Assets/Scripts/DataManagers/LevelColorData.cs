using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Level Color Data", fileName = "LevelColorData")]
public class LevelColorData : ScriptableObject
{
    public Color polygonColor;
    public Color backgroundColor;
    public Color foregroundColor;
    public Color badIntersectingLineColor;
    public Color destructionColor;
}
