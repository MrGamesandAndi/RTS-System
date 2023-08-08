using UnityEngine;

namespace RTSSystem.Buildables
{
    [CreateAssetMenu(menuName = "RTS/Buildables/Upgrade", fileName = "BLD_Upgrade")]
    public class BuildableUpgradeSO : BuildableObjectBaseSO
    {
#if UNITY_EDITOR
        protected override void InitialiseDefaults()
        {
            BuildableType = Type.Upgrade;
            ObjectName = "Upgrade";
            Description = ObjectName;
            Cost = 0;
            BuildTime = 5f;
            QueueSizeLimit = 1;
            GlobalBuildLimit = 1;
        }
#endif
    }
}