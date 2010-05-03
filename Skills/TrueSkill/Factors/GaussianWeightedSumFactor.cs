using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors
{
    /// <summary>
    /// Factor that sums together multiple Gaussians.
    /// </summary>
    /// <remarks>See the accompanying math paper for more details.</remarks>
    public class GaussianWeightedSumFactor : GaussianFactor
    {
        private readonly List<int[]> _VariableIndexOrdersForWeights = new List<int[]>();

        // This following is used for convenience, for example, the first entry is [0, 1, 2] 
        // corresponding to v[0] = a1*v[1] + a2*v[2]
        private readonly double[][] _Weights;
        private readonly double[][] _WeightsSquared;        

        public GaussianWeightedSumFactor(Variable<GaussianDistribution> sumVariable,
                                         Variable<GaussianDistribution>[] variablesToSum)
            : this(sumVariable,
                   variablesToSum,
                   variablesToSum.Select(v => 1.0).ToArray()) // By default, set the weight to 1.0
        {
        }

        public GaussianWeightedSumFactor(Variable<GaussianDistribution> sumVariable,
                                         Variable<GaussianDistribution>[] variablesToSum, double[] variableWeights)
            : base(CreateName(sumVariable, variablesToSum, variableWeights))
        {
            _Weights = new double[variableWeights.Length + 1][];
            _WeightsSquared = new double[_Weights.Length][];

            // The first weights are a straightforward copy
            // v_0 = a_1*v_1 + a_2*v_2 + ... + a_n * v_n
            _Weights[0] = new double[variableWeights.Length];
            Array.Copy(variableWeights, _Weights[0], variableWeights.Length);
            _WeightsSquared[0] = _Weights[0].Select(w => w*w).ToArray();

            // 0..n-1
            _VariableIndexOrdersForWeights.Add(Enumerable.Range(0, 1 + variablesToSum.Length).ToArray());


            // The rest move the variables around and divide out the constant. 
            // For example:
            // v_1 = (-a_2 / a_1) * v_2 + (-a3/a1) * v_3 + ... + (1.0 / a_1) * v_0
            // By convention, we'll put the v_0 term at the end

            for (int weightsIndex = 1; weightsIndex < _Weights.Length; weightsIndex++)
            {
                var currentWeights = new double[variableWeights.Length];
                _Weights[weightsIndex] = currentWeights;

                var variableIndices = new int[variableWeights.Length + 1];
                variableIndices[0] = weightsIndex;

                var currentWeightsSquared = new double[variableWeights.Length];
                _WeightsSquared[weightsIndex] = currentWeightsSquared;

                // keep a single variable to keep track of where we are in the array.
                // This is helpful since we skip over one of the spots
                int currentDestinationWeightIndex = 0;

                for (int currentWeightSourceIndex = 0;
                     currentWeightSourceIndex < variableWeights.Length;
                     currentWeightSourceIndex++)
                {                    
                    if (currentWeightSourceIndex == (weightsIndex - 1))
                    {
                        continue;
                    }

                    double currentWeight = (-variableWeights[currentWeightSourceIndex]/variableWeights[weightsIndex - 1]);

                    if (variableWeights[weightsIndex - 1] == 0)
                    {
                        // HACK: Getting around division by zero
                        currentWeight = 0;
                    }

                    currentWeights[currentDestinationWeightIndex] = currentWeight;
                    currentWeightsSquared[currentDestinationWeightIndex] = currentWeight*currentWeight;

                    variableIndices[currentDestinationWeightIndex + 1] = currentWeightSourceIndex + 1;
                    currentDestinationWeightIndex++;
                }

                // And the final one
                double finalWeight = 1.0/variableWeights[weightsIndex - 1];

                if (variableWeights[weightsIndex - 1] == 0)
                {
                    // HACK: Getting around division by zero
                    finalWeight = 0;
                }
                currentWeights[currentDestinationWeightIndex] = finalWeight;
                currentWeightsSquared[currentDestinationWeightIndex] = finalWeight*finalWeight;
                variableIndices[variableIndices.Length - 1] = 0;
                _VariableIndexOrdersForWeights.Add(variableIndices);
            }

            CreateVariableToMessageBinding(sumVariable);

            foreach (var currentVariable in variablesToSum)
            {
                CreateVariableToMessageBinding(currentVariable);
            }
        }

        public override double LogNormalization
        {
            get
            {
                ReadOnlyCollection<Variable<GaussianDistribution>> vars = Variables;
                ReadOnlyCollection<Message<GaussianDistribution>> messages = Messages;

                double result = 0.0;

                // We start at 1 since offset 0 has the sum
                for (int i = 1; i < vars.Count; i++)
                {
                    result += GaussianDistribution.LogRatioNormalization(vars[i].Value, messages[i].Value);
                }

                return result;
            }
        }

        private double UpdateHelper(double[] weights, double[] weightsSquared,
                                    IList<Message<GaussianDistribution>> messages,
                                    IList<Variable<GaussianDistribution>> variables)
        {
            // Potentially look at http://mathworld.wolfram.com/NormalSumDistribution.html for clues as 
            // to what it's doing

            GaussianDistribution message0 = messages[0].Value.Clone();
            GaussianDistribution marginal0 = variables[0].Value.Clone();

            // The math works out so that 1/newPrecision = sum of a_i^2 /marginalsWithoutMessages[i]
            double inverseOfNewPrecisionSum = 0.0;
            double anotherInverseOfNewPrecisionSum = 0.0;
            double weightedMeanSum = 0.0;
            double anotherWeightedMeanSum = 0.0;

            for (int i = 0; i < weightsSquared.Length; i++)
            {
                // These flow directly from the paper

                inverseOfNewPrecisionSum += weightsSquared[i]/
                                            (variables[i + 1].Value.Precision - messages[i + 1].Value.Precision);

                GaussianDistribution diff = (variables[i + 1].Value/messages[i + 1].Value);
                anotherInverseOfNewPrecisionSum += weightsSquared[i]/diff.Precision;

                weightedMeanSum += weights[i]
                                   *
                                   (variables[i + 1].Value.PrecisionMean - messages[i + 1].Value.PrecisionMean)
                                   /
                                   (variables[i + 1].Value.Precision - messages[i + 1].Value.Precision);

                anotherWeightedMeanSum += weights[i]*diff.PrecisionMean/diff.Precision;
            }

            double newPrecision = 1.0/inverseOfNewPrecisionSum;
            double anotherNewPrecision = 1.0/anotherInverseOfNewPrecisionSum;

            double newPrecisionMean = newPrecision*weightedMeanSum;
            double anotherNewPrecisionMean = anotherNewPrecision*anotherWeightedMeanSum;

            GaussianDistribution newMessage = GaussianDistribution.FromPrecisionMean(newPrecisionMean, newPrecision);
            GaussianDistribution oldMarginalWithoutMessage = marginal0/message0;

            GaussianDistribution newMarginal = oldMarginalWithoutMessage*newMessage;

            /// Update the message and marginal

            messages[0].Value = newMessage;
            variables[0].Value = newMarginal;

            /// Return the difference in the new marginal
            return newMarginal - marginal0;
        }

        public override double UpdateMessage(int messageIndex)
        {
            ReadOnlyCollection<Message<GaussianDistribution>> allMessages = Messages;
            ReadOnlyCollection<Variable<GaussianDistribution>> allVariables = Variables;

            Guard.ArgumentIsValidIndex(messageIndex, allMessages.Count, "messageIndex");

            var updatedMessages = new List<Message<GaussianDistribution>>();
            var updatedVariables = new List<Variable<GaussianDistribution>>();

            int[] indicesToUse = _VariableIndexOrdersForWeights[messageIndex];

            // The tricky part here is that we have to put the messages and variables in the same
            // order as the weights. Thankfully, the weights and messages share the same index numbers,
            // so we just need to make sure they're consistent
            for (int i = 0; i < allMessages.Count; i++)
            {
                updatedMessages.Add(allMessages[indicesToUse[i]]);
                updatedVariables.Add(allVariables[indicesToUse[i]]);
            }

            return UpdateHelper(_Weights[messageIndex], _WeightsSquared[messageIndex], updatedMessages, updatedVariables);
        }

        private static string CreateName(Variable<GaussianDistribution> sumVariable,
                                         IList<Variable<GaussianDistribution>> variablesToSum, double[] weights)
        {
            var sb = new StringBuilder();
            sb.Append(sumVariable.ToString());
            sb.Append(" = ");
            for (int i = 0; i < variablesToSum.Count; i++)
            {
                bool isFirst = (i == 0);

                if (isFirst && (weights[i] < 0))
                {
                    sb.Append("-");
                }

                sb.Append(Math.Abs(weights[i]).ToString("0.00"));
                sb.Append("*[");
                sb.Append(variablesToSum[i]);
                sb.Append("]");

                bool isLast = (i == variablesToSum.Count - 1);

                if (!isLast)
                {
                    if (weights[i + 1] >= 0)
                    {
                        sb.Append(" + ");
                    }
                    else
                    {
                        sb.Append(" - ");
                    }
                }
            }

            return sb.ToString();
        }
    }
}