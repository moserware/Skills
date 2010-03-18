using System;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors
{
    /// <summary>
    /// Connects two variables and adds uncertainty.
    /// </summary>
    /// <remarks>See the accompanying math paper for more details.</remarks>
    public class GaussianLikelihoodFactor : GaussianFactor
    {
        private readonly double _Precision;

        public GaussianLikelihoodFactor(double betaSquared, Variable<GaussianDistribution> variable1,
                                        Variable<GaussianDistribution> variable2)
            : base(String.Format("Likelihood of {0} going to {1}", variable2, variable1))
        {
            _Precision = 1.0/betaSquared;
            CreateVariableToMessageBinding(variable1);
            CreateVariableToMessageBinding(variable2);
        }

        public override double LogNormalization
        {
            get { return GaussianDistribution.LogRatioNormalization(Variables[0].Value, Messages[0].Value); }
        }

        private double UpdateHelper(Message<GaussianDistribution> message1, Message<GaussianDistribution> message2,
                                    Variable<GaussianDistribution> variable1, Variable<GaussianDistribution> variable2)
        {
            GaussianDistribution message1Value = message1.Value.Clone();
            GaussianDistribution message2Value = message2.Value.Clone();

            GaussianDistribution marginal1 = variable1.Value.Clone();
            GaussianDistribution marginal2 = variable2.Value.Clone();

            double a = _Precision/(_Precision + marginal2.Precision - message2Value.Precision);

            GaussianDistribution newMessage = GaussianDistribution.FromPrecisionMean(
                a*(marginal2.PrecisionMean - message2Value.PrecisionMean),
                a*(marginal2.Precision - message2Value.Precision));

            GaussianDistribution oldMarginalWithoutMessage = marginal1/message1Value;

            GaussianDistribution newMarginal = oldMarginalWithoutMessage*newMessage;

            /// Update the message and marginal

            message1.Value = newMessage;
            variable1.Value = newMarginal;

            /// Return the difference in the new marginal
            return newMarginal - marginal1;
        }

        public override double UpdateMessage(int messageIndex)
        {
            switch (messageIndex)
            {
                case 0:
                    return UpdateHelper(Messages[0], Messages[1],
                                        Variables[0], Variables[1]);
                case 1:
                    return UpdateHelper(Messages[1], Messages[0],
                                        Variables[1], Variables[0]);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}