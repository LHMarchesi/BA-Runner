using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RoadConfig",
    menuName = "Scriptable Objects/Road Config")]
public class RoadConfig : ScriptableObject
{
    public List<RoadSection> sections = new();
}
