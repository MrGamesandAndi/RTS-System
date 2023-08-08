using UnityEngine;

namespace RTSSystem.Buildables
{
    [CreateAssetMenu(menuName = "RTS/Buildables/Unit", fileName = "BLD_Unit")]
    public class BuildableUnitSO : BuildableObjectBaseSO
    {
#if UNITY_EDITOR
        protected override void InitialiseDefaults()
        {
            BuildableType = Type.Unit;
            ObjectName = "Unit";
            Description = ObjectName;
            Cost = 0;
            BuildTime = 5f;
            QueueSizeLimit = 10;
            GlobalBuildLimit = -1;
        }
#endif
    }
}
