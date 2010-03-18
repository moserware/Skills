using System;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors
{
    /// <summary>
    /// Supplies the factor graph with prior information.
    /// </summary>
    /// <remarks>See the accompanying math paper for more details.</remarks>
    public class GaussianPriorFactor : GaussianFactor
    {
        private readonly GaussianDistribution _NewMessage;

        public GaussianPriorFactor(double mean, double variance, Variable<GaussianDistribution> variable)
            : base(String.Format("Prior value going to {0}", variable))
        {
            _NewMessage = new GaussianDistribution(mean, Math.Sqrt(variance));
            CreateVariableToMessageBinding(variable,
                                           new Message<GaussianDistribution>(
                                               GaussianDistribution.FromPrecisionMean(0, 0), "message from {0} to {1}",
                                               this, variable));
        }

        protected override double UpdateMessage(Message<GaussianDistribution> message,
                                                Variable<GaussianDistribution> variable)
        {
            GaussianDistribution oldMarginal = variable.Value.Clone();
            Message<GaussianDistribution> oldMessage = message;
            GaussianDistribution newMarginal =
                GaussianDistribution.FromPrecisionMean(
                    oldMarginal.PrecisionMean + _NewMessage.PrecisionMean - oldMessage.Value.PrecisionMean,
                    oldMarginal.Precision + _NewMessage.Precision - oldMessage.Value.Precision);
            variable.Value = newMarginal;
            message.Value = _NewMessage;
            return oldMarginal - newMarginal;
        }
    }
}