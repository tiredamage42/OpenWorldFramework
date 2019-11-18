using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;
using System.Linq;

using System.Reflection;
using System.Collections.Generic;
namespace UnityTools {

    public static class SystemTools 
    {   
        
        public static Type[] FindDerivedTypes(this Type type, bool includeSelf)
        {
            return type.Assembly.GetTypes().Where(t => (t != type || includeSelf) && type.IsAssignableFrom(t)).ToArray();
        }

        public static Type[] FindAllDerivedTypes(this Type type)
        {
            Assembly assembly = Assembly.LoadFrom(type.Assembly.Location);
            Type[] types = assembly.GetTypes();
            List<Type> results = new List<Type>();
            GetAllDerivedTypesRecursively(types, type, ref results);
            return results.Where( t => !t.IsAbstract ).ToArray();
        }

        static void GetAllDerivedTypesRecursively(Type[] types, Type type1, ref List<Type> results) {
            if (type1.IsGenericType)
                GetDerivedFromGeneric(types, type1, ref results);
            else
                GetDerivedFromNonGeneric(types, type1, ref results);
        }
        static void GetDerivedFromGeneric(Type[] types, Type type, ref List<Type> results) {
            var derivedTypes = types.Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == type);
            results.AddRange(derivedTypes);
            foreach (Type derivedType in derivedTypes)
                GetAllDerivedTypesRecursively(types, derivedType, ref results);
        }
        static void GetDerivedFromNonGeneric(Type[] types, Type type, ref List<Type> results) {
            var derivedTypes = types.Where(t => t != type && type.IsAssignableFrom(t));
            results.AddRange(derivedTypes);
            foreach (Type derivedType in derivedTypes)
                GetAllDerivedTypesRecursively(types, derivedType, ref results);
        } 

        /*
            gets a string hash code that will persist between runs
        */
        public static int GetPersistentHashCode(this string str)
        {
            /*
                'unchecked' keyword disables overflow-checking for the integer arithmetic done inside the function. 
                If the function was executed inside a checked context, you might get an OverflowException at runtime
            */
            unchecked
            {
                
                // int hash1 = 5381;
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;
                // for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    // if (i == str.Length - 1 || str[i+1] == '\0')
                    if (i == str.Length - 1)
                        break;

                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }
                return hash1 + (hash2 * 1566083941);
            }
        }

        public static T StringToEnum<T>(string value, T defValue)
		{
			if(string.IsNullOrEmpty(value))
				return defValue;
            
            try {
				return (T)Enum.Parse(typeof(T), value, true);
			}
			catch {
				return defValue;
			}
		}

		
        public static void SaveToFile (object obj, string filePath) {
            using (FileStream file = File.Create(filePath))
            {
                using (GZipStream compressed = new GZipStream(file, CompressionMode.Compress))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        new BinaryFormatter().Serialize(ms, obj);
                        byte[] bytesToCompress = ms.ToArray();
                        compressed.Write(bytesToCompress, 0, bytesToCompress.Length);
                    }
                }
            }
        }

        public static object LoadFromFile (string filePath) {
            object obj = null;
            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                using (GZipStream decompressed = new GZipStream(file, CompressionMode.Decompress))
                {
                    obj = new BinaryFormatter().Deserialize(decompressed);
                }
            }
            return obj;
        }
        
    }
}
