
using UnityEngine;
using UnityEngine.Rendering;
using UnityTools;
namespace OpenWorldFramework.Terrain {

    /*
     
        Hub for all terrain chunks in the world cells to keep terrain settings consistent

        this componenet should be kept in the world settings scene...
        although it probably doesnt matter, sicne the singleton marks it as 
        dont destory on load, and removes any duplicates...
    */
    public class TerrainDefenition : Singleton<TerrainDefenition>
    {
        [Header("Terrain")]
        public float baseMapDistance = 250;
        public bool castShadows = true;
        public bool drawInstanced;
        public int heightmapMaximumLOD;
        [Range(0, 200)] public float heightmapPixelError = 5;
        public ReflectionProbeUsage reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;

        public bool drawTreesAndFoliage = true;
        [Header("Details")]
        [Range(0,1)] public float detailObjectDensity = 1;
        [Range(0, 1000)] public float detailObjectDistance = 80;
        
        [Header("Trees")]
        public float treeDistance = 5000;
        public float treeBillboardDistance = 50;
        public float treeCrossFadeLength = 5;
        [Range(0, 10000)] public int treeMaximumFullLODCount = 50;
        [Range(1, 10)] public float treeLODBiasMultiplier = 1;
    }
}