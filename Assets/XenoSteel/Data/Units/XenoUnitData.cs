using UnityEngine;
using XenoSteel.Units;
using XenoSteel.Combat;

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
    public int attack;

    [Header("環境適応")]
    public TerrainAdaptation terrainAdaptation;

    [Header("サイズ")]
    public int size;

    [Header("パイロット")]
    public PilotData pilot;

    [Header("スキル")]
    public SkillData[] skills;

    [Header("Information")]
    public int visionRange = 5;
    public int radarRange = 0;
    public int radarAccuracy = 0;
    public int stealth = 0;
}