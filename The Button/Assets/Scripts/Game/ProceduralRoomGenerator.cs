using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// Generate floor and ceiling as single planes (much more efficient!)
        /// </summary>
        private void GenerateFloorAndCeiling()
        {
            // Calculate room size
            float roomWidthSize = roomConfig.roomWidth * roomConfig.cubeSize;
            float roomDepthSize = roomConfig.roomDepth * roomConfig.cubeSize;
            float halfCubeSize = roomConfig.cubeSize / 2f;
            
            // Floor - position at half cube UP so it aligns with bottom of wall cubes
            if (roomConfig.floorPrefab != null)
            {
                // Use prefab - single plane
                Vector3 floorPos = new Vector3(roomWidthSize / 2f, halfCubeSize, roomDepthSize / 2f);
                GameObject floor = Instantiate(roomConfig.floorPrefab, floorPos, Quaternion.identity);
                floor.name = "Floor";
                
                // Scale to match room size (default plane is 10x10)
                floor.transform.localScale = new Vector3(roomWidthSize / 10f, 1f, roomDepthSize / 10f);
                
                // Spawn as NetworkObject
                NetworkObject floorNetObj = floor.GetComponent<NetworkObject>();
                if (floorNetObj != null)
                {
                    floorNetObj.Spawn(true);
                    Log("Floor spawned as NetworkObject");
                }
                
                generatedObjects.Add(floor);
            }
            else
            {
                // Create simple plane
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Floor";
                floor.transform.position = new Vector3(roomWidthSize / 2f, halfCubeSize, roomDepthSize / 2f);
                floor.transform.localScale = new Vector3(roomWidthSize / 10f, 1f, roomDepthSize / 10f);
                
                if (roomConfig.floorMaterial != null)
                {
                    floor.GetComponent<MeshRenderer>().material = roomConfig.floorMaterial;
                }
                
                generatedObjects.Add(floor);
            }
            
            // Ceiling - position at half cube above the top wall cubes
            if (roomConfig.ceilingPrefab != null)
            {
                // Use prefab - single plane
                // Top wall cube is at: (roomHeight - 1) * cubeSize + cubeSize (center)
                // So ceiling should be at: roomHeight * cubeSize - halfCubeSize
                float ceilingHeight = (roomConfig.roomHeight * roomConfig.cubeSize) - halfCubeSize;
                Vector3 ceilingPos = new Vector3(roomWidthSize / 2f, ceilingHeight, roomDepthSize / 2f);
                GameObject ceiling = Instantiate(roomConfig.ceilingPrefab, ceilingPos, Quaternion.Euler(180, 0, 0)); // Flip upside down
                ceiling.name = "Ceiling";
                
                // Scale to match room size
                ceiling.transform.localScale = new Vector3(roomWidthSize / 10f, 1f, roomDepthSize / 10f);
                
                // Spawn as NetworkObject
                NetworkObject ceilingNetObj = ceiling.GetComponent<NetworkObject>();
                if (ceilingNetObj != null)
                {
                    ceilingNetObj.Spawn(true);
                    Log("Ceiling spawned as NetworkObject");
                }
                
                generatedObjects.Add(ceiling);
            }
            else
            {
                // Create simple plane
                float ceilingHeight = (roomConfig.roomHeight * roomConfig.cubeSize) - halfCubeSize;
                GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ceiling.name = "Ceiling";
                ceiling.transform.position = new Vector3(roomWidthSize / 2f, ceilingHeight, roomDepthSize / 2f);
                ceiling.transform.rotation = Quaternion.Euler(180, 0, 0); // Flip upside down
                ceiling.transform.localScale = new Vector3(roomWidthSize / 10f, 1f, roomDepthSize / 10f);
                
                if (roomConfig.ceilingMaterial != null)
                {
                    ceiling.GetComponent<MeshRenderer>().material = roomConfig.ceilingMaterial;
                }
                
                generatedObjects.Add(ceiling);
            }
            
            Log($"Generated floor and ceiling as single planes (Size: {roomWidthSize}x{roomDepthSize}, Floor Y: {halfCubeSize}, Ceiling Y: {(roomConfig.roomHeight * roomConfig.cubeSize) - halfCubeSize})");
        }
        
        /// <summary>
        /// Generate all 4 walls with buttons (based on density settings)
        /// </summary>
        private void GenerateWallsWithButtons()
        {
            GameObject wallsParent = new GameObject("Walls");
            generatedObjects.Add(wallsParent);
            
            // Calculate total wall positions (not including corner cubes)
            // North/South walls: (width - 2) * height each (corners excluded)
            // East/West walls: (depth - 2) * height each (corners are cornerCube, not button)
            int northSouthWalls = (roomConfig.roomWidth - 2) * (roomConfig.roomHeight - 1) * 2;
            int eastWestWalls = (roomConfig.roomDepth - 2) * (roomConfig.roomHeight - 1) * 2;
            int totalWallPositions = northSouthWalls + eastWestWalls;
            
            // Calculate button density for this room
            float densityPercent = Random.Range(roomConfig.minButtonDensityPercent, roomConfig.maxButtonDensityPercent);
            int targetButtonCount = Mathf.RoundToInt(totalWallPositions * (densityPercent / 100f));
            
            Log($"Button Density: {densityPercent:F1}% ({targetButtonCount} buttons out of {totalWallPositions} wall positions)");
            
            // Collect all required and random items
            List<ItemData> itemsToPlace = new List<ItemData>();
            
            // Add required items first (these MUST spawn)
            if (itemPool.requiredItems != null)
            {
                itemsToPlace.AddRange(itemPool.requiredItems);
            }
            
            int requiredItemCount = itemsToPlace.Count;
            
            // Ensure we have enough buttons for required items
            if (targetButtonCount < requiredItemCount)
            {
                Log($"Warning: Button density too low! Required items: {requiredItemCount}, target buttons: {targetButtonCount}. Increasing to minimum.");
                targetButtonCount = requiredItemCount;
            }
            
            // Fill remaining button slots with random items
            int randomButtonSlots = targetButtonCount - requiredItemCount;
            for (int i = 0; i < randomButtonSlots; i++)
            {
                ItemData randomItem = itemPool.GetRandomItem();
                if (randomItem != null)
                {
                    itemsToPlace.Add(randomItem);
                }
            }
            
            // Shuffle items for random placement
            ShuffleList(itemsToPlace);
            
            // Create list of ALL wall positions
            List<int> allPositions = new List<int>();
            for (int i = 0; i < totalWallPositions; i++)
            {
                allPositions.Add(i);
            }
            
            // Shuffle ALL positions
            ShuffleList(allPositions);
            
            // Take first N positions as button positions (now truly random!)
            HashSet<int> buttonPositionIndices = new HashSet<int>();
            for (int i = 0; i < targetButtonCount && i < allPositions.Count; i++)
            {
                buttonPositionIndices.Add(allPositions[i]);
            }
            
            Log($"Random button positions: {string.Join(", ", buttonPositionIndices.Take(10))}...");
            
            int itemIndex = 0;
            int globalPositionIndex = 0;
            
            // North wall (positive Z) - skip corners
            GenerateWall_Internal(
                new Vector3(roomConfig.cubeSize, roomConfig.cubeSize, (roomConfig.roomDepth - 1) * roomConfig.cubeSize), // Start 1 cube in, Z at last row
                Vector3.right,
                Vector3.up,
                Quaternion.Euler(0, 180, 0),
                roomConfig.roomWidth - 2, // Width minus 2 corners
                roomConfig.roomHeight - 1,
                wallsParent.transform,
                itemsToPlace,
                buttonPositionIndices,
                ref itemIndex,
                ref globalPositionIndex,
                isEastOrWestWall: false // North wall, corners already excluded
            );
            
            // South wall (negative Z) - skip corners
            GenerateWall_Internal(
                new Vector3(roomConfig.cubeSize, roomConfig.cubeSize, 0), // Start 1 cube in
                Vector3.right,
                Vector3.up,
                Quaternion.identity,
                roomConfig.roomWidth - 2, // Width minus 2 corners
                roomConfig.roomHeight - 1,
                wallsParent.transform,
                itemsToPlace,
                buttonPositionIndices,
                ref itemIndex,
                ref globalPositionIndex,
                isEastOrWestWall: false // South wall, corners already excluded
            );
            
            // East wall (positive X) - include corners as cornerCube
            GenerateWall_Internal(
                new Vector3((roomConfig.roomWidth - 1) * roomConfig.cubeSize, roomConfig.cubeSize, 0),
                Vector3.forward,
                Vector3.up,
                Quaternion.Euler(0, -90, 0),
                roomConfig.roomDepth,
                roomConfig.roomHeight - 1,
                wallsParent.transform,
                itemsToPlace,
                buttonPositionIndices,
                ref itemIndex,
                ref globalPositionIndex,
                isEastOrWestWall: true // East wall, will place cornerCube at first and last positions
            );
            
            // West wall (negative X) - include corners as cornerCube
            GenerateWall_Internal(
                new Vector3(0, roomConfig.cubeSize, 0),
                Vector3.forward,
                Vector3.up,
                Quaternion.Euler(0, 90, 0),
                roomConfig.roomDepth,
                roomConfig.roomHeight - 1,
                wallsParent.transform,
                itemsToPlace,
                buttonPositionIndices,
                ref itemIndex,
                ref globalPositionIndex,
                isEastOrWestWall: true // West wall, will place cornerCube at first and last positions
            );
        }
        
        /// <summary>
        /// Generate a single wall with buttons (based on button density)
        /// </summary>
        private void GenerateWall_Internal(Vector3 startPos, Vector3 widthDir, Vector3 heightDir,
            Quaternion rotation, int width, int height, Transform parent, List<ItemData> items, 
            HashSet<int> buttonPositionIndices, ref int itemIndex, ref int globalPositionIndex, bool isEastOrWestWall = false)
        {
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    Vector3 position = startPos + 
                        widthDir * w * roomConfig.cubeSize + 
                        heightDir * h * roomConfig.cubeSize;
                    
                    // Check if this is a corner position (first or last column of East/West walls)
                    bool isCorner = isEastOrWestWall && (w == 0 || w == width - 1);
                    
                    if (isCorner && roomConfig.cornerCubePrefab != null)
                    {
                        // Place corner cube (no button)
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
                        // Check if this position should have a button
                        bool shouldPlaceButton = buttonPositionIndices.Contains(globalPositionIndex);
                        
                        if (shouldPlaceButton && itemIndex < items.Count)
                        {
                            // Place wall cube with button
                            WallPosition wallPos = new WallPosition(position, rotation);
                            SpawnWallCubeWithButton(wallPos, items[itemIndex], itemIndex < (itemPool.requiredItems?.Count ?? 0));
                            itemIndex++;
                        }
                        else if (roomConfig.plainWallCubePrefab != null)
                        {
                            // Place plain wall cube (no button)
                            GameObject plainCube = Instantiate(roomConfig.plainWallCubePrefab, position, rotation, parent);
                            plainCube.name = $"PlainWall_{globalPositionIndex}";
                            
                            NetworkObject netObj = plainCube.GetComponent<NetworkObject>();
                            if (netObj != null)
                            {
                                netObj.Spawn(true);
                            }
                            
                            generatedObjects.Add(plainCube);
                        }
                        
                        globalPositionIndex++;
                    }
                }
            }
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
        /// Positioned just below the ceiling plane
        /// </summary>
        private void CreateCeilingSpawnPoint()
        {
            if (roomConfig.spawnPointCubePrefab == null)
            {
                Log("No spawn point cube prefab assigned, skipping...");
                return;
            }
            
            // Calculate ceiling center position
            // Ceiling is at (roomHeight * cubeSize) - halfCubeSize
            // Spawn point should be below it by one cube
            float halfCubeSize = roomConfig.cubeSize / 2f;
            float ceilingY = (roomConfig.roomHeight * roomConfig.cubeSize) - halfCubeSize;
            float spawnPointY = ceilingY - roomConfig.cubeSize; // One cube below ceiling
            
            Vector3 spawnPointPos = new Vector3(
                roomConfig.roomWidth * roomConfig.cubeSize / 2f,
                spawnPointY,
                roomConfig.roomDepth * roomConfig.cubeSize / 2f
            );
            
            GameObject spawnCube = Instantiate(roomConfig.spawnPointCubePrefab, spawnPointPos, Quaternion.identity);
            spawnCube.name = "GlobalItemSpawnPoint";
            spawnCube.tag = "ItemSpawnPoint"; // Tag for easy finding
            
            NetworkObject netObj = spawnCube.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
            }
            
            generatedObjects.Add(spawnCube);
            
            Log($"Created global item spawn point at {spawnPointPos} (Ceiling Y: {ceilingY})");
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
        /// Players spawn on the floor in the center of the room
        /// </summary>
        public Vector3 GetRoomCenter()
        {
            if (roomConfig == null)
            {
                Debug.LogError("[RoomGenerator] RoomConfiguration is null!");
                return Vector3.zero;
            }
            
            // Calculate center from room dimensions
            // Y position should be just above the floor (floor is at halfCubeSize = 0.5)
            // Player should spawn 1 unit above floor to stand on it
            float halfCubeSize = roomConfig.cubeSize / 2f;
            Vector3 center = new Vector3(
                roomConfig.roomWidth * roomConfig.cubeSize / 2f,
                halfCubeSize + roomConfig.cubeSize, // Floor + 1 cube height = standing on floor
                roomConfig.roomDepth * roomConfig.cubeSize / 2f
            );
            
            Vector3 finalPos = center + roomConfig.playerSpawnOffset;
            Log($"GetRoomCenter calculated: {finalPos} (Floor at Y: {halfCubeSize})");
            
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

