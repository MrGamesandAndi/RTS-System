using System.Collections.Generic;
using UnityEngine;

namespace RTSSystem.Buildables
{
    [CreateAssetMenu(menuName = "RTS/Buildables Database", fileName = "BuildablesDB")]
    public class BuildableDatabaseSO : ScriptableObject
    {
        [field: SerializeField] public List<BuildableObjectBaseSO> AllBuildables { get; private set; } = new();
    }
}