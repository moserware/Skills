using System;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors
{
    /// <summary>
    /// Factor representing a team difference that has exceeded the draw margin.
    /// </summary>
    /// <remarks>See the accompanying math paper for more details.</remarks>
    public class GaussianGreaterThanFactor : GaussianFactor
    {
        private readonly double _Epsilon;

        public GaussianGreaterThanFactor(double epsilon, Variable<GaussianDistribution> variable)
            : base(String.Format("{0} > {1:0.000}", variable, epsilon))
        {
            _Epsilon = epsilon;
            CreateVariableToMessageBinding(variable);
        }

        public override double LogNormalization
        {
            get
            {
                GaussianDistribution marginal = Variables[0].Value;
                GaussianDistribution message = Messages[0].Value;
                GaussianDistribution messageFromVariable = marginal/message;
                return -GaussianDistribution.LogProductNormalization(messageFromVariable, message)
                       +
                       Math.Log(
                           GaussianDistribution.CumulativeTo((messageFromVariable.Mean - _Epsilon)/
                                                             messageFromVariable.StandardDeviation));
            }
        }

        protected override double UpdateMessage(Message<GaussianDistribution> message,
                                                Variable<GaussianDistribution> variable)
        {
            GaussianDistribution oldMarginal = variable.Value.Clone();
            GaussianDistribution oldMessage = message.Value.Clone();
            GaussianDistribution messageFromVar = oldMarginal/oldMessage;

            double c = messageFromVar.Precision;
            double d = messageFromVar.PrecisionMean;

            double sqrtC = Math.Sqrt(c);

            double dOnSqrtC = d/sqrtC;

            double epsilsonTimesSqrtC = _Epsilon*sqrtC;
            d = messageFromVar.PrecisionMean;

            double denom = 1.0 - TruncatedGaussianCorrectionFunctions.WExceedsMargin(dOnSqrtC, epsilsonTimesSqrtC);

            double newPrecision = c/denom;
            double newPrecisionMean = (d +
                                       sqrtC*
                                       TruncatedGaussianCorrectionFunctions.VExceedsMargin(dOnSqrtC, epsilsonTimesSqrtC))/
                                      denom;

            GaussianDistribution newMarginal = GaussianDistribution.FromPrecisionMean(newPrecisionMean, newPrecision);

            GaussianDistribution newMessage = oldMessage*newMarginal/oldMarginal;

            /// Update the message and marginal
            message.Value = newMessage;

            variable.Value = newMarginal;

            /// Return the difference in the new marginal
            return newMarginal - oldMarginal;
        }
    }
}