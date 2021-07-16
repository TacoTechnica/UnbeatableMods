using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CustomBeatmaps.ReflectionHelper
{
    public class MemberKey
    {
        public readonly Type[] Arguments;
        public readonly string Name;

        public MemberKey(string name, Type[] arguments = null)
        {
            Name = name;
            Arguments = arguments;
        }

        public bool ArgumentsSpecified => Arguments != null;

        public override bool Equals(object? obj)
        {
            if (obj is MemberKey other)
                return other.Name == Name && other.Arguments == null == (Arguments == null) &&
                       (other.Arguments == null || Arguments == null || other.Arguments.SequenceEqual(Arguments));

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() * 1239 + (Arguments != null
                ? ((IStructuralEquatable) Arguments).GetHashCode(EqualityComparer<Type>.Default) * 777
                : 0);
        }
    }
}