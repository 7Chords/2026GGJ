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
    public static RandomUtility Instance { get; private set; }

    [SerializeField]
    private string globalSeed = "2026GGJ";

    private Dictionary<ModuleType, System.Random> _randomGenerators = new Dictionary<ModuleType, System.Random>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGenerators();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGenerators()
    {
        int seedHash = globalSeed.GetHashCode();
        foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
        {
            _randomGenerators[type] = new System.Random(seedHash + (int)type);
        }
    }

    public System.Random GetRandomGenerator(ModuleType module)
    {
        if (!_randomGenerators.ContainsKey(module))
        {
            _randomGenerators[module] = new System.Random();
        }
        return _randomGenerators[module];
    }
}
