using RimWorld;
using Verse;

namespace PainAffectsMovement
{
    /// <summary>
    /// Multiplies MoveSpeed based on the pawn's current PainTotal.
    ///
    /// Curve (piecewise linear):
    ///   pain <= 20%           -> no penalty   (factor 1.00)
    ///   20% < pain < 80%      -> linear ramp   (factor 1.00 -> 0.60)
    ///   pain >= 80%           -> capped penalty (factor 0.60, i.e. -40%)
    ///
    /// SimpleCurve.Evaluate() clamps to the first/last point's Y value for
    /// any X outside the defined range, which gives us both the 0-20% buffer
    /// and the 80%+ cap for free.
    /// </summary>
    public class StatPart_PainMoveSpeed : StatPart
    {
        private const float BufferPain = 0.20f;
        private const float MaxPenaltyPain = 0.80f;
        private const float MinFactor = 0.60f; // 1 - 0.40 max penalty

        private static readonly SimpleCurve PainToFactorCurve = new SimpleCurve
        {
            new CurvePoint(BufferPain, 1f),
            new CurvePoint(MaxPenaltyPain, MinFactor),
        };

        public override void TransformValue(StatRequest req, ref float val)
        {
            float factor;
            if (!TryGetFactor(req, out factor))
                return;

            val *= factor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            float factor;
            if (!TryGetFactor(req, out factor) || factor >= 1f)
                return null;

            Pawn pawn = req.Thing as Pawn;
            float pain = pawn.health.hediffSet.PainTotal;

            return "Pain (" + pain.ToStringPercent() + "): x" + factor.ToStringPercent();
        }

        private static bool TryGetFactor(StatRequest req, out float factor)
        {
            factor = 1f;

            if (!req.HasThing)
                return false;

            Pawn pawn = req.Thing as Pawn;
            if (pawn?.health?.hediffSet == null)
                return false;

            float pain = pawn.health.hediffSet.PainTotal;
            if (pain <= BufferPain)
                return false;

            factor = PainToFactorCurve.Evaluate(pain);
            return true;
        }
    }
}
