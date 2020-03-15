using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills.FactorGraphs
{
    /// <summary>
    /// Helper class for computing the factor graph's normalization constant.
    /// </summary>    
    public class FactorList<TValue>
    {        
        private readonly List<Factor<TValue>> _List = new List<Factor<TValue>>();

        public double LogNormalization
        {
            get
            {                
                _List.ForEach(f => f.ResetMarginals());

                double sumLogZ = 0.0;
                                
                for (int i = 0; i < _List.Count; i++)
                {
                    Factor<TValue> f = _List[i];
                    for (int j = 0; j < f.NumberOfMessages; j++)
                    {
                        sumLogZ += f.SendMessage(j);
                    }
                }
                                
                double sumLogS = _List.Aggregate(0.0, (acc, fac) => acc + fac.LogNormalization);

                return sumLogZ + sumLogS;
            }
        }

        public int Count
        {
            get { return _List.Count; }
        }
                
        public Factor<TValue> AddFactor(Factor<TValue> factor)
        {
            _List.Add(factor);
            return factor;
        }
    }
}