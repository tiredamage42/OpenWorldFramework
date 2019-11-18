// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Reflection;
using System;

using UnityEditor;
using UnityTools.EditorTools;
namespace UnityTools.Internal {
    public enum RunTarget { Subject, Target, Reference, Static, Player };

    

    public static class Messaging {


        public static bool PrepareForMessageSend (
            string callMethod, 
            RunTarget runTarget, 
            GameObject subject, 
            GameObject target, 
            GameObject referenceTarget,
            MessageParameters parameters,
            out GameObject obj,
            out object[] suppliedParameters
        ) {

            suppliedParameters = new object[0];
            obj = subject;

            if (string.IsNullOrEmpty(callMethod) || string.IsNullOrWhiteSpace(callMethod)) {
                Debug.LogWarning("Call Method is blank...");
                return false;
            }

            if (runTarget == RunTarget.Player && !GameManager.playerExists) {
                Debug.LogWarning("Cant Call Method: " + callMethod + " on player, player doesnt exist yet!");
                return false;
            }


            switch (runTarget) {
                case RunTarget.Subject: 
                    obj = subject; 
                    break;
                case RunTarget.Target: 
                    obj = target; 
                    break;
                case RunTarget.Reference: 
                    obj = referenceTarget; 
                    break;
                case RunTarget.Static: 
                    obj = null; 
                    break;
                case RunTarget.Player: 
                    obj = GameManager.playerActor.gameObject; 
                    break;
            }

            if (obj == null && runTarget != RunTarget.Static) {
                Debug.LogWarning("RunTarget: " + runTarget.ToString() + " is null, can't call method: " + callMethod);
                return false;
            }
            
            
            if (parameters.Length > 0) {

                List<object> parametersList = new List<object>();
                for (int i = 0; i < parameters.Length; i++) {
                    if (parameters[i] != null) 
                        parametersList.Add(parameters[i].GetParamObject());
                }
                suppliedParameters = parametersList.ToArray();
            }

            return true;
        }









        

        static Dictionary<int, Component[]> componentsPerGameObject = new Dictionary<int, Component[]>();
        static Component[] GetComponentsForGameObject (GameObject g) {
            int id = g.GetInstanceID();
            Component[] components;
            if (!componentsPerGameObject.TryGetValue(id, out components)) {
                components = g.GetComponents<Component>();
                componentsPerGameObject[id] = components;
            }
            return components;
        }
        
        public static bool CallMethod (this GameObject g, string callMethod, object[] parameters, out float value) {

            Component[] components = GetComponentsForGameObject(g);
            
            for (int i = 0; i < components.Length; i++)
                if (TryAndCallMethod ( components[i].GetType(), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, callMethod, components[i], parameters, out value, i == components.Length - 1, "Run Target: " + g.name))
                    return true;

            value = 0;
            return false;
        }
        public static bool CallMethod (this GameObject g, string callMethod, object[] parameters) {

            Component[] components = GetComponentsForGameObject(g);
            
            for (int i = 0; i < components.Length; i++)
                if (TryAndCallMethod ( components[i].GetType(), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, callMethod, components[i], parameters, i == components.Length - 1, "Run Target: " + g.name))
                    return true;

            return false;
        }  

        static bool SeperateClassNameAndMethod (string callClassAndMethod, out Type classType, out string callMethod) {
            
            classType = null;
            callMethod = null;
            int idx = callClassAndMethod.LastIndexOf('.');
            if (idx == -1) {
                Debug.LogError("Class and method string '" + callClassAndMethod + "' is not in format: Class.Method");
                return false;
            }
            
            string className = callClassAndMethod.Substring(0, idx);
            callMethod = callClassAndMethod.Substring(idx + 1);
            classType = Type.GetType(className, false);

            if (classType == null) {
                Debug.LogError("Couldnt find class '" + className + "' in current assembly!");
                return false;
            }

            return true;
        }
            
        public static bool CallStaticMethod (string callClassAndMethod, object[] parameters, out float value) {
            value = 0;

            string callMethod;
            Type classType;
            if (!SeperateClassNameAndMethod ( callClassAndMethod, out classType, out callMethod ))
                return false;
            
            return TryAndCallMethod ( classType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, callMethod, null, parameters, out value, true, "Class: " + classType.FullName);
        }

        public static bool CallStaticMethod (string callClassAndMethod, object[] parameters) {
            string callMethod;
            Type classType;
            if (!SeperateClassNameAndMethod ( callClassAndMethod, out classType, out callMethod ))
                return false;
            
            return TryAndCallMethod ( classType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, callMethod, null, parameters, true, "Class: " + classType.FullName);
        }


        static void LogError (string errorPrefix, string callMethod, object[] parameters) {
            
            string typeNames = "";
            for (int i = 0; i < parameters.Length; i++) typeNames += parameters[i].GetType().Name + ", ";
            Debug.LogError(errorPrefix + " does not contain a call method: '" + callMethod + "' with parameter types: (" + typeNames + ")");
            
        }

        static bool CheckParametersForMethod (MethodInfo method, object[] suppliedParameters) {
            ParameterInfo[] methodParams = method.GetParameters();

            if (methodParams.Length != suppliedParameters.Length)
                return false;

            for (int p = 0; p < methodParams.Length; p++) {
                Type methodType = methodParams[p].ParameterType;
                Type suppliedType = suppliedParameters[p].GetType();
                if (suppliedType != methodType && !suppliedType.IsSubclassOf(methodType)) 
                    return false;
            }
            return true;
        }

        public static bool TryAndCallMethod (Type type, BindingFlags flags, string callMethod, object instance, object[] parameters, bool debugError, string errorPrefix) {
            MethodInfo[] allMethods = type.GetMethods(flags);
            for (int m = 0; m < allMethods.Length; m++) {
                MethodInfo method = allMethods[m];
                if (method.Name == callMethod) {
                    if (CheckParametersForMethod ( method, parameters )) {
                        method.Invoke(instance, parameters );
                        return true;
                    }
                }
            }
            if (debugError) LogError ( errorPrefix, callMethod, parameters);
            return false;
        }



        
    


        public static bool TryAndCallMethod (Type type, BindingFlags flags, string callMethod, object instance, object[] parameters, out float value, bool debugError, string errorPrefix) {
            MethodInfo[] allMethods = type.GetMethods(flags);
            for (int m = 0; m < allMethods.Length; m++) {
                MethodInfo method = allMethods[m];
                if (method.Name == callMethod) {
                    if (CheckParametersForMethod ( method, parameters )) {
                        if (method.ReturnType == typeof(float) || method.ReturnType == typeof(int)) {
                            value = (float)method.Invoke(instance, parameters );
                            return true;
                        }
                        else if (method.ReturnType == typeof(bool)) {
                            value = ((bool)method.Invoke(instance, parameters )) ? 1f : 0f;
                            return true;
                        }
                    }
                }
            }
            if (debugError) LogError ( errorPrefix, callMethod, parameters);
            value = 0;
            return false;
        }
    }
    

}

