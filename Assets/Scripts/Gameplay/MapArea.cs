using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//To control the wild pokemons spwaning in a region (map)
public class MapArea : MonoBehaviour
{
    [SerializeField] List<AnimalEncounterRecord> wildAnimals;
    [SerializeField] List<AnimalEncounterRecord> wildAnimalsInWater;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    [HideInInspector]
    [SerializeField] int totalChanceWater = 0;

    private void OnValidate()
    {
        CalculateChancePercentage();
    }

    private void Start()
    {
        CalculateChancePercentage();
    }

    void CalculateChancePercentage()
    {
        totalChance = -1;
        totalChanceWater = -1;

        if (wildAnimals.Count > 0)
        {
            totalChance = 0;
            foreach (var record in wildAnimals)
            {
                record.chanceLower = totalChance;
                record.chanceUpper = totalChance + record.chancePercentage;

                totalChance = totalChance + record.chancePercentage;
            }
        }

        if (wildAnimalsInWater.Count > 0)
        {
            totalChanceWater = 0;
            foreach (var record in wildAnimalsInWater)
            {
                record.chanceLower = totalChanceWater;
                record.chanceUpper = totalChanceWater + record.chancePercentage;

                totalChanceWater = totalChanceWater + record.chancePercentage;
            }
        }
    }

    public Animal GetRandomWildAnimal(BattleTrigger trigger)
    {
        var animalList = (trigger == BattleTrigger.LongGrass) ? wildAnimals : wildAnimalsInWater;

        int randVal = Random.Range(1, 101);
        var animalRecord = animalList.FirstOrDefault(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);

        var levelRange = animalRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);

        var wildAnimal = new Animal(animalRecord.animal, level);
        wildAnimal.Init();
        return wildAnimal;

    }
}

[System.Serializable]
public class AnimalEncounterRecord
{
    public AnimalBase animal;
    public Vector2Int levelRange;
    public int chancePercentage;

    public int chanceLower { get; set; }
    public int chanceUpper { get; set; }
}
