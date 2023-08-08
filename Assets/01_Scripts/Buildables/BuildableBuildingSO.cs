using UnityEngine;

namespace RTSSystem.Buildables
{
    [CreateAssetMenu(menuName ="RTS/Buildables/Building",fileName ="BLD_Building")]
    public class BuildableBuildingSO : BuildableObjectBaseSO
    {
#if UNITY_EDITOR
        protected override void InitialiseDefaults()
        {
            BuildableType = Type.Building;
            ObjectName = "Building";
            Description = ObjectName;
            Cost = 0;
            BuildTime = 5f;
            QueueSizeLimit = 1;
            GlobalBuildLimit = -1;
        }
#endif
    }
}
