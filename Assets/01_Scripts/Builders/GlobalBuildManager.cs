using RTSSystem.Buildables;
using System.Collections.Generic;
using UnityEngine;

namespace RTSSystem.Builders
{
    public class GlobalBuildManager : MonoBehaviour
    {
        public static GlobalBuildManager Instance { get; private set; } = null;

        public Dictionary<BuildableObjectBaseSO, int> GlobalBuildCounts { get; private set; } = new();
        public Dictionary<BuildableObjectBaseSO, int> BuildInProgressCounts { get; private set; } = new();


        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"Found duplicate GlobalBuildManager on {gameObject.name}");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void OnBuildRequested(BuilderBase.BuildData buildData)
        {
            int numInProgress = GetNumberOfBuildsInProgress(buildData.ObjectBeingBuilt);
            BuildInProgressCounts[buildData.ObjectBeingBuilt] = numInProgress + 1;
        }

        public void OnBuildCancelled(BuilderBase.BuildData buildData)
        {
            int numInProgress = GetNumberOfBuildsInProgress(buildData.ObjectBeingBuilt);
            BuildInProgressCounts[buildData.ObjectBeingBuilt] = Mathf.Max(0, numInProgress - 1);
        }

        public void OnBuildCompleted(BuilderBase.BuildData buildData)
        {
            int numInProgress = GetNumberOfBuildsInProgress(buildData.ObjectBeingBuilt);
            BuildInProgressCounts[buildData.ObjectBeingBuilt] = Mathf.Max(0, numInProgress - 1);
            int numBuilt = GetNumberBuilt(buildData.ObjectBeingBuilt);
            GlobalBuildCounts[buildData.ObjectBeingBuilt] = numBuilt + 1;
        }

        public int GetNumberBuilt(BuildableObjectBaseSO buildable)
        {
            int numBuilt = 0;
            GlobalBuildCounts.TryGetValue(buildable, out numBuilt);
            return numBuilt;
        }

        public int GetNumberOfBuildsInProgress(BuildableObjectBaseSO buildable)
        {
            int numInProgress = 0;
            BuildInProgressCounts.TryGetValue(buildable, out numInProgress);
            return numInProgress;
        }
    }
}