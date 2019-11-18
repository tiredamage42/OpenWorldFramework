using UnityEngine;

using System.Collections.Generic;
namespace UnityTools {

    public class TerrainTools
    {
        public static int CalculateChunksInTerrain (Terrain terrain, float chunkSize) {
            float chunkResF = (terrain.terrainData.size.x / chunkSize);
            int chunkResI = (int)chunkResF;
            return (chunkResF - chunkResI > 0 ? chunkResI + 1 : chunkResI);
        }
            
        public static Dictionary<Vector2Int, Terrain> SplitTerrainIntoChunks(
            Terrain split, float chunkSize, int heightsRes, int alphasRes, int detailsRes
        )
        {
            if (split == null) {
                Debug.LogWarning("splitTerrain == null");
                return null;
            }

            TerrainData splitData = split.terrainData;

            float parentSize = splitData.size.x;

            if (parentSize <= chunkSize) {
                Debug.Log("splitTerrain fits in one grid already...");
                return null;
            }
            
            int splits = CalculateChunksInTerrain(split, chunkSize);
            

            Dictionary<Vector2Int, Terrain> grid2Terrain = new Dictionary<Vector2Int, Terrain>();

            float[,] heights = splitData.GetHeights(0, 0, splitData.heightmapResolution, splitData.heightmapResolution);
            float[,] chunkHeights = new float[heightsRes, heightsRes];
            
            TerrainLayer[] terrainLayers = splitData.terrainLayers;
            float[,,] alphas = splitData.GetAlphamaps(0, 0, splitData.alphamapResolution, splitData.alphamapResolution);
            float[,,] chunkAlphas = new float[alphasRes, alphasRes, splitData.alphamapLayers];
            
            detailsRes = Mathf.Max(0, detailsRes);
            DetailPrototype[] detailPrototypes = splitData.detailPrototypes;
            int[][,] details = new int[detailPrototypes.Length][,];
            int[,] chunkDetails = new int[detailsRes, detailsRes];
            if (detailsRes > 0) {
                for (int i = 0; i < detailPrototypes.Length; i++) 
                    details[i] = splitData.GetDetailLayer(0, 0, splitData.detailResolution, splitData.detailResolution, i);
            }
            
            TreePrototype[] treePrototypes = splitData.treePrototypes;
            TreeInstance[] treeInstances = splitData.treeInstances;

            Vector3 chunkSize3 = new Vector3(chunkSize, splitData.size.y, chunkSize);

            for (int y = 0; y < splits; y++) {
                for (int x = 0; x < splits; x++) {
                    Vector2Int grid = new Vector2Int(x, y);
                    
                    TerrainData data = new TerrainData();
                    GameObject terrainG = Terrain.CreateTerrainGameObject(data);
                    terrainG.name = split.name + "_" + grid;
                    Terrain terrain = terrainG.GetComponent<Terrain>();
                    terrain.terrainData = data;

                    grid2Terrain[grid] = terrain;

                    // Copy parent terrain propeties
                    #region parent properties
                    terrain.basemapDistance = split.basemapDistance;
                    terrain.castShadows = split.castShadows;
                    terrain.detailObjectDensity = split.detailObjectDensity;
                    terrain.detailObjectDistance = split.detailObjectDistance;
                    terrain.heightmapPixelError = split.heightmapPixelError;
                    terrain.treeBillboardDistance = split.treeBillboardDistance;
                    terrain.treeCrossFadeLength = split.treeCrossFadeLength;
                    terrain.treeDistance = split.treeDistance;
                    terrain.treeMaximumFullLODCount = split.treeMaximumFullLODCount;
                    #endregion
                    
                    Vector3 gridWorld = new Vector3(grid.x * chunkSize, 0, grid.y * chunkSize);
                    terrain.transform.position = split.transform.position + gridWorld;

                    SplitHeights (data, splitData, gridWorld, chunkSize, parentSize, chunkHeights, heights, heightsRes);
                    SplitAlphas (data, splitData, gridWorld, chunkSize, parentSize, chunkAlphas, alphas, alphasRes, terrainLayers);
                    
                    if (detailsRes > 0) {
                        SplitDetails (data, splitData, gridWorld, chunkSize, parentSize, chunkDetails, details, detailsRes, detailPrototypes);
                    }
                    
                    SplitTrees (terrain, splitData, gridWorld, chunkSize, treeInstances, treePrototypes);
        
                    data.size = chunkSize3;
                }
            }
            return grid2Terrain;
        }

        static float _TransferIndex (int i, float maxI, float worldGrid, float origSize, int origRes, int maxOrig, float chunkSize) {
            float t = (float)i / maxI;
            float world = worldGrid + chunkSize * t;
            return ((world / origSize) * maxOrig);
        }

        static int TransferIndex (int i, float maxI, float worldGrid, float origSize, int origRes, int maxOrig, float chunkSize) {
            return (int)_TransferIndex(i, maxI, worldGrid, origSize, origRes, maxOrig, chunkSize);
        }

        static void GetTransferIndexBounds (int i, float maxI, float worldGrid, float origSize, int origRes, int maxOrig, float chunkSize, out int lower, out int upper, out float interpolator) {
            float fIndex = _TransferIndex(i, maxI, worldGrid, origSize, origRes, maxOrig, chunkSize);
            lower = (int)fIndex;
            interpolator = fIndex - lower;
            upper = lower + 1;
            if (upper > maxOrig) upper = maxOrig;
        }

        static void SplitHeights (TerrainData data, TerrainData splitData, Vector3 worldGrid, float chunkSize, float parentSize, float[,] chunkHeights, float[,] heights, int heightsRes) {
            data.heightmapResolution = heightsRes;
            float maxRes = (float)(heightsRes - 1);

            int origRes = splitData.heightmapResolution;
            int origMax = origRes - 1;

            for (int yh = 0; yh < heightsRes; yh++) {

                float yT;
                int yLower, yUpper;
                        
                GetTransferIndexBounds (yh, maxRes, worldGrid.z, parentSize, origRes, origMax, chunkSize, out yLower, out yUpper, out yT);
                float yTi = 1-yT;
                            
                for (int xh = 0; xh < heightsRes; xh++) {
                    if (yLower >= origRes) {
                        chunkHeights[yh, xh] = 0;
                        continue;
                    }
                    float xT;
                    int xLower, xUpper;
                    GetTransferIndexBounds (xh, maxRes, worldGrid.x, parentSize, origRes, origMax, chunkSize, out xLower, out xUpper, out xT);
                    
                    if (xLower >= origRes) {
                        chunkHeights[yh, xh] = 0;
                        continue;
                    }
                    float xTi = 1-xT;
                    float y0 = xTi * heights[yLower, xLower] + xT * heights[yLower, xUpper];
                    float y1 = xTi * heights[yUpper, xLower] + xT * heights[yUpper, xUpper];
                    
                    chunkHeights[yh, xh] = yTi * y0 + yT * y1;
                    
                }
            }
            data.SetHeights(0, 0, chunkHeights);
        }
        static void SplitAlphas (TerrainData data, TerrainData splitData, Vector3 worldGrid, float chunkSize, float parentSize, float[,,] chunkAlphas, float[,,] alphas, int alphasRes, TerrainLayer[] terrainLayers) {
            data.terrainLayers = terrainLayers;   
            
            data.alphamapResolution = alphasRes;
            float maxRes = (float)(alphasRes - 1);

            int origRes = splitData.alphamapResolution;
            int origMax = origRes - 1;


            for (int a = 0; a < splitData.alphamapLayers; a++) {
                for (int yh = 0; yh < alphasRes; yh++) {
                    int yIndex = TransferIndex (yh, maxRes, worldGrid.z, parentSize, origRes, origMax, chunkSize);
            
                    for (int xh = 0; xh < alphasRes; xh++) {
                        if (yIndex >= origRes) {
                            chunkAlphas[yh, xh, a] = 0;
                            continue;
                        }
                        int xIndex = TransferIndex (xh, maxRes, worldGrid.x, parentSize, origRes, origMax, chunkSize);
                        if (xIndex >= origRes) {
                            chunkAlphas[yh, xh, a] = 0;
                            continue;
                        }
                        
                        chunkAlphas[yh, xh, a] = alphas[yIndex, xIndex, a];
                        
                    }
                }
            }
            data.SetAlphamaps(0, 0, chunkAlphas);
        }
        static void SplitDetails (TerrainData data, TerrainData splitData, Vector3 worldGrid, float chunkSize, float parentSize, int[,] chunkDetails, int[][,] details, int detailsRes, DetailPrototype[] detailPrototypes) {
            data.detailPrototypes = detailPrototypes;
                    
            float maxRes = (float)(detailsRes - 1);
            data.SetDetailResolution(detailsRes, splitData.detailResolutionPerPatch); 

            int origRes = splitData.detailResolution;
            int origMax = origRes - 1;

            for (int a = 0; a < detailPrototypes.Length; a++) {
                for (int yh = 0; yh < detailsRes; yh++) {
                    int yIndex = TransferIndex (yh, maxRes, worldGrid.z, parentSize, origRes, origMax, chunkSize);
                    for (int xh = 0; xh < detailsRes; xh++) {
                        if (yIndex >= origRes) {
                            chunkDetails[yh, xh] = 0;
                            continue;
                        }
                        int xIndex = TransferIndex (xh, maxRes, worldGrid.x, parentSize, origRes, origMax, chunkSize);
                        if (xIndex >= origRes) {
                            chunkDetails[yh, xh] = 0;
                            continue;
                        }
                        
                        chunkDetails[yh, xh] = details[a][yIndex, xIndex];
                    }
                }
                data.SetDetailLayer(0, 0, a, chunkDetails);
            }
        }
        static void SplitTrees (Terrain terrain, TerrainData splitData, Vector3 worldGrid, float chunkSize, TreeInstance[] treeInstances, TreePrototype[] treePrototypes) {
            terrain.terrainData.treePrototypes = treePrototypes;
                    
            Vector3 nextWorldGrid = new Vector3(worldGrid.x + chunkSize, 0, worldGrid.y + chunkSize);
            for (int i = 0; i < treeInstances.Length; i++) {
                TreeInstance ti = treeInstances[i];
                Vector3 wPos = new Vector3(ti.position.x * splitData.size.x, ti.position.y, ti.position.z * splitData.size.z);
                if (wPos.x >= worldGrid.x && wPos.z >= worldGrid.z && wPos.x < nextWorldGrid.x && wPos.z < nextWorldGrid.y) {
                    ti.position = new Vector3(
                        (wPos.x - worldGrid.x)/chunkSize, 
                        wPos.y, 
                        (wPos.z - worldGrid.z)/chunkSize
                    );
                    terrain.AddTreeInstance(ti);
                }
            }
        }








        static float GetAverage (float[,] originalHeightMap, int x, int y, int resolution, int reach) {
            float c = 0;
            float total = 0;
            for (int yn = -reach; yn <= reach; yn++) {
                int nY = y + yn;
                if (nY >= 0 && nY < resolution) {
                    for (int xn = -reach; xn <= reach; xn++) {
                        int nX = x + xn;
                        if (nX >= 0 && nX < resolution) {
                            total += originalHeightMap[nX, nY];
                            c += 1;
                        }
                    }
                }
            }
            return total / c;
        }

        public static void SmoothHeightMapV1 (float[,] originalHeightMap, float[,] temp, int resolution, int passes, int reach) {
            int m = resolution - 1;
            for (int i = 0; i < passes; i++) {
                for (int y = 1; y < m; y++) {
                    for (int x = 1; x < m; x++) {
                        temp[x, y] = GetAverage ( originalHeightMap, x, y, resolution, reach );
                    }    
                }
                for (int y = 1; y < m; y++) {
                    for (int x = 1; x < m; x++) {
                        originalHeightMap[x, y] = temp[x, y];
                    }    
                }       
            }
        }
        public static void SmoothHeightMap (float[,] heightMap, int resolution, int passes, int reach) {
            int m = resolution - 1;
            for (int i = 0; i < passes; i++) {
                for (int y = 1; y < m; y++) {
                    for (int x = 1; x < m; x++) {
                        heightMap[x, y] = GetAverage ( heightMap, x, y, resolution, reach );
                    }    
                }       
            }
        }   
    }
}
