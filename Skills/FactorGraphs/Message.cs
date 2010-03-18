using System;

namespace Moserware.Skills.FactorGraphs
{
    public class Message<T>
    {
        private readonly string _NameFormat;
        private readonly object[] _NameFormatArgs;

        public Message()
            : this(default(T), null, null)
        {
        }

        public Message(T value, string nameFormat, params object[] args)

        {
            _NameFormat = nameFormat;
            _NameFormatArgs = args;
            Value = value;
        }

        public T Value { get; set; }

        public override string ToString()
        {
            return (_NameFormat == null) ? base.ToString() : String.Format(_NameFormat, _NameFormatArgs);
        }
    }
}