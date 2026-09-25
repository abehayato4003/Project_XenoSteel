using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Information
{
    public class XenoSteelRadarDetectionResult
    {
        public IUnit Target { get; }

        public int InformationPrecision { get; }

        public XenoSteelRadarDetectionResult(
            IUnit target,
            int informationPrecision)
        {
            Target = target;
            InformationPrecision = informationPrecision;
        }
    }
}