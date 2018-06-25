
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

public class ReflectionTool
{
    public static System.Object CreateInstance(string typeName, System.Object[] functionParam)
    {
        System.Type type = System.Type.GetType(typeName);
        var obj = type.GetConstructor(Type.EmptyTypes).Invoke(functionParam);
        return obj;
    }

    //--------------------------------------------------------------------------//

    public static System.Object CallStaticFunction(string strType, string functionName, System.Object[] functionParam)
    {
        System.Type type = System.Type.GetType(strType);
        return CallStaticFunction(type, functionName, functionParam);
    }

    public static System.Object CallStaticFunction(System.Type type, string functionName, System.Object[] functionParam)
    {
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.MethodInfo methodInfo = type.GetMethod(functionName, bindingAttr);

        while (methodInfo == null && type.BaseType != null)
        {
            type = type.BaseType;
            methodInfo = type.GetMethod(functionName, bindingAttr);
        }

        return methodInfo.Invoke(null, functionParam);
    }

    //--------------------------------------------------------------------------//

    public static System.Object CallInstanceFunction(System.Object instance, string functionName, System.Object[] functionParam)
    {
        try
        {
            System.Type type = instance.GetType();
            System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            System.Reflection.MethodInfo methodInfo = type.GetMethod(functionName, bindingAttr);

            while (methodInfo == null && type.BaseType != null)
            {
                type = type.BaseType;
                methodInfo = type.GetMethod(functionName, bindingAttr);
            }

            return methodInfo.Invoke(instance, functionParam);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return null;
    }

    //--------------------------------------------------------------------------//

    public static System.Object GetStaticFieldValue(string typeName, string fieldName)
    {
        System.Type type = System.Type.GetType(typeName);
        return GetStaticFieldValue(type, fieldName);
    }

    public static System.Object GetStaticFieldValue(System.Type type, string fieldName)
    {
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.FieldInfo fileNameField = type.GetField(fieldName, bindingAttr);

        while (fileNameField == null && type.BaseType != null)
        {
            type = type.BaseType;
            fileNameField = type.GetField(fieldName, bindingAttr);
        }

        return fileNameField.GetValue(null);
    }

    public static void SetStaticFieldValue(string typeName, string fieldName, System.Object value)
    {
        System.Type type = System.Type.GetType(typeName);
        SetStaticFieldValue(type, fieldName, value);
    }

    public static void SetStaticFieldValue(System.Type type, string fieldName, System.Object value)
    {
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.FieldInfo fileNameField = type.GetField(fieldName, bindingAttr);

        while (fileNameField == null && type.BaseType != null)
        {
            type = type.BaseType;
            fileNameField = type.GetField(fieldName, bindingAttr);
        }

        fileNameField.SetValue(null, value);
    }

    //--------------------------------------------------------------------------//

    public static System.Object GetInstanceFieldValue(System.Object instance, string fieldName)
    {
        System.Type type = instance.GetType();
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.FieldInfo fileNameField = type.GetField(fieldName, bindingAttr);

        while (fileNameField == null && type.BaseType != null)
        {
            type = type.BaseType;
            fileNameField = type.GetField(fieldName, bindingAttr);
        }

        return fileNameField.GetValue(instance);
    }

    public static void SetInstanceFieldValue(System.Object instance, string fieldName, System.Object value)
    {
        System.Type type = instance.GetType();
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.FieldInfo fileNameField = type.GetField(fieldName, bindingAttr);

        while (fileNameField == null && type.BaseType != null)
        {
            type = type.BaseType;
            fileNameField = type.GetField(fieldName, bindingAttr);
        }

        fileNameField.SetValue(instance, value);
    }

    //--------------------------------------------------------------------------//

    public static System.Object GetStaticPropertyValue(string typeName, string propertyName)
    {
        System.Type type = System.Type.GetType(typeName);
        return GetStaticPropertyValue(type, propertyName);
    }

    public static System.Object GetStaticPropertyValue(System.Type type, string propertyName)
    {
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingAttr);

        while (propertyInfo == null && type.BaseType != null)
        {
            type = type.BaseType;
            propertyInfo = type.GetProperty(propertyName, bindingAttr);
        }

        return propertyInfo.GetValue(null, null);
    }

    public static void SetStaticPropertyValue(string typeName, string propertyName, System.Object value)
    {
        var type = System.Type.GetType(typeName);
        SetStaticPropertyValue(type, propertyName, value);
    }

    public static void SetStaticPropertyValue(System.Type type, string propertyName, System.Object value)
    {
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingAttr);

        while (propertyInfo == null && type.BaseType != null)
        {
            type = type.BaseType;
            propertyInfo = type.GetProperty(propertyName, bindingAttr);
        }

        propertyInfo.SetValue(null, value, null);
    }

    //--------------------------------------------------------------------------//

    public static System.Object GetInstancePropertyValue(System.Object instance, string propertyName)
    {
        System.Type type = instance.GetType();
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingAttr);

        while (propertyInfo == null && type.BaseType != null)
        {
            type = type.BaseType;
            propertyInfo = type.GetProperty(propertyName, bindingAttr);
        }

        return propertyInfo.GetValue(instance, null);
    }

    public static void SetInstancePropertyValue(System.Object instance, string propertyName, System.Object value)
    {
        System.Type type = instance.GetType();
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        System.Reflection.PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingAttr);

        while (propertyInfo == null && type.BaseType != null)
        {
            type = type.BaseType;
            propertyInfo = type.GetProperty(propertyName, bindingAttr);
        }

        propertyInfo.SetValue(instance, value, null);
    }

    //--------------------------------------------------------------------------//

    public static EventInfo GetEventInfo(System.Type type, string eventName)
    {
        var flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        EventInfo ei = type.GetEvent(eventName, flags);
        while(ei == null && type.BaseType != null)
        {
            type = type.BaseType;
            ei = type.GetEvent(eventName, flags);
        }

        return ei;
    }

    public static EventInfo GetInstanceEventInfo(System.Type type, string eventName)
    {
        var flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        EventInfo ei = type.GetEvent(eventName, flags);
        while (ei == null && type.BaseType != null)
        {
            type = type.BaseType;
            ei = type.GetEvent(eventName, flags);
        }

        return ei;
    }

    public static EventInfo GetStaticEventInfo(System.Type type, string eventName)
    {
        var flags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        EventInfo ei = type.GetEvent(eventName, flags);
        while (ei == null && type.BaseType != null)
        {
            type = type.BaseType;
            ei = type.GetEvent(eventName, flags);
        }

        return ei;
    }

    //--------------------------------------------------------------------------//

    public static MethodInfo GetMethodInfo(System.Type type, string methodName)
    {
        System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

        System.Reflection.MethodInfo methodInfo = type.GetMethod(methodName, bindingAttr);

        while (methodInfo == null && type.BaseType != null)
        {
            type = type.BaseType;
            methodInfo = type.GetMethod(methodName, bindingAttr);
        }

        return methodInfo;
    }
}
