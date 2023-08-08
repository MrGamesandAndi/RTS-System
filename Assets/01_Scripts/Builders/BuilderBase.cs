using RTSSystem.Buildables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RTSSystem.Builders
{
    public class BuilderBase : MonoBehaviour
    {
        public enum CancelBehaviour
        {
            CancelsInProgress = 0,
            CancelsLastQueued = 1
        }

        public class BuildData
        {
            public BuilderBase OwningBuilder { get; private set; }
            public BuildableObjectBaseSO ObjectBeingBuilt { get; private set; }
            public float CurrentBuildProgress { get; private set; }

            public bool HasStarted => CurrentBuildProgress > 0f;

            public BuildData(BuilderBase owningBuilder, BuildableObjectBaseSO objectBeingBuilt)
            {
                OwningBuilder = owningBuilder;
                ObjectBeingBuilt = objectBeingBuilt;
                CurrentBuildProgress = 0f;
            }

            public bool Tick(float deltaTime)
            {
                CurrentBuildProgress = Mathf.Clamp01(CurrentBuildProgress + (deltaTime / ObjectBeingBuilt.BuildTime));
                return CurrentBuildProgress >= 1f;
            }
        }

        [SerializeField] BuildableDatabaseSO _buildablesDB;
        [SerializeField] CancelBehaviour _cancelBehaviour = CancelBehaviour.CancelsLastQueued;

        [Tooltip("If not empty then can only see these types of buildables")]
        [SerializeField] List<BuildableObjectBaseSO.Type> _permittedTypes = new();

        [Tooltip("If not empty then only the listed buildables are supported")]
        [SerializeField] List<BuildableObjectBaseSO> _overridePermittedBuildables = new();

        Dictionary<BuildableObjectBaseSO.Type, List<BuildableObjectBaseSO>> _availableBuildables = new();
        List<BuildData> _buildsInProgress = new();
        bool _isPaused = false;


        [Header("Events")]
        public UnityEvent<BuildData> OnBuildQueued = new();
        public UnityEvent<BuildData> OnBuildStarted = new();
        public UnityEvent<BuildData> OnBuildPaused = new();
        public UnityEvent<BuildData> OnBuildResumed = new();
        public UnityEvent<BuildData> OnBuildCancelled = new();
        public UnityEvent<BuildData> OnBuildTicked = new();
        public UnityEvent<BuildData> OnBuildCompleted = new();

        [Header("DEBUG ONLY")]
        [SerializeField] BuildableObjectBaseSO DEBUG_ObjectToBuild;
        [SerializeField] bool DEBUG_LogActions = true;
        [SerializeField] bool DEBUG_TriggerPause = false;
        [SerializeField] bool DEBUG_TriggerResume = false;
        [SerializeField] bool DEBUG_CancelBuild = false;

        private void Awake()
        {
            //Check if buildables are being overwritten
            if (_overridePermittedBuildables.Count > 0)
            {
                foreach (var buildable in _overridePermittedBuildables)
                {
                    List<BuildableObjectBaseSO> buildablesOfType;

                    if (!_availableBuildables.TryGetValue(buildable.BuildableType, out buildablesOfType))
                    {
                        _availableBuildables[buildable.BuildableType] = buildablesOfType = new List<BuildableObjectBaseSO>();
                    }

                    buildablesOfType.Add(buildable);
                }
            }
            else
            {
                //Populate using the database
                foreach (var buildable in _buildablesDB.AllBuildables)
                {
                    List<BuildableObjectBaseSO> buildablesOfType;

                    //Skip if not permitted type
                    if (_permittedTypes.Count > 0 && !_permittedTypes.Contains(buildable.BuildableType))
                    {
                        continue;
                    }

                    if (!_availableBuildables.TryGetValue(buildable.BuildableType, out buildablesOfType))
                    {
                        _availableBuildables[buildable.BuildableType] = buildablesOfType = new List<BuildableObjectBaseSO>();
                    }

                    buildablesOfType.Add(buildable);
                }
            }
        }

        private void Start()
        {
            OnBuildQueued.AddListener(GlobalBuildManager.Instance.OnBuildRequested);
            OnBuildCancelled.AddListener(GlobalBuildManager.Instance.OnBuildCancelled);
            OnBuildCompleted.AddListener(GlobalBuildManager.Instance.OnBuildCompleted);

            if (DEBUG_LogActions)
            {
                OnBuildQueued.AddListener((BuildData buildData) => {Debug.Log($"Queued build of {buildData.ObjectBeingBuilt.ObjectName}");});
                OnBuildStarted.AddListener((BuildData buildData) => {Debug.Log($"Started build of {buildData.ObjectBeingBuilt.ObjectName}");});
                OnBuildPaused.AddListener((BuildData buildData) => {Debug.Log($"Paused build of {buildData.ObjectBeingBuilt.ObjectName}");});
                OnBuildResumed.AddListener((BuildData buildData) => {Debug.Log($"Resumed build of {buildData.ObjectBeingBuilt.ObjectName}");});
                OnBuildCancelled.AddListener((BuildData buildData) => {Debug.Log($"Cancelled build of {buildData.ObjectBeingBuilt.ObjectName}");});
                OnBuildTicked.AddListener((BuildData buildData) => {Debug.Log($"Ticked build of {buildData.ObjectBeingBuilt.ObjectName}");});
                OnBuildCompleted.AddListener((BuildData buildData) => { Debug.Log($"Completed build of {buildData.ObjectBeingBuilt.ObjectName}"); });
            }
        }

        private void Update()
        {
            if (DEBUG_ObjectToBuild)
            {
                RequestToQueueBuild(DEBUG_ObjectToBuild);
                DEBUG_ObjectToBuild = null;
            }

            if (DEBUG_TriggerPause)
            {
                DEBUG_TriggerPause = false;
                Pause();
            }

            if (DEBUG_TriggerResume)
            {
                DEBUG_TriggerResume = false;
                Resume();
            }

            if (DEBUG_CancelBuild && _buildsInProgress.Count > 0)
            {
                CancelBuild(_buildsInProgress[0]);
            }

            if (!_isPaused && _buildsInProgress.Count > 0)
            {
                TickBuilds(Time.deltaTime);
            }
        }

        private void TickBuilds(float deltaTime)
        {
            for (int index = 0; index < _buildsInProgress.Count; index++)
            {
                var buildData = _buildsInProgress[index];

                if (!buildData.HasStarted)
                {
                    OnBuildStarted.Invoke(buildData);
                }

                bool isFinished = buildData.Tick(deltaTime);
                OnBuildTicked.Invoke(buildData);

                if (isFinished)
                {
                    OnBuildCompleted.Invoke(buildData);
                    _buildsInProgress.RemoveAt(index);
                    index--;
                }
                else
                {
                    return;
                }
            }
        }

        public bool CanBuild(BuildableObjectBaseSO buildable)
        {
            List<BuildableObjectBaseSO> buildablesSubset;

            if (!_availableBuildables.TryGetValue(buildable.BuildableType, out buildablesSubset))
            {
                return false;
            }

            if (!buildablesSubset.Contains(buildable))
            {
                return false;
            }

            if (buildable.QueueSizeLimit > 0)
            {
                int numBeingBuilt = GlobalBuildManager.Instance.GetNumberOfBuildsInProgress(buildable);

                if (numBeingBuilt >= buildable.QueueSizeLimit)
                {
                    return false;
                }
            }

            if (buildable.GlobalBuildLimit > 0)
            {
                int numBeingBuiltOrInProgress = GlobalBuildManager.Instance.GetNumberBuilt(buildable) +
                    GlobalBuildManager.Instance.GetNumberOfBuildsInProgress(buildable);

                if (numBeingBuiltOrInProgress >= buildable.GlobalBuildLimit)
                {
                    return false;
                }
            }

            return true;
        }

        public bool RequestToQueueBuild(BuildableObjectBaseSO buildable)
        {
            if (!CanBuild(buildable))
            {
                return false;
            }

            BuildData newBuildData = new BuildData(this, buildable);
            _buildsInProgress.Add(newBuildData);
            OnBuildQueued.Invoke(newBuildData);
            return true;
        }

        public bool RequestToCancelBuild(BuildableObjectBaseSO buildable)
        {
            if (_buildsInProgress.Count == 0)
            {
                return true;
            }

            if (_buildsInProgress[0].ObjectBeingBuilt == buildable)
            {
                if (_cancelBehaviour == CancelBehaviour.CancelsInProgress)
                {
                    return CancelBuild(_buildsInProgress[0]);
                }
                else
                {
                    for (int itemIndex = _buildsInProgress.Count - 1; itemIndex >= 0; itemIndex--)
                    {
                        if (_buildsInProgress[itemIndex].ObjectBeingBuilt == buildable)
                        {
                            return CancelBuild(_buildsInProgress[itemIndex]);
                        }
                    }
                }
            }

            for (int itemIndex = 0; itemIndex < _buildsInProgress.Count; itemIndex++)
            {
                if (_buildsInProgress[itemIndex].ObjectBeingBuilt == buildable)
                {
                    _buildsInProgress.RemoveAt(itemIndex);
                    return true;
                }
            }

            return false;
        }

        public bool CancelBuild(BuildData buildData)
        {
            if (!_buildsInProgress.Contains(buildData))
            {
                return false;
            }

            _buildsInProgress.Remove(buildData);
            OnBuildCancelled.Invoke(buildData);
            return true;
        }

        public void Pause()
        {
            if (_isPaused)
            {
                return;
            }

            _isPaused = true;

            if (_buildsInProgress.Count > 0)
            {
                OnBuildPaused.Invoke(_buildsInProgress[0]);
            }
        }

        public void Resume()
        {
            if (!_isPaused)
            {
                return;
            }

            _isPaused = false;

            if (_buildsInProgress.Count > 0)
            {
                OnBuildResumed.Invoke(_buildsInProgress[0]);
            }
        }

        public List<BuildableObjectBaseSO>GetBuildableItemsForType(BuildableObjectBaseSO.Type inType)
        {
            List<BuildableObjectBaseSO> outList = null;
            _availableBuildables.TryGetValue(inType, out outList);
            return outList;
        }

        public int GetNumberOfItemQueued(BuildableObjectBaseSO buildable)
        {
            int numQueued = 0;

            foreach (var buildData in _buildsInProgress)
            {
                if (buildData.ObjectBeingBuilt == buildable)
                {
                    numQueued++;
                }
            }

            return numQueued;
        }
    }
}
