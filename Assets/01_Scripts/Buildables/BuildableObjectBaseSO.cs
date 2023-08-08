using UnityEngine;

namespace RTSSystem.Buildables
{

    public abstract class BuildableObjectBaseSO : ScriptableObject
    {
        public enum Type
        {
            NotSet = 0,
            Building = 1,
            Unit = 101,
            Upgrade = 201
        }

        [field: SerializeField] public Type BuildableType { get; protected set; } = Type.NotSet;
        [field: SerializeField] public string ObjectName { get; protected set; }
        [field: SerializeField] public string Description { get; protected set; }
        [field: SerializeField] public int Cost { get; protected set; }
        [field: SerializeField] public float BuildTime { get; protected set; }
        [field: SerializeField] public int QueueSizeLimit { get; protected set; }
        [field: SerializeField] public int GlobalBuildLimit { get; protected set; }
        [field: SerializeField] public Sprite UIImage { get; protected set; }

#if UNITY_EDITOR
        private void Reset()
        {
            if (BuildableType == Type.NotSet && !Application.isPlaying)
            {
                InitialiseDefaults();
            }
        }

        protected abstract void InitialiseDefaults();
#endif
    }
}