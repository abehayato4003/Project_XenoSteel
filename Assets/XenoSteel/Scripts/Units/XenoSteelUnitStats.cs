using UnityEngine;
using XenoSteel.Units;

public class XenoSteelUnitStats
{
    public int HP { get; }
    public int EN { get; }
    public int Armor { get; }
    public int Mobility { get; }
    public int Movement { get; }
    public int Attack { get; }
    public TerrainAdaptation TerrainAdaptation { get; }
    public int Size { get; }

    public XenoSteelUnitStats(XenoUnitData unitData)
    {
        HP = unitData.hp;
        EN = unitData.en;
        Armor = unitData.armor;
        Mobility = unitData.mobility;
        Movement = unitData.movement;
        Attack = unitData.attack;
        TerrainAdaptation = unitData.terrainAdaptation;
        Size = unitData.size;

        if (unitData.pilot != null)
        {
            HP = Mathf.RoundToInt(HP * unitData.pilot.HPMultiplier);
            EN = Mathf.RoundToInt(EN * unitData.pilot.ENMultiplier);
            Armor = Mathf.RoundToInt(Armor * unitData.pilot.ArmorMultiplier);
            Mobility = Mathf.RoundToInt(Mobility * unitData.pilot.MobilityMultiplier);
        }
    }
}