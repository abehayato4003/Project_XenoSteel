using UnityEngine;
using XenoSteel.Units;

public class XenoSteelUnitStats
{
    public int HP { get; }
    
    public int MaxEN { get; }
    public int EN { get; private set; }

    public int Armor { get; }
    public int Mobility { get; }
    public int Movement { get; }
    public int Attack { get; }
    public TerrainAdaptation TerrainAdaptation { get; }
    public int Size { get; }

    public XenoSteelUnitStats(XenoUnitData unitData)
    {
        HP = unitData.hp;

        MaxEN = unitData.en;
        EN = MaxEN;

        Armor = unitData.armor;
        Mobility = unitData.mobility;
        Movement = unitData.movement;
        Attack = unitData.attack;
        TerrainAdaptation = unitData.terrainAdaptation;
        Size = unitData.size;

        if (unitData.pilot != null)
        {
            HP = Mathf.RoundToInt(HP * unitData.pilot.HPMultiplier);

            MaxEN = Mathf.RoundToInt(MaxEN * unitData.pilot.ENMultiplier);
            EN = MaxEN;

            Armor = Mathf.RoundToInt(Armor * unitData.pilot.ArmorMultiplier);
            Mobility = Mathf.RoundToInt(Mobility * unitData.pilot.MobilityMultiplier);
        }
    }

    public bool CanConsumeEN(int amount)
    {
        return EN >= amount;
    }

    public bool ConsumeEN(int amount)
    {
        if (amount < 0 || EN < amount)
        {
            return false;
        }

        EN -= amount;
        return true;
    }

    public void RecoverEN(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        EN = Mathf.Min(EN + amount, MaxEN);
    }
}