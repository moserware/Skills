using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors
{
    public abstract class GaussianFactor : Factor<GaussianDistribution>
    {
        protected GaussianFactor(string name)
            : base(name)
        {
        }

        /// Sends the factor-graph message with and returns the log-normalization constant        
        protected override double SendMessage(Message<GaussianDistribution> message,
                                              Variable<GaussianDistribution> variable)
        {
            GaussianDistribution marginal = variable.Value;
            GaussianDistribution messageValue = message.Value;
            double logZ = GaussianDistribution.LogProductNormalization(marginal, messageValue);
            variable.Value = marginal*messageValue;
            return logZ;
        }

        public override Message<GaussianDistribution> CreateVariableToMessageBinding(
            Variable<GaussianDistribution> variable)
        {
            return CreateVariableToMessageBinding(variable,
                                                  new Message<GaussianDistribution>(
                                                      GaussianDistribution.FromPrecisionMean(0, 0),
                                                      "message from {0} to {1}", this, variable));
        }
    }
}