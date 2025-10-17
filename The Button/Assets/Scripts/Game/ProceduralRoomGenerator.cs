using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TheButton.Items;
using TheButton.Interactables;

namespace TheButton.Game
{
    /// <summary>
    /// Server-authoritative procedural room generator
    /// Creates a room made of cubes with buttons on walls
    /// Uses network seed for deterministic generation across all clients
    /// </summary>
    public class ProceduralRoomGenerator : NetworkBehaviour
    {
        public static ProceduralRoomGenerator Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private RoomConfiguration roomConfig;
        [SerializeField] private RoomItemPool itemPool;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Network synced seed for deterministic generation
        private NetworkVariable<int> roomSeed = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        // Track generation state
        private NetworkVariable<bool> isRoomGenerated = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        
        // Store generated objects for cleanup
        private List<GameObject> generatedObjects = new List<GameObject>();
        private List<WallPosition> availableWallPositions = new List<WallPosition>();
        private List<WallPosition> usedWallPositions = new List<WallPosition>();
        
        // Helper struct to store position and rotation
        private struct WallPosition
        {
            public Vector3 position;
            public Quaternion rotation;
            
            public WallPosition(Vector3 pos, Quaternion rot)
            {
                position = pos;
                rotation = rot;
            }
        }
        
        // Events
        public event System.Action OnRoomGenerationComplete;
        
        // Room center for player spawning
        private Vector3 roomCenter;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            isRoomGenerated.OnValueChanged += OnRoomGeneratedChanged;
            
            // If client and room already generated, we missed it
            if (!IsServer && isRoomGenerated.Value)
            {
                Log("[Client] Room already generated");
                OnRoomGenerationComplete?.Invoke();
            }
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            isRoomGenerated.OnValueChanged -= OnRoomGeneratedChanged;
        }
        
        /// <summary>
        /// Generate the room (Server only)
        /// </summary>
        public void GenerateRoom()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[RoomGenerator] GenerateRoom can only be called on server!");
                return;
            }
            
            if (isRoomGenerated.Value)
            {
                Debug.LogWarning("[RoomGenerator] Room already generated!");
                return;
            }
            
            if (roomConfig == null)
            {
                Debug.LogError("[RoomGenerator] RoomConfiguration is not assigned!");
                return;
            }
            
            if (itemPool == null)
            {
                Debug.LogError("[RoomGenerator] RoomItemPool is not assigned!");
                return;
            }
            
            if (!itemPool.Validate())
            {
                Debug.LogError("[RoomGenerator] RoomItemPool validation failed!");
                return;
            }
            
            // Generate random seed
            int seed = Random.Range(int.MinValue, int.MaxValue);
            roomSeed.Value = seed;
            
            Log($"Starting room generation with seed: {seed}");
            
            StartCoroutine(GenerateRoomCoroutine(seed));
        }
        
        /// <summary>
        /// Generate room in batches for performance
        /// </summary>
        private IEnumerator GenerateRoomCoroutine(int seed)
        {
            // Set random seed for deterministic generation
            Random.InitState(seed);
            
            // Clear previous generation
            ClearRoom();
            yield return null;
            
            // Calculate room center
            roomCenter = new Vector3(
                roomConfig.roomWidth * roomConfig.cubeSize / 2f,
                roomConfig.roomHeight * roomConfig.cubeSize / 2f,
                roomConfig.roomDepth * roomConfig.cubeSize / 2f
            );
            
            Log("Generating floor and ceiling...");
            GenerateFloorAndCeiling();
            yield return null;
            
            Log("Generating walls with buttons...");
            GenerateWallsWithButtons();
            yield return null;
            
            Log("Creating ceiling spawn point...");
            if (roomConfig.createCeilingSpawnPoint)
            {
                CreateCeilingSpawnPoint();
            }
            yield return null;
            
            // Mark generation complete
            isRoomGenerated.Value = true;
            Log("Room generation complete!");
            
            OnRoomGenerationComplete?.Invoke();
        }
        
        /// <summary>
        /// Generate floor and ceiling (no buttons)
        /// </summary>
        private void GenerateFloorAndCeiling()
        {
            GameObject floorParent = new GameObject("Floor");
            GameObject ceilingParent = new GameObject("Ceiling");
            generatedObjects.Add(floorParent);
            generatedObjects.Add(ceilingParent);
            
            for (int x = 0; x < roomConfig.roomWidth; x++)
            {
                for (int z = 0; z < roomConfig.roomDepth; z++)
                {
                    Vector3 position = new Vector3(x * roomConfig.cubeSize, 0, z * roomConfig.cubeSize);
                    
                    // Floor
                    GameObject floorTile = CreateCube(position, "FloorTile", floorParent.transform);
                    if (roomConfig.floorMaterial != null && floorTile != null)
                    {
                        var renderer = floorTile.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            renderer.material = roomConfig.floorMaterial;
                        }
                    }
                    
                    // Ceiling
                    Vector3 ceilingPos = position + Vector3.up * (roomConfig.roomHeight * roomConfig.cubeSize);
                    GameObject ceilingTile = CreateCube(ceilingPos, "CeilingTile", ceilingParent.transform);
                    if (roomConfig.ceilingMaterial != null && ceilingTile != null)
                    {
                        var renderer = ceilingTile.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            renderer.material = roomConfig.ceilingMaterial;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Generate all 4 walls with buttons (fills entire wall)
        /// </summary>
        private void GenerateWallsWithButtons()
        {
            GameObject wallsParent = new GameObject("Walls");
            generatedObjects.Add(wallsParent);
            
            // Collect all required and random items
            List<ItemData> itemsToPlace = new List<ItemData>();
            
            // Add required items first
            if (itemPool.requiredItems != null)
            {
                itemsToPlace.AddRange(itemPool.requiredItems);
            }
            
            // Calculate total wall positions
            int totalWallPositions = (roomConfig.roomWidth * (roomConfig.roomHeight - 1) * 2) + 
                                    (roomConfig.roomDepth * (roomConfig.roomHeight - 1) * 2);
            
            // Fill remaining with random items
            int remainingSlots = totalWallPositions - itemsToPlace.Count;
            for (int i = 0; i < remainingSlots; i++)
            {
                ItemData randomItem = itemPool.GetRandomItem();
                if (randomItem != null)
                {
                    itemsToPlace.Add(randomItem);
                }
            }
            
            // Shuffle items for random placement
            ShuffleList(itemsToPlace);
            
            int itemIndex = 0;
            
            // North wall (positive Z)
            GenerateWall_Internal(
                new Vector3(0, roomConfig.cubeSize, roomConfig.roomDepth * roomConfig.cubeSize),
                Vector3.right,
                Vector3.up,
                Quaternion.Euler(0, 180, 0),
                roomConfig.roomWidth,
                roomConfig.roomHeight - 1,
                wallsParent.transform,
                itemsToPlace,
                ref itemIndex
            );
            
            // South wall (negative Z)
            GenerateWall_Internal(
                new Vector3(0, roomConfig.cubeSize, 0),
                Vector3.right,
                Vector3.up,
                Quaternion.identity,
                roomConfig.roomWidth,
                roomConfig.roomHeight - 1,
                wallsParent.transform,
                itemsToPlace,
                ref itemIndex
            );
            
            // East wall (positive X)
            GenerateWall_Internal(
                new Vector3(roomConfig.roomWidth * roomConfig.cubeSize, roomConfig.cubeSize, 0),
                Vector3.forward,
                Vector3.up,
                Quaternion.Euler(0, -90, 0),
                roomConfig.roomDepth,
                roomConfig.roomHeight - 1,
                wallsParent.transform,
                itemsToPlace,
                ref itemIndex
            );
            
            // West wall (negative X)
            GenerateWall_Internal(
                new Vector3(0, roomConfig.cubeSize, 0),
                Vector3.forward,
                Vector3.up,
                Quaternion.Euler(0, 90, 0),
                roomConfig.roomDepth,
                roomConfig.roomHeight - 1,
                wallsParent.transform,
                itemsToPlace,
                ref itemIndex
            );
        }
        
        /// <summary>
        /// Generate a single wall with buttons
        /// </summary>
        private void GenerateWall_Internal(Vector3 startPos, Vector3 widthDir, Vector3 heightDir,
            Quaternion rotation, int width, int height, Transform parent, List<ItemData> items, ref int itemIndex)
        {
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    Vector3 position = startPos + 
                        widthDir * w * roomConfig.cubeSize + 
                        heightDir * h * roomConfig.cubeSize;
                    
                    // Check if this is a corner position
                    bool isCorner = IsCornerPosition(w, h, width, height);
                    
                    if (isCorner && roomConfig.cornerCubePrefab != null)
                    {
                        // Place plain corner cube (no button)
                        GameObject cornerCube = Instantiate(roomConfig.cornerCubePrefab, position, rotation, parent);
                        cornerCube.name = $"CornerCube_{w}_{h}";
                        
                        NetworkObject netObj = cornerCube.GetComponent<NetworkObject>();
                        if (netObj != null)
                        {
                            netObj.Spawn(true);
                        }
                        
                        generatedObjects.Add(cornerCube);
                    }
                    else
                    {
                        // Place wall cube with button
                        if (itemIndex < items.Count)
                        {
                            WallPosition wallPos = new WallPosition(position, rotation);
                            SpawnWallCubeWithButton(wallPos, items[itemIndex], itemIndex < (itemPool.requiredItems?.Count ?? 0));
                            itemIndex++;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if position is at a corner (should be skipped to avoid overlap)
        /// Corners are where walls would intersect
        /// </summary>
        private bool IsCornerPosition(int w, int h, int width, int height)
        {
            // Skip first and last column (these are corners that intersect with perpendicular walls)
            bool isWidthCorner = (w == 0 || w == width - 1);
            
            // For corner columns, skip all positions
            // For other columns, check if at top or bottom edge (vertical corners)
            if (isWidthCorner)
            {
                return true; // Skip entire corner columns
            }
            
            return false;
        }
        
        /// <summary>
        /// Shuffle a list using Fisher-Yates algorithm
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
        
        /// <summary>
        /// Create ceiling spawn point cube - this will be THE spawn point for all items
        /// </summary>
        private void CreateCeilingSpawnPoint()
        {
            if (roomConfig.spawnPointCubePrefab == null)
            {
                Log("No spawn point cube prefab assigned, skipping...");
                return;
            }
            
            // Calculate ceiling center position
            Vector3 ceilingCenter = new Vector3(
                roomConfig.roomWidth * roomConfig.cubeSize / 2f,
                (roomConfig.roomHeight - 1) * roomConfig.cubeSize,
                roomConfig.roomDepth * roomConfig.cubeSize / 2f
            );
            
            GameObject spawnCube = Instantiate(roomConfig.spawnPointCubePrefab, ceilingCenter, Quaternion.identity);
            spawnCube.name = "GlobalItemSpawnPoint";
            spawnCube.tag = "ItemSpawnPoint"; // Tag for easy finding
            
            NetworkObject netObj = spawnCube.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
            }
            
            generatedObjects.Add(spawnCube);
            
            Log($"Created global item spawn point at {ceilingCenter}");
        }
        
        /// <summary>
        /// Create a simple cube at position
        /// </summary>
        private GameObject CreateCube(Vector3 position, string name, Transform parent)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = Vector3.one * roomConfig.cubeSize;
            cube.transform.parent = parent;
            
            // Make static for performance
            cube.isStatic = true;
            
            return cube;
        }
        
        /// <summary>
        /// Spawn a wall cube with button prefab at the given position
        /// </summary>
        private void SpawnWallCubeWithButton(WallPosition wallPos, ItemData itemData, bool isRequired)
        {
            if (roomConfig.wallCubeWithButtonPrefab == null)
            {
                Debug.LogError("[RoomGenerator] WallCubeWithButton prefab is not assigned!");
                return;
            }
            
            if (itemData == null)
            {
                Debug.LogError("[RoomGenerator] ItemData is null! Cannot spawn wall cube.");
                return;
            }
            
            // Instantiate the wall cube with button
            GameObject wallCubeObj = Instantiate(
                roomConfig.wallCubeWithButtonPrefab, 
                wallPos.position, 
                wallPos.rotation
            );
            wallCubeObj.name = $"WallCube_{itemData.itemName}_{(isRequired ? "Required" : "Random")}";
            
            // Find the SpawnButton component (should be in the prefab hierarchy)
            SpawnButton spawnButton = wallCubeObj.GetComponentInChildren<SpawnButton>();
            if (spawnButton != null)
            {
                Log($"Setting ItemData '{itemData.itemName}' to button at {wallPos.position}");
                spawnButton.SetItemData(itemData);
            }
            else
            {
                Debug.LogWarning($"[RoomGenerator] WallCube at {wallPos.position} has no SpawnButton component!");
            }
            
            // Get NetworkObject and spawn it
            NetworkObject networkObject = wallCubeObj.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(true);
                generatedObjects.Add(wallCubeObj);
            }
            else
            {
                Debug.LogError($"[RoomGenerator] WallCubeWithButton prefab has no NetworkObject component!");
                Destroy(wallCubeObj);
            }
        }
        
        /// <summary>
        /// Clear all generated room objects
        /// </summary>
        public void ClearRoom()
        {
            if (!IsServer)
            {
                Debug.LogWarning("[RoomGenerator] ClearRoom can only be called on server!");
                return;
            }
            
            Log("Clearing room...");
            
            foreach (var obj in generatedObjects)
            {
                if (obj != null)
                {
                    // Despawn networked objects
                    NetworkObject netObj = obj.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned)
                    {
                        netObj.Despawn(true);
                    }
                    else
                    {
                        Destroy(obj);
                    }
                }
            }
            
            generatedObjects.Clear();
            availableWallPositions.Clear();
            usedWallPositions.Clear();
            isRoomGenerated.Value = false;
            
            Log("Room cleared");
        }
        
        /// <summary>
        /// Get the center position of the room for player spawning
        /// Calculates from room dimensions (not using cached roomCenter)
        /// </summary>
        public Vector3 GetRoomCenter()
        {
            if (roomConfig == null)
            {
                Debug.LogError("[RoomGenerator] RoomConfiguration is null!");
                return Vector3.zero;
            }
            
            // Calculate center from room dimensions
            Vector3 center = new Vector3(
                roomConfig.roomWidth * roomConfig.cubeSize / 2f,
                roomConfig.roomHeight * roomConfig.cubeSize / 2f,
                roomConfig.roomDepth * roomConfig.cubeSize / 2f
            );
            
            Vector3 finalPos = center + roomConfig.playerSpawnOffset;
            Log($"GetRoomCenter calculated: {finalPos}");
            
            return finalPos;
        }
        
        /// <summary>
        /// Check if room generation is complete
        /// </summary>
        public bool IsRoomReady()
        {
            return isRoomGenerated.Value;
        }
        
        private void OnRoomGeneratedChanged(bool oldValue, bool newValue)
        {
            if (newValue && !IsServer)
            {
                Log("[Client] Room generation completed (network sync)");
                OnRoomGenerationComplete?.Invoke();
            }
        }
        
        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[RoomGenerator] {message}");
            }
        }
    }
}

