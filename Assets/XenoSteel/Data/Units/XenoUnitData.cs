using UnityEngine;
using XenoSteel.Units;

public enum TerrainAdaptation
{
    Good,
    Normal,
    Bad
}

[CreateAssetMenu(fileName = "NewXenoUnitData", menuName = "XenoSteel/Unit Data")]
public class XenoUnitData : ScriptableObject
{
    [Header("基本情報")]
    public string unitName;

    [Header("機体ステータス")]
    public int hp;
    public int en;
    public int armor;
    public int mobility;
    public int movement;

    [Header("環境適応")]
    public TerrainAdaptation terrainAdaptation;

    [Header("サイズ")]
    public int size;

    [Header("パイロット")]
    public PilotData pilot;
}