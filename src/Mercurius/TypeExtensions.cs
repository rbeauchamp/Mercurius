﻿using System;
using System.Reflection;

namespace Mercurius
{
    public static class TypeExtensions
    {
        public static bool IsInstanceOfType(this Type type, object obj)
        {
            return obj != null && type.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo());
        }
    }
}
