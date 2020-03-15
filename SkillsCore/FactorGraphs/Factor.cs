using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Moserware.Skills.FactorGraphs
{    
    public abstract class Factor<TValue>        
    {
        private readonly List<Message<TValue>> _Messages = new List<Message<TValue>>();

        private readonly Dictionary<Message<TValue>, Variable<TValue>> _MessageToVariableBinding =
            new Dictionary<Message<TValue>, Variable<TValue>>();

        private readonly string _Name;
        private readonly List<Variable<TValue>> _Variables = new List<Variable<TValue>>();

        protected Factor(string name)
        {
            _Name = "Factor[" + name + "]";
        }

        /// Returns the log-normalization constant of that factor
        public virtual double LogNormalization
        {
            get { return 0; }
        }

        /// Returns the number of messages that the factor has
        public int NumberOfMessages
        {
            get { return _Messages.Count; }
        }

        protected ReadOnlyCollection<Variable<TValue>> Variables
        {
            get { return _Variables.AsReadOnly(); }
        }

        protected ReadOnlyCollection<Message<TValue>> Messages
        {
            get { return _Messages.AsReadOnly(); }
        }

        /// Update the message and marginal of the i-th variable that the factor is connected to
        public virtual double UpdateMessage(int messageIndex)
        {
            Guard.ArgumentIsValidIndex(messageIndex, _Messages.Count, "messageIndex");
            return UpdateMessage(_Messages[messageIndex], _MessageToVariableBinding[_Messages[messageIndex]]);
        }

        protected virtual double UpdateMessage(Message<TValue> message, Variable<TValue> variable)
        {
            throw new NotImplementedException();
        }

        /// Resets the marginal of the variables a factor is connected to
        public virtual void ResetMarginals()
        {
            foreach (var currentVariable in _MessageToVariableBinding.Values)
            {
                currentVariable.ResetToPrior();
            }
        }

        /// Sends the ith message to the marginal and returns the log-normalization constant
        public virtual double SendMessage(int messageIndex)
        {
            Guard.ArgumentIsValidIndex(messageIndex, _Messages.Count, "messageIndex");

            Message<TValue> message = _Messages[messageIndex];
            Variable<TValue> variable = _MessageToVariableBinding[message];
            return SendMessage(message, variable);
        }

        protected abstract double SendMessage(Message<TValue> message, Variable<TValue> variable);

        public abstract Message<TValue> CreateVariableToMessageBinding(Variable<TValue> variable);

        protected Message<TValue> CreateVariableToMessageBinding(Variable<TValue> variable, Message<TValue> message)
        {
            int index = _Messages.Count;
            _Messages.Add(message);
            _MessageToVariableBinding[message] = variable;
            _Variables.Add(variable);

            return message;
        }

        public override string ToString()
        {
            return _Name ?? base.ToString();
        }
    }
}