﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Utils.ReflectionHelper
{
    public class TypeReflections
    {
        private static readonly BindingFlags _instanceBindingFlags =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static readonly BindingFlags _staticBindingFlags =
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        private readonly Dictionary<MemberKey, FieldInfo> _fields = new Dictionary<MemberKey, FieldInfo>();
        private readonly Dictionary<MemberKey, MethodInfo> _methods = new Dictionary<MemberKey, MethodInfo>();
        private readonly Type _type;

        public TypeReflections(Type type)
        {
            _type = type;
        }

        private object InvokeMethodGeneric(object instance, string name, Type[] argumentTypes,
            BindingFlags bindingFlags, params object[] arguments)
        {
            var key = new MemberKey(name, argumentTypes);
            if (!_methods.ContainsKey(key))
            {
                MethodInfo result;
                if (key.ArgumentsSpecified)
                    result = _type.GetMethod(name, bindingFlags, null, argumentTypes, null);
                else
                    result = _type.GetMethod(name, bindingFlags);

                if (result == null)
                    throw new InvalidOperationException(
                        $"Method {name} with args {argumentTypes} NOT FOUND in class {_type}");
                _methods[key] = result;
            }

            return _methods[key].Invoke(instance, arguments);
        }

        public object InvokeMethod(object instance, string name, Type[] argumentTypes, params object[] arguments)
        {
            return InvokeMethodGeneric(instance, name, argumentTypes, _instanceBindingFlags, arguments);
        }

        public object InvokeMethod(object instance, string name, params object[] arguments)
        {
            return InvokeMethod(instance, name, null, arguments);
        }

        public object InvokeMethodStatic(string name, Type[] argumentTypes,
            params object[] arguments)
        {
            return InvokeMethodGeneric(null, name, argumentTypes, _staticBindingFlags, arguments);
        }

        public object InvokeMethodStatic(string name, params object[] arguments)
        {
            return InvokeMethodStatic(null, name, null, arguments);
        }

        public T GetField<T>(object instance, string name)
        {
            var key = new MemberKey(name);
            if (!_fields.ContainsKey(key))
            {
                var field = _type.GetField(name, _instanceBindingFlags);
                if (field == null)
                    throw new InvalidOperationException(
                        $"Method {name} NOT FOUND in class {_type}");
                _fields[key] = field;
            }

            return (T) _fields[key].GetValue(instance);
        }

        public void SetField(object instance, string name, object value)
        {
            var key = new MemberKey(name);
            if (!_fields.ContainsKey(key))
            {
                var field = _type.GetField(name, _instanceBindingFlags);
                if (field == null)
                    throw new InvalidOperationException(
                        $"Method {name} NOT FOUND in class {_type}");
                _fields[key] = field;
            }

            _fields[key].SetValue(instance, value);
        }

        public T GetFieldStatic<T>(string name)
        {
            var key = new MemberKey(name);
            if (!_fields.ContainsKey(key)) _fields[key] = _type.GetField(name, _staticBindingFlags);

            return (T) _fields[key].GetValue(null);
        }

        public void SetFieldStatic(string name, object value)
        {
            var key = new MemberKey(name);
            if (!_fields.ContainsKey(key)) _fields[key] = _type.GetField(name, _staticBindingFlags);
            _fields[key].SetValue(null, value);
        }
    }
}