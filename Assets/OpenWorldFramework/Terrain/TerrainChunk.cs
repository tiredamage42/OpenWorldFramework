
using UnityEngine;
using UnityTools;
namespace OpenWorldFramework.Terrain {

    /*
        added to split terrain chunk when split into world cells scene
    */
    [ExecuteInEditMode] public class TerrainChunk : MonoBehaviour {
        UnityEngine.Terrain _terrain;
        UnityEngine.Terrain terrain { get { return gameObject.GetComponentIfNull<UnityEngine.Terrain>(ref _terrain, false); } }

        void OnEnable () {
            UpdateTerrain();
        }

        #if UNITY_EDITOR
        void Update () {
            UpdateTerrain();
        }
        #endif

        void UpdateTerrain () {
            TerrainDefenition def;
            if (Application.isPlaying) {
                def = TerrainDefenition.instance;
            }
            else {
                def = GameObject.FindObjectOfType<TerrainDefenition>();
            }
            if (def == null) {
                Debug.LogWarning("Cant Find Terrain Defenition...");
                return;
            }

            UnityEngine.Terrain terrain = this.terrain;
            if (terrain == null)
                return;

            terrain.basemapDistance = def.baseMapDistance;
            terrain.castShadows = def.castShadows;
            terrain.drawInstanced = def.drawInstanced;
            terrain.reflectionProbeUsage = def.reflectionProbeUsage;
            terrain.heightmapMaximumLOD = def.heightmapMaximumLOD;
            terrain.heightmapPixelError = def.heightmapPixelError;

            terrain.drawTreesAndFoliage = def.drawTreesAndFoliage;
            terrain.detailObjectDensity = def.detailObjectDensity;
            terrain.detailObjectDistance = def.detailObjectDistance;

            terrain.treeDistance = def.treeDistance;
            terrain.treeBillboardDistance = def.treeBillboardDistance;
            terrain.treeCrossFadeLength = def.treeCrossFadeLength;
            terrain.treeLODBiasMultiplier = def.treeLODBiasMultiplier;
            terrain.treeMaximumFullLODCount = def.treeMaximumFullLODCount;
        }
    }
}