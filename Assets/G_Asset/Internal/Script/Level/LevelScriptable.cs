using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class LevelScriptable : ScriptableObject
{
    public Vector2Int size;
    public List<Sprite> sprites;
    public List<Vector2Int> blocksList;
    public int requirePoint = 0;
}
