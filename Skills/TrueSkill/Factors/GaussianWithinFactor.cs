using System;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors
{
    /// <summary>
    /// Factor representing a team difference that has not exceeded the draw margin.
    /// </summary>
    /// <remarks>See the accompanying math paper for more details.</remarks>
    public class GaussianWithinFactor : GaussianFactor
    {
        private readonly double _Epsilon;

        public GaussianWithinFactor(double epsilon, Variable<GaussianDistribution> variable)
            : base(String.Format("{0} <= {1:0.000}", variable, epsilon))
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
                double mean = messageFromVariable.Mean;
                double std = messageFromVariable.StandardDeviation;
                double z = GaussianDistribution.CumulativeTo((_Epsilon - mean)/std)
                           -
                           GaussianDistribution.CumulativeTo((-_Epsilon - mean)/std);

                return -GaussianDistribution.LogProductNormalization(messageFromVariable, message) + Math.Log(z);
            }
        }

        protected override double UpdateMessage(Message<GaussianDistribution> message,
                                                Variable<GaussianDistribution> variable)
        {
            GaussianDistribution oldMarginal = variable.Value.Clone();
            GaussianDistribution oldMessage = message.Value.Clone();
            GaussianDistribution messageFromVariable = oldMarginal/oldMessage;

            double c = messageFromVariable.Precision;
            double d = messageFromVariable.PrecisionMean;

            double sqrtC = Math.Sqrt(c);
            double dOnSqrtC = d/sqrtC;

            double epsilonTimesSqrtC = _Epsilon*sqrtC;
            d = messageFromVariable.PrecisionMean;

            double denominator = 1.0 - TruncatedGaussianCorrectionFunctions.WWithinMargin(dOnSqrtC, epsilonTimesSqrtC);
            double newPrecision = c/denominator;
            double newPrecisionMean = (d +
                                       sqrtC*
                                       TruncatedGaussianCorrectionFunctions.VWithinMargin(dOnSqrtC, epsilonTimesSqrtC))/
                                      denominator;

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