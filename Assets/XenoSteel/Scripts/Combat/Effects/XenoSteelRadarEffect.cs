using UnityEngine;
using TurnBasedStrategyFramework.Unity.Units;
using XenoSteel.Information;

namespace XenoSteel.Combat
{
    [CreateAssetMenu(
        fileName = "NewRadarEffect",
        menuName = "XenoSteel/Effects/Radar"
    )]
    public class XenoSteelRadarEffect : XenoSteelEffect
    {
        [Header("レーダー範囲")]
        [Min(1)]
        public int radarRange = 5;

        [Header("レーダー強度")]
        [Min(0)]
        public int radarAccuracy = 5;

        public override void Execute(
            XenoSteelEffectContext context)
        {
            Debug.Log("=== RadarEffect Execute ===");

            if (context == null)
            {
                Debug.Log("RadarEffect: context is null");
                return;
            }

            if (context.InformationManager == null)
            {
                Debug.Log("RadarEffect: InformationManager is null");
                return;
            }


            Unit observer =
                context.User as Unit;

            if (observer == null)
            {
                Debug.Log("RadarEffect: observer is null");
                return;
            }

            XenoSteelRadarSystem radarSystem =
                new XenoSteelRadarSystem();

            radarSystem.UpdateRadarInformation(
                observer,
                context.GridController,
                context.InformationManager,
                context.CurrentRound,
                radarRange,
                radarAccuracy
            );

            XenoSteelVisionManager visionManager =
                Object.FindFirstObjectByType<XenoSteelVisionManager>();

            if (visionManager != null)
            {
                visionManager.UpdateTacticalMapEnemyMarkers();
            }

            Debug.Log(
                $"Radar Effect Execute: " +
                $"Range={radarRange}, " +
                $"Accuracy={radarAccuracy}"
            );
        }
    }
}