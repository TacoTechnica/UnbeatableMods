using System;
using System.Collections.Generic;

namespace Utils.ReflectionHelper
{
    /// <summary>
    ///     Makes reflection invoking less icky.
    ///     TODO: Add a means to add custom variables and functions to objects, returning
    ///     a key that can be used on any object just like it's a private field/variable.
    /// </summary>
    public static class ReflectionHelper
    {
        #region Reflection Cache

        private static readonly Dictionary<Type, TypeReflections> ReflectionCache = new Dictionary<Type, TypeReflections>();

        private static TypeReflections GetReflections(Type type)
        {
            if (!ReflectionCache.ContainsKey(type)) ReflectionCache[type] = new TypeReflections(type);
            return ReflectionCache[type];
        }

        private static TypeReflections GetReflections(object instance)
        {
            return GetReflections(instance.GetType());
        }

        #endregion

        #region Invoke Instance

        public static T InvokeMethod<T>(this object instance, MemberKey key, params object[] arguments)
        {
            return (T) GetReflections(instance).InvokeMethod(instance, key.Name, key.Arguments, arguments);
        }

        public static T InvokeMethod<T>(this object instance, string name, params object[] arguments)
        {
            return instance.InvokeMethod<T>(new MemberKey(name), arguments);
        }

        public static T InvokeMethod<T>(this object instance, string name, Type[] argumentTypes,
            params object[] arguments)
        {
            return instance.InvokeMethod<T>(new MemberKey(name, argumentTypes), arguments);
        }

        public static void InvokeMethod(this object instance, MemberKey key, params object[] arguments)
        {
            GetReflections(instance).InvokeMethod(instance, key.Name, key.Arguments, arguments);
        }

        public static void InvokeMethod(this object instance, string name, params object[] arguments)
        {
            instance.InvokeMethod(new MemberKey(name), arguments);
        }

        public static void InvokeMethod(this object instance, string name, Type[] argumentTypes,
            params object[] arguments)
        {
            instance.InvokeMethod(new MemberKey(name, argumentTypes), arguments);
        }

        #endregion

        #region Get Field Instance

        public static T GetField<T>(this object instance, MemberKey key)
        {
            return GetReflections(instance).GetField<T>(instance, key.Name);
        }

        public static T GetField<T>(this object instance, string name)
        {
            return instance.GetField<T>(new MemberKey(name));
        }

        #endregion

        #region Set Field Instance

        public static void SetField(this object instance, MemberKey key, object value)
        {
            GetReflections(instance).SetField(instance, key.Name, value);
        }

        public static void SetField(this object instance, string name, object value)
        {
            instance.SetField(new MemberKey(name), value);
        }

        #endregion


        #region Invoke Static

        public static T InvokeMethodStatic<T>(this Type type, MemberKey key, params object[] arguments)
        {
            return (T) GetReflections(type).InvokeMethodStatic(key.Name, key.Arguments, arguments);
        }

        public static T InvokeMethodStatic<T>(this Type type, string name, params object[] arguments)
        {
            return type.InvokeMethodStatic<T>(new MemberKey(name), arguments);
        }

        public static T InvokeMethodStatic<T>(this Type type, string name, Type[] argumentTypes,
            params object[] arguments)
        {
            return type.InvokeMethodStatic<T>(new MemberKey(name, argumentTypes), arguments);
        }

        public static void InvokeMethodStatic(this Type type, MemberKey key, params object[] arguments)
        {
            GetReflections(type).InvokeMethodStatic(key.Name, key.Arguments, arguments);
        }

        public static void InvokeMethodStatic(this Type type, string name, params object[] arguments)
        {
            type.InvokeMethodStatic(new MemberKey(name), arguments);
        }

        public static void InvokeMethodStatic(this Type type, string name, Type[] argumentTypes,
            params object[] arguments)
        {
            type.InvokeMethodStatic(new MemberKey(name, argumentTypes), arguments);
        }

        #endregion

        #region Get Field Static

        public static T GetFieldStatic<T>(this Type type, MemberKey key)
        {
            return GetReflections(type).GetFieldStatic<T>(key.Name);
        }

        public static T GetFieldStatic<T>(this Type type, string name)
        {
            return type.GetFieldStatic<T>(new MemberKey(name));
        }

        #endregion

        #region Set Field Static

        public static void SetFieldStatic(this Type type, MemberKey key, object value)
        {
            GetReflections(type).SetFieldStatic(key.Name, value);
        }

        public static void SetFieldStatic<T>(this Type type, string name, object value)
        {
            type.SetFieldStatic(new MemberKey(name), value);
        }

        #endregion
    }
}