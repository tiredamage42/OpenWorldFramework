#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools {
    public class BuildSettings 
    {
        public static List<string> buildWindowIgnorePatterns = new List<string>();
        public static void AddBuildWindowIgnorePattern (string pattern) {
            if (!buildWindowIgnorePatterns.Contains(pattern)) {
                buildWindowIgnorePatterns.Add(pattern);
            }
        }
    }
}
#endif
