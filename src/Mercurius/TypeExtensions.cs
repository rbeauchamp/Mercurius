using System;
using System.Reflection;

namespace Mercurius
{
    public static class TypeExtensions
    {
        public static bool IsInstanceOfType(this Type type, object instance)
        {
            return instance != null && type.GetTypeInfo().IsAssignableFrom(instance.GetType().GetTypeInfo());
        }

        /// <summary>
        /// Returns true of this class is a true subclass of c. Everything
        /// else returns false.  If this class and c are the same class false is 
        /// returned.
        /// </summary>
        public static bool IsSubclassOf(this Type type, Type c)
        {
            var p = type;
            if (p == c)
                return false;
            while (p != null)
            {
                if (p == c)
                    return true;
                p = p.GetTypeInfo().BaseType;
            }
            return false;
        }
    }
}
