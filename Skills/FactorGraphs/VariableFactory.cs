using System;
using System.Collections.Generic;

namespace Moserware.Skills.FactorGraphs
{
    public class VariableFactory<TValue>
    {
        // using a Func<TValue> to encourage fresh copies in case it's overwritten
        private readonly Func<TValue> _VariablePriorInitializer;

        public VariableFactory(Func<TValue> variablePriorInitializer)
        {
            _VariablePriorInitializer = variablePriorInitializer;
        }

        public Variable<TValue> CreateBasicVariable(string nameFormat, params object[] args)
        {
            var newVar = new Variable<TValue>(
                String.Format(nameFormat, args),                                
                _VariablePriorInitializer());

            return newVar;
        }

        public KeyedVariable<TKey, TValue> CreateKeyedVariable<TKey>(TKey key, string nameFormat, params object[] args)
        {
            var newVar = new KeyedVariable<TKey, TValue>(
                key,
                String.Format(nameFormat, args),                                
                _VariablePriorInitializer());
            
            return newVar;
        }
    }
}