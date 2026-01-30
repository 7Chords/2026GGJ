using System;
using System.Collections.Generic;
using UnityEngine;

public enum ModuleType
{
    Map,
    Combat,
    Event,
    Loot
}

public class RandomUtility : MonoBehaviour
{

    [SerializeField]
    private string globalSeed = "2026GGJ";

    private static Dictionary<ModuleType, System.Random> _randomGenerators = new Dictionary<ModuleType, System.Random>();

    private void Awake()
    {
        InitializeGenerators();
    }

    private void InitializeGenerators()
    {
        int seedHash = globalSeed.GetHashCode();
        foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
        {
            _randomGenerators[type] = new System.Random(seedHash + (int)type);
        }
    }

    public static System.Random GetRandomGenerator(ModuleType module)
    {
        if (!_randomGenerators.ContainsKey(module))
        {
            _randomGenerators[module] = new System.Random();
        }
        return _randomGenerators[module];
    }
}
