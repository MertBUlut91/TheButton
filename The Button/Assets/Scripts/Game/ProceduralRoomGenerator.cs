using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using TheButton.Items;
using TheButton.Interactables;
using TheButton.Enemy;

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
        
        // Track occupied grid positions for multi-block events
        private HashSet<Vector3Int> occupiedGridPositions = new HashSet<Vector3Int>();
        
        // Store spawned events
        private List<GameObject> spawnedEvents = new List<GameObject>();
        
        // Helper struct to store position and rotation
        private struct WallPosition
        {
            public Vector3 position;
            public Quaternion rotation;
            public GameObject plainWallCube; // Reference to plain wall cube (to be replaced by enemy button)
            
            public WallPosition(Vector3 pos, Quaternion rot, GameObject plainWall = null)
            {
                position = pos;
                rotation = rot;
                plainWallCube = plainWall;
            }
        }
        
        // Helper struct for event placement
        private struct EventPlacement
        {
            public EventData eventData;
            public Vector3 worldPosition;
            public Quaternion rotation;
            public PlacementType placementType;
            public Vector3Int gridPosition; // Starting grid position
            
            public EventPlacement(EventData data, Vector3 worldPos, Quaternion rot, PlacementType placement, Vector3Int gridPos)
            {
                eventData = data;
                worldPosition = worldPos;
                rotation = rot;
                placementType = placement;
                gridPosition = gridPos;
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
        /// NEW: Uses prefab-based system with wall markers
        /// </summary>
        private IEnumerator GenerateRoomCoroutine(int seed)
        {
            // Set random seed for deterministic generation
            Random.InitState(seed);
            
            // Clear previous generation
            ClearRoom();
            yield return null;
            
            // Check if using new prefab system or old procedural system
            if (roomConfig.roomPrefab != null)
            {
                // NEW PREFAB SYSTEM
                Log("Using prefab-based room system...");
                
                Log("Loading room prefab...");
                GameObject roomInstance = LoadRoomPrefab();
                if (roomInstance == null)
                {
                    Debug.LogError("[RoomGenerator] Failed to load room prefab!");
                    yield break;
                }
                yield return null;
                
                Log("Getting markers from manager...");
                List<WallMarker> markers = GetMarkersFromManager(roomInstance);
                if (markers == null || markers.Count == 0)
                {
                    Debug.LogError("[RoomGenerator] No markers found in room prefab!");
                    yield break;
                }
                yield return null;
                
                // Calculate room center from prefab bounds first
                CalculateRoomCenterFromPrefab(roomInstance);
                yield return null;
                
                // Place events before processing markers
                Log("Placing events...");
                PlaceEventsInPrefabRoom(markers);
                yield return null;
                
                Log($"Processing {markers.Count} markers...");
                ProcessMarkers(markers);
                yield return null;
            }
            else
            {
                // OLD PROCEDURAL SYSTEM (DEPRECATED)
                Log("Using old procedural room system (DEPRECATED)...");
            
            // Calculate room center
            roomCenter = new Vector3(
                roomConfig.roomWidth * roomConfig.cubeSize / 2f,
                roomConfig.roomHeight * roomConfig.cubeSize / 2f,
                roomConfig.roomDepth * roomConfig.cubeSize / 2f
            );
            
            Log("Generating floor and ceiling...");
            GenerateFloorAndCeiling();
            yield return null;
            
            // Place events before walls (events occupy grid positions)
            Log("Placing events...");
            PlaceEvents();
            yield return null;
            
            Log("Generating walls with buttons...");
            GenerateWallsWithButtons();
            yield return null;
            
            Log("Generating enemy spawn buttons...");
            GenerateEnemySpawnButtons();
            yield return null;
            }
            
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
                isEastOrWestWall: false, // North wall, corners already excluded
                wallStartGridPos: new Vector3Int(1, 0, roomConfig.roomDepth - 1) // Grid start position
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
                isEastOrWestWall: false, // South wall, corners already excluded
                wallStartGridPos: new Vector3Int(1, 0, 0) // Grid start position
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
                isEastOrWestWall: true, // East wall, will place cornerCube at first and last positions
                wallStartGridPos: new Vector3Int(roomConfig.roomWidth - 1, 0, 0) // Grid start position
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
                isEastOrWestWall: true, // West wall, will place cornerCube at first and last positions
                wallStartGridPos: new Vector3Int(0, 0, 0) // Grid start position
            );
        }
        
        /// <summary>
        /// Generate enemy spawn buttons on remaining available wall positions
        /// </summary>
        private void GenerateEnemySpawnButtons()
        {
            Log("[DEBUG] GenerateEnemySpawnButtons called!");
            
            // Check if enemy system is configured
            if (roomConfig.wallCubeWithEnemyButtonPrefab == null)
            {
                Log("[DEBUG] Enemy spawn button prefab not assigned. Skipping enemy button generation.");
                return;
            }
            
            Log($"[DEBUG] Enemy button prefab assigned: {roomConfig.wallCubeWithEnemyButtonPrefab.name}");
            
            if (roomConfig.enemyPool == null)
            {
                Log("[DEBUG] Enemy pool not assigned. Skipping enemy button generation.");
                return;
            }
            
            Log($"[DEBUG] Enemy pool assigned: {roomConfig.enemyPool.name}");
            
            if (!roomConfig.enemyPool.IsValid())
            {
                Debug.LogWarning("[RoomGenerator] Enemy pool validation failed!");
                Log("[DEBUG] Enemy pool validation FAILED!");
                return;
            }
            
            Log("[DEBUG] Enemy pool validation PASSED!");
            
            // Calculate how many enemy buttons to spawn
            int remainingWallPositions = availableWallPositions.Count;
            
            Log($"[DEBUG] Remaining wall positions: {remainingWallPositions}");
            
            if (remainingWallPositions == 0)
            {
                Log("[DEBUG] No available wall positions for enemy buttons.");
                return;
            }
            
            float enemyDensityPercent = Random.Range(roomConfig.minEnemyButtonDensityPercent, roomConfig.maxEnemyButtonDensityPercent);
            int targetEnemyButtonCount = Mathf.RoundToInt(remainingWallPositions * (enemyDensityPercent / 100f));
            
            // Clamp to available positions
            targetEnemyButtonCount = Mathf.Min(targetEnemyButtonCount, remainingWallPositions);
            
            Log($"Enemy Button Density: {enemyDensityPercent:F1}% ({targetEnemyButtonCount} enemy buttons out of {remainingWallPositions} remaining positions)");
            
            if (targetEnemyButtonCount == 0)
            {
                Log("No enemy buttons to spawn (density too low or no available positions).");
                return;
            }
            
            // Get random enemies from pool
            List<EnemyData> enemiesToPlace = roomConfig.enemyPool.GetRandomEnemies(targetEnemyButtonCount);
            
            // Shuffle available wall positions
            ShuffleList(availableWallPositions);
            
            // Spawn enemy buttons
            for (int i = 0; i < targetEnemyButtonCount && i < availableWallPositions.Count && i < enemiesToPlace.Count; i++)
            {
                WallPosition wallPos = availableWallPositions[i];
                EnemyData enemyData = enemiesToPlace[i];
                
                if (enemyData != null)
                {
                    SpawnWallCubeWithEnemyButton(wallPos, enemyData);
                }
            }
            
            Log($"Spawned {targetEnemyButtonCount} enemy spawn buttons");
        }
        
        /// <summary>
        /// Generate a single wall with buttons (based on button density)
        /// Pass wall start grid position for accurate occupied checking
        /// </summary>
        private void GenerateWall_Internal(Vector3 startPos, Vector3 widthDir, Vector3 heightDir,
            Quaternion rotation, int width, int height, Transform parent, List<ItemData> items, 
            HashSet<int> buttonPositionIndices, ref int itemIndex, ref int globalPositionIndex, bool isEastOrWestWall = false, Vector3Int wallStartGridPos = default)
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
                        // Calculate grid position based on wall direction and loop indices
                        Vector3Int gridPos = CalculateWallGridPosition(wallStartGridPos, widthDir, heightDir, w, h);
                        bool isOccupiedByEvent = occupiedGridPositions.Contains(gridPos);
                        
                        if (isOccupiedByEvent)
                        {
                            // Skip this position, it's occupied by an event
                            // Don't place wall or button here
                            Log($"Skipping wall cube at world: {position}, grid: {gridPos} - occupied by event");
                            globalPositionIndex++;
                            continue;
                        }
                        
                        // Check if this position should have a button
                        bool shouldPlaceButton = buttonPositionIndices.Contains(globalPositionIndex);
                        
                        if (shouldPlaceButton && itemIndex < items.Count)
                        {
                            // Place wall cube with button
                            WallPosition wallPos = new WallPosition(position, rotation);
                            SpawnWallCubeWithButton(wallPos, items[itemIndex], itemIndex < (itemPool.requiredItems?.Count ?? 0));
                            usedWallPositions.Add(wallPos);
                            itemIndex++;
                        }
                        else
                        {
                            // Place plain wall cube for now (will be replaced by enemy button if selected)
                            GameObject plainCube = null;
                            if (roomConfig.plainWallCubePrefab != null)
                            {
                                plainCube = Instantiate(roomConfig.plainWallCubePrefab, position, rotation, parent);
                                plainCube.name = $"PlainWall_{globalPositionIndex}";
                                
                                NetworkObject netObj = plainCube.GetComponent<NetworkObject>();
                                if (netObj != null)
                                {
                                    netObj.Spawn(true);
                                }
                                
                                generatedObjects.Add(plainCube);
                            }
                            
                            // This position is available for enemy buttons!
                            WallPosition wallPos = new WallPosition(position, rotation, plainCube);
                            availableWallPositions.Add(wallPos);
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
        /// Spawn a wall cube with enemy spawn button prefab at the given position
        /// </summary>
        private void SpawnWallCubeWithEnemyButton(WallPosition wallPos, EnemyData enemyData)
        {
            if (roomConfig.wallCubeWithEnemyButtonPrefab == null)
            {
                Debug.LogError("[RoomGenerator] WallCubeWithEnemyButton prefab is not assigned!");
                return;
            }
            
            if (enemyData == null)
            {
                Debug.LogError("[RoomGenerator] EnemyData is null! Cannot spawn enemy button.");
                return;
            }
            
            // Remove the plain wall cube if it exists
            if (wallPos.plainWallCube != null)
            {
                NetworkObject plainNetObj = wallPos.plainWallCube.GetComponent<NetworkObject>();
                if (plainNetObj != null && plainNetObj.IsSpawned)
                {
                    plainNetObj.Despawn(true);
                }
                else
                {
                    Destroy(wallPos.plainWallCube);
                }
                
                generatedObjects.Remove(wallPos.plainWallCube);
            }
            
            // Instantiate the wall cube with enemy button
            GameObject wallCubeObj = Instantiate(
                roomConfig.wallCubeWithEnemyButtonPrefab, 
                wallPos.position, 
                wallPos.rotation
            );
            wallCubeObj.name = $"WallCube_Enemy_{enemyData.enemyName}";
            
            // Find the EnemySpawnButton component (should be in the prefab hierarchy)
            EnemySpawnButton enemyButton = wallCubeObj.GetComponentInChildren<EnemySpawnButton>();
            if (enemyButton != null)
            {
                Log($"Setting EnemyData '{enemyData.enemyName}' to button at {wallPos.position}");
                enemyButton.SetEnemyData(enemyData);
            }
            else
            {
                Debug.LogWarning($"[RoomGenerator] WallCube at {wallPos.position} has no EnemySpawnButton component!");
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
                Debug.LogError($"[RoomGenerator] WallCubeWithEnemyButton prefab has no NetworkObject component!");
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
            occupiedGridPositions.Clear();
            spawnedEvents.Clear();
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
        
        #region Event Placement System
        
        /// <summary>
        /// Place events in the room before generating walls
        /// </summary>
        private void PlaceEvents()
        {
            if (roomConfig.eventPool == null)
            {
                Log("No event pool configured, skipping event placement");
                return;
            }
            
            if (!roomConfig.eventPool.Validate())
            {
                Debug.LogError("[RoomGenerator] Event pool validation failed!");
                return;
            }
            
            List<EventData> eventsToPlace = new List<EventData>();
            
            // Add required events first
            if (roomConfig.eventPool.requiredEvents != null)
            {
                foreach (var eventData in roomConfig.eventPool.requiredEvents)
                {
                    if (eventData != null)
                    {
                        eventsToPlace.Add(eventData);
                    }
                }
            }
            
            // Add random events
            int randomEventCount = Random.Range(
                roomConfig.eventPool.minRandomEvents,
                roomConfig.eventPool.maxRandomEvents + 1
            );
            
            for (int i = 0; i < randomEventCount; i++)
            {
                EventData randomEvent = roomConfig.eventPool.GetRandomEvent();
                if (randomEvent != null)
                {
                    eventsToPlace.Add(randomEvent);
                }
            }
            
            Log($"Placing {eventsToPlace.Count} events ({roomConfig.eventPool.requiredEvents?.Count ?? 0} required, {randomEventCount} random)");
            
            // Try to place each event
            foreach (var eventData in eventsToPlace)
            {
                if (TryPlaceEvent(eventData, out EventPlacement placement))
                {
                    SpawnEvent(placement);
                    
                    // Add event's required items to item pool for buttons
                    if (eventData.HasRequiredItems)
                    {
                        AssignRequiredItemsToButtons(eventData);
                    }
                }
                else
                {
                    Debug.LogWarning($"[RoomGenerator] Failed to place event: {eventData.eventName}");
                }
            }
        }
        
        /// <summary>
        /// Try to place an event in the room
        /// </summary>
        private bool TryPlaceEvent(EventData eventData, out EventPlacement placement)
        {
            placement = default;
            
            if (eventData == null || eventData.eventPrefab == null)
            {
                return false;
            }
            
            // Determine where to try placing based on placement type
            PlacementType[] placementTypes;
            
            switch (eventData.placementType)
            {
                case PlacementType.Wall:
                    placementTypes = new[] { PlacementType.Wall };
                    break;
                case PlacementType.Floor:
                    placementTypes = new[] { PlacementType.Floor };
                    break;
                case PlacementType.Ceiling:
                    placementTypes = new[] { PlacementType.Ceiling };
                    break;
                case PlacementType.Any:
                    placementTypes = new[] { PlacementType.Wall, PlacementType.Floor, PlacementType.Ceiling };
                    break;
                default:
                    placementTypes = new[] { PlacementType.Wall };
                    break;
            }
            
            // Shuffle placement types for randomness
            placementTypes = placementTypes.OrderBy(x => Random.value).ToArray();
            
            // Try each placement type
            foreach (var pType in placementTypes)
            {
                if (TryFindSpaceForEvent(eventData, pType, out placement))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Try to find available space for an event
        /// </summary>
        private bool TryFindSpaceForEvent(EventData eventData, PlacementType placementType, out EventPlacement placement)
        {
            placement = default;
            
            // Get list of possible positions based on placement type
            List<Vector3Int> possiblePositions = GetPossiblePositionsForPlacement(placementType, eventData.size);
            
            if (possiblePositions.Count == 0)
            {
                return false;
            }
            
            // Shuffle positions for randomness
            ShuffleList(possiblePositions);
            
            // Try each position
            foreach (var gridPos in possiblePositions)
            {
                if (CanPlaceEventAt(gridPos, eventData.size, placementType))
                {
                    // Calculate world position and rotation
                    Vector3 worldPos = GridToWorldPosition(gridPos, eventData.size, placementType);
                    Quaternion rotation = GetRotationForPlacement(gridPos, placementType);
                    
                    placement = new EventPlacement(eventData, worldPos, rotation, placementType, gridPos);
                    
                    // Mark space as occupied
                    MarkSpaceAsOccupied(gridPos, eventData.size, placementType);
                    
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Get list of possible grid positions for a placement type
        /// </summary>
        private List<Vector3Int> GetPossiblePositionsForPlacement(PlacementType placementType, Vector3Int size)
        {
            List<Vector3Int> positions = new List<Vector3Int>();
            
            switch (placementType)
            {
                case PlacementType.Wall:
                    // North wall (Z = depth - 1)
                    for (int x = 1; x < roomConfig.roomWidth - 1 - size.x + 1; x++)
                    {
                        for (int y = 0; y < roomConfig.roomHeight - 1 - size.y + 1; y++)
                        {
                            positions.Add(new Vector3Int(x, y, roomConfig.roomDepth - 1));
                        }
                    }
                    
                    // South wall (Z = 0)
                    for (int x = 1; x < roomConfig.roomWidth - 1 - size.x + 1; x++)
                    {
                        for (int y = 0; y < roomConfig.roomHeight - 1 - size.y + 1; y++)
                        {
                            positions.Add(new Vector3Int(x, y, 0));
                        }
                    }
                    
                    // East wall (X = width - 1)
                    for (int z = 1; z < roomConfig.roomDepth - 1 - size.x + 1; z++)
                    {
                        for (int y = 0; y < roomConfig.roomHeight - 1 - size.y + 1; y++)
                        {
                            positions.Add(new Vector3Int(roomConfig.roomWidth - 1, y, z));
                        }
                    }
                    
                    // West wall (X = 0)
                    for (int z = 1; z < roomConfig.roomDepth - 1 - size.x + 1; z++)
                    {
                        for (int y = 0; y < roomConfig.roomHeight - 1 - size.y + 1; y++)
                        {
                            positions.Add(new Vector3Int(0, y, z));
                        }
                    }
                    break;
                    
                case PlacementType.Floor:
                    // Floor (Y = 0)
                    for (int x = 1; x < roomConfig.roomWidth - 1 - size.x + 1; x++)
                    {
                        for (int z = 1; z < roomConfig.roomDepth - 1 - size.z + 1; z++)
                        {
                            positions.Add(new Vector3Int(x, 0, z));
                        }
                    }
                    break;
                    
                case PlacementType.Ceiling:
                    // Ceiling (Y = height - 1)
                    for (int x = 1; x < roomConfig.roomWidth - 1 - size.x + 1; x++)
                    {
                        for (int z = 1; z < roomConfig.roomDepth - 1 - size.z + 1; z++)
                        {
                            positions.Add(new Vector3Int(x, roomConfig.roomHeight - 1, z));
                        }
                    }
                    break;
            }
            
            return positions;
        }
        
        /// <summary>
        /// Check if an event can be placed at the given grid position
        /// For walls, size is interpreted based on wall orientation
        /// </summary>
        private bool CanPlaceEventAt(Vector3Int gridPos, Vector3Int size, PlacementType placementType)
        {
            if (placementType == PlacementType.Wall)
            {
                // Determine wall orientation
                bool isWestWall = (gridPos.x == 0);
                bool isEastWall = (gridPos.x == roomConfig.roomWidth - 1);
                bool isSouthWall = (gridPos.z == 0);
                bool isNorthWall = (gridPos.z == roomConfig.roomDepth - 1);
                
                if (isWestWall || isEastWall)
                {
                    // West/East wall: size.x = Z (width), size.y = Y (height), size.z = X (depth)
                    for (int z = 0; z < size.x; z++) // Width along Z
                    {
                        for (int y = 0; y < size.y; y++) // Height along Y
                        {
                            for (int x = 0; x < size.z; x++) // Depth along X
                            {
                                Vector3Int checkPos = gridPos + new Vector3Int(x, y, z);
                                
                                // Check if already occupied
                                if (occupiedGridPositions.Contains(checkPos))
                                {
                                    return false;
                                }
                                
                                // Check bounds
                                if (checkPos.x < 0 || checkPos.x >= roomConfig.roomWidth ||
                                    checkPos.y < 0 || checkPos.y >= roomConfig.roomHeight ||
                                    checkPos.z < 0 || checkPos.z >= roomConfig.roomDepth)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                else if (isSouthWall || isNorthWall)
                {
                    // North/South wall: size.x = X (width), size.y = Y (height), size.z = Z (depth)
                    for (int x = 0; x < size.x; x++) // Width along X
                    {
                        for (int y = 0; y < size.y; y++) // Height along Y
                        {
                            for (int z = 0; z < size.z; z++) // Depth along Z
                            {
                                Vector3Int checkPos = gridPos + new Vector3Int(x, y, z);
                                
                                // Check if already occupied
                                if (occupiedGridPositions.Contains(checkPos))
                                {
                                    return false;
                                }
                                
                                // Check bounds
                                if (checkPos.x < 0 || checkPos.x >= roomConfig.roomWidth ||
                                    checkPos.y < 0 || checkPos.y >= roomConfig.roomHeight ||
                                    checkPos.z < 0 || checkPos.z >= roomConfig.roomDepth)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Floor/Ceiling: straightforward X, Y, Z
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        for (int z = 0; z < size.z; z++)
                        {
                            Vector3Int checkPos = gridPos + new Vector3Int(x, y, z);
                            
                            // Check if already occupied
                            if (occupiedGridPositions.Contains(checkPos))
                            {
                                return false;
                            }
                            
                            // Check bounds
                            if (checkPos.x < 0 || checkPos.x >= roomConfig.roomWidth ||
                                checkPos.y < 0 || checkPos.y >= roomConfig.roomHeight ||
                                checkPos.z < 0 || checkPos.z >= roomConfig.roomDepth)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Mark grid space as occupied by an event
        /// For walls, size is interpreted based on wall orientation
        /// </summary>
        private void MarkSpaceAsOccupied(Vector3Int gridPos, Vector3Int size, PlacementType placementType)
        {
            if (placementType == PlacementType.Wall)
            {
                // Determine wall orientation
                bool isWestWall = (gridPos.x == 0);
                bool isEastWall = (gridPos.x == roomConfig.roomWidth - 1);
                bool isSouthWall = (gridPos.z == 0);
                bool isNorthWall = (gridPos.z == roomConfig.roomDepth - 1);
                
                if (isWestWall || isEastWall)
                {
                    // West/East wall: size.x = Z (width), size.y = Y (height), size.z = X (depth)
                    for (int z = 0; z < size.x; z++) // Width along Z
                    {
                        for (int y = 0; y < size.y; y++) // Height along Y
                        {
                            for (int x = 0; x < size.z; x++) // Depth along X (usually 1)
                            {
                                Vector3Int pos = gridPos + new Vector3Int(x, y, z);
                                occupiedGridPositions.Add(pos);
                            }
                        }
                    }
                }
                else if (isSouthWall || isNorthWall)
                {
                    // North/South wall: size.x = X (width), size.y = Y (height), size.z = Z (depth)
                    for (int x = 0; x < size.x; x++) // Width along X
                    {
                        for (int y = 0; y < size.y; y++) // Height along Y
                        {
                            for (int z = 0; z < size.z; z++) // Depth along Z (usually 1)
                            {
                                Vector3Int pos = gridPos + new Vector3Int(x, y, z);
                                occupiedGridPositions.Add(pos);
                            }
                        }
                    }
                }
            }
            else
            {
                // Floor/Ceiling: straightforward X, Y, Z
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        for (int z = 0; z < size.z; z++)
                        {
                            Vector3Int pos = gridPos + new Vector3Int(x, y, z);
                            occupiedGridPositions.Add(pos);
                        }
                    }
                }
            }
            
            Log($"Marked {size.x}x{size.y}x{size.z} blocks as occupied at {gridPos} ({placementType})");
        }
        
        /// <summary>
        /// Convert grid position to world position (for single block)
        /// </summary>
        private Vector3 GridToWorldPosition(Vector3Int gridPos)
        {
            return new Vector3(
                gridPos.x * roomConfig.cubeSize + roomConfig.cubeSize / 2f,
                gridPos.y * roomConfig.cubeSize + roomConfig.cubeSize / 2f,
                gridPos.z * roomConfig.cubeSize + roomConfig.cubeSize / 2f
            );
        }
        
        /// <summary>
        /// Convert grid position to world position for multi-block events
        /// Returns the CENTER position of the event (for center pivot prefabs)
        /// </summary>
        private Vector3 GridToWorldPosition(Vector3Int gridPos, Vector3Int size, PlacementType placementType)
        {
            // Base position (corner of the starting block)
            Vector3 basePos = new Vector3(
                gridPos.x * roomConfig.cubeSize,
                gridPos.y * roomConfig.cubeSize,
                gridPos.z * roomConfig.cubeSize
            );
            
            // Wall placement: Y coordinate needs offset (walls start at cubeSize, not 0)
            if (placementType == PlacementType.Wall)
            {
                basePos.y += roomConfig.cubeSize; // Walls start at Y=1, not Y=0
            }
            
            // Calculate center offset based on event size and wall direction
            Vector3 centerOffset = Vector3.zero;
            
            if (placementType == PlacementType.Wall)
            {
                // For wall placement, determine which wall based on grid position
                bool isWestWall = (gridPos.x == 0);
                bool isEastWall = (gridPos.x == roomConfig.roomWidth - 1);
                bool isSouthWall = (gridPos.z == 0);
                bool isNorthWall = (gridPos.z == roomConfig.roomDepth - 1);
                
                // Apply center offset only on axes PARALLEL to the wall
                // NOT on the axis perpendicular to the wall (event embeds into wall)
                
                if (isWestWall || isEastWall)
                {
                    // West/East wall: size.x = Z (width), size.y = Y (height), size.z = X (depth)
                    centerOffset.x = 0; // NO X offset (into wall, depth always minimal)
                    centerOffset.y = (size.y - 1) * roomConfig.cubeSize / 2f; // Height
                    centerOffset.z = (size.x - 1) * roomConfig.cubeSize / 2f; // Width (size.x maps to Z!)
                }
                else if (isSouthWall || isNorthWall)
                {
                    // North/South wall: size.x = X (width), size.y = Y (height), size.z = Z (depth)
                    centerOffset.x = (size.x - 1) * roomConfig.cubeSize / 2f; // Width
                    centerOffset.y = (size.y - 1) * roomConfig.cubeSize / 2f; // Height
                    centerOffset.z = 0; // NO Z offset (into wall, depth always minimal)
                }
            }
            else
            {
                // Floor/Ceiling: center offset on all axes
                centerOffset = new Vector3(
                    (size.x - 1) * roomConfig.cubeSize / 2f,
                    (size.y - 1) * roomConfig.cubeSize / 2f,
                    (size.z - 1) * roomConfig.cubeSize / 2f
                );
            }
            
            return basePos + centerOffset;
        }
        
        /// <summary>
        /// Convert world position to grid position
        /// Assumes corner pivot positioning (matching wall cubes)
        /// </summary>
        private Vector3Int WorldToGridPosition(Vector3 worldPos)
        {
            // Wall cubes use corner pivot, so no offset needed
            // Direct division and rounding
            return new Vector3Int(
                Mathf.RoundToInt(worldPos.x / roomConfig.cubeSize),
                Mathf.RoundToInt(worldPos.y / roomConfig.cubeSize),
                Mathf.RoundToInt(worldPos.z / roomConfig.cubeSize)
            );
        }
        
        /// <summary>
        /// Convert wall world position to grid position
        /// Wall coordinates start at Y=cubeSize, so need to adjust
        /// </summary>
        private Vector3Int WorldToGridPositionForWall(Vector3 worldPos)
        {
            // Walls start at Y = cubeSize (1), but grid starts at 0
            // So World Y=1 → Grid Y=0, World Y=2 → Grid Y=1, etc.
            return new Vector3Int(
                Mathf.RoundToInt(worldPos.x / roomConfig.cubeSize),
                Mathf.RoundToInt((worldPos.y - roomConfig.cubeSize) / roomConfig.cubeSize),
                Mathf.RoundToInt(worldPos.z / roomConfig.cubeSize)
            );
        }
        
        /// <summary>
        /// Calculate wall grid position based on start position and loop indices
        /// More accurate than world→grid conversion
        /// </summary>
        private Vector3Int CalculateWallGridPosition(Vector3Int startGridPos, Vector3 widthDir, Vector3 heightDir, int w, int h)
        {
            // Determine which axis to increment based on widthDir
            Vector3Int gridPos = startGridPos;
            
            // Width direction (horizontal along the wall)
            if (widthDir == Vector3.right)
            {
                gridPos.x += w; // North/South walls move in X
            }
            else if (widthDir == Vector3.forward)
            {
                gridPos.z += w; // East/West walls move in Z
            }
            else if (widthDir == Vector3.left)
            {
                gridPos.x -= w;
            }
            else if (widthDir == Vector3.back)
            {
                gridPos.z -= w;
            }
            
            // Height direction (always up for walls)
            gridPos.y += h;
            
            return gridPos;
        }
        
        /// <summary>
        /// Get rotation for event based on grid position and placement type
        /// </summary>
        private Quaternion GetRotationForPlacement(Vector3Int gridPos, PlacementType placementType)
        {
            if (placementType != PlacementType.Wall)
            {
                return Quaternion.identity;
            }
            
            // Determine which wall based on position
            // North wall (Z = depth - 1) - face south (180 degrees)
            if (gridPos.z == roomConfig.roomDepth - 1)
            {
                return Quaternion.Euler(0, 180, 0);
            }
            // South wall (Z = 0) - face north (0 degrees)
            else if (gridPos.z == 0)
            {
                return Quaternion.Euler(0, 0, 0);
            }
            // East wall (X = width - 1) - face west (270 degrees)
            else if (gridPos.x == roomConfig.roomWidth - 1)
            {
                return Quaternion.Euler(0, 270, 0);
            }
            // West wall (X = 0) - face east (90 degrees)
            else if (gridPos.x == 0)
            {
                return Quaternion.Euler(0, 90, 0);
            }
            
            return Quaternion.identity;
        }
        
        /// <summary>
        /// Spawn an event in the room
        /// </summary>
        private void SpawnEvent(EventPlacement placement)
        {
            if (placement.eventData == null || placement.eventData.eventPrefab == null)
            {
                return;
            }
            
            GameObject eventObj = Instantiate(
                placement.eventData.eventPrefab,
                placement.worldPosition,
                placement.rotation
            );
            
            eventObj.name = $"Event_{placement.eventData.eventName}";
            
            // Set required items on the event if it supports it
            var interactableEvent = eventObj.GetComponent<Interactables.InteractableEvent>();
            if (interactableEvent != null && placement.eventData.HasRequiredItems)
            {
                interactableEvent.SetRequiredItems(placement.eventData.requiredItems);
            }
            
            // Network spawn
            NetworkObject netObj = eventObj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
            }
            else
            {
                Debug.LogWarning($"[RoomGenerator] Event {placement.eventData.eventName} has no NetworkObject component!");
            }
            
            spawnedEvents.Add(eventObj);
            generatedObjects.Add(eventObj);
            
            Log($"Spawned event '{placement.eventData.eventName}' at {placement.worldPosition} ({placement.placementType})");
        }
        
        /// <summary>
        /// Add event's required items to the item pool so they spawn on buttons
        /// </summary>
        private void AssignRequiredItemsToButtons(EventData eventData)
        {
            if (!eventData.HasRequiredItems)
            {
                return;
            }
            
            foreach (var item in eventData.requiredItems)
            {
                if (item != null)
                {
                    // Add to required items list
                    if (!itemPool.requiredItems.Contains(item))
                    {
                        itemPool.requiredItems.Add(item);
                        Log($"Added required item '{item.itemName}' for event '{eventData.eventName}'");
                    }
                }
            }
        }
        
        #endregion
        
        #region Prefab-Based Room System (NEW)
        
        /// <summary>
        /// Load and instantiate the room prefab
        /// </summary>
        private GameObject LoadRoomPrefab()
        {
            if (roomConfig.roomPrefab == null)
            {
                Debug.LogError("[RoomGenerator] Room prefab is not assigned in RoomConfiguration!");
                return null;
            }
            
            GameObject roomInstance = Instantiate(roomConfig.roomPrefab, Vector3.zero, Quaternion.identity);
            roomInstance.name = "Room_Instance";
            
            generatedObjects.Add(roomInstance);
            
            Log($"Loaded room prefab: {roomConfig.roomPrefab.name}");
            return roomInstance;
        }
        
        /// <summary>
        /// Get wall markers from the room prefab manager
        /// </summary>
        private List<WallMarker> GetMarkersFromManager(GameObject roomInstance)
        {
            RoomPrefabManager manager = roomInstance.GetComponent<RoomPrefabManager>();
            if (manager == null)
            {
                Debug.LogError("[RoomGenerator] Room prefab does not have RoomPrefabManager component!");
                return null;
            }
            
            if (!manager.Validate())
            {
                Debug.LogError("[RoomGenerator] RoomPrefabManager validation failed!");
                return null;
            }
            
            List<WallMarker> markers = manager.GetAllMarkers();
            Log($"Found {markers.Count} markers in room prefab '{manager.roomName}'");
            
            return markers;
        }
        
        /// <summary>
        /// Process markers and replace them with buttons/events
        /// </summary>
        private void ProcessMarkers(List<WallMarker> markers)
        {
            if (markers == null || markers.Count == 0)
            {
                Debug.LogWarning("[RoomGenerator] No markers to process!");
                return;
            }
            
            // Remove null markers and markers already used by events
            markers.RemoveAll(m => m == null);
            
            // IMPORTANT: Filter out markers that are already disabled (used by events)
            List<WallMarker> availableMarkers = new List<WallMarker>();
            foreach (var marker in markers)
            {
                if (marker.gameObject.activeSelf && marker.markerRenderer != null && marker.markerRenderer.enabled)
                {
                    availableMarkers.Add(marker);
                }
            }
            
            int totalMarkers = availableMarkers.Count;
            
            if (totalMarkers == 0)
            {
                Log("No available markers remaining after event placement");
                return;
            }
            
            Log($"Available markers for buttons: {totalMarkers} (after event placement)");
            
            // Shuffle markers for randomness
            ShuffleList(availableMarkers);
            
            // Calculate item button density
            float itemDensityPercent = Random.Range(roomConfig.minButtonDensityPercent, roomConfig.maxButtonDensityPercent);
            int itemButtonCount = Mathf.RoundToInt(totalMarkers * (itemDensityPercent / 100f));
            
            Log($"Item Button Density: {itemDensityPercent:F1}% ({itemButtonCount} buttons out of {totalMarkers} markers)");
            
            // Collect items to place
            List<ItemData> itemsToPlace = new List<ItemData>();
            
            // Add required items first
            if (itemPool.requiredItems != null)
            {
                itemsToPlace.AddRange(itemPool.requiredItems);
            }
            
            int requiredItemCount = itemsToPlace.Count;
            
            // Ensure we have enough buttons for required items
            if (itemButtonCount < requiredItemCount)
            {
                Log($"Warning: Button density too low! Required items: {requiredItemCount}, target buttons: {itemButtonCount}. Increasing to minimum.");
                itemButtonCount = requiredItemCount;
            }
            
            // Fill remaining button slots with random items
            int randomButtonSlots = itemButtonCount - requiredItemCount;
            for (int i = 0; i < randomButtonSlots; i++)
            {
                ItemData randomItem = itemPool.GetRandomItem();
                if (randomItem != null)
                {
                    itemsToPlace.Add(randomItem);
                }
            }
            
            Log($"Placing {itemsToPlace.Count} item buttons ({requiredItemCount} required, {randomButtonSlots} random)");
            
            // Place item buttons
            int markerIndex = 0;
            for (int i = 0; i < itemsToPlace.Count && markerIndex < availableMarkers.Count; i++, markerIndex++)
            {
                ReplaceMarkerWithItemButton(availableMarkers[markerIndex], itemsToPlace[i]);
            }
            
            // Calculate enemy button density from remaining markers
            int remainingMarkers = availableMarkers.Count - markerIndex;
            float enemyDensityPercent = Random.Range(roomConfig.minEnemyButtonDensityPercent, roomConfig.maxEnemyButtonDensityPercent);
            int enemyButtonCount = Mathf.RoundToInt(remainingMarkers * (enemyDensityPercent / 100f));
            
            Log($"Enemy Button Density: {enemyDensityPercent:F1}% ({enemyButtonCount} buttons out of {remainingMarkers} remaining markers)");
            
            // Place enemy buttons
            if (roomConfig.enemyPool != null)
            {
                for (int i = 0; i < enemyButtonCount && markerIndex < availableMarkers.Count; i++, markerIndex++)
                {
                    EnemyData enemyData = roomConfig.enemyPool.GetRandomEnemy();
                    if (enemyData != null)
                    {
                        ReplaceMarkerWithEnemyButton(availableMarkers[markerIndex], enemyData);
                    }
                }
            }
            
            // Remaining markers stay as walls (no replacement needed)
            int remainingWalls = availableMarkers.Count - markerIndex;
            Log($"Remaining {remainingWalls} markers will stay as walls");
        }
        
        /// <summary>
        /// Replace a marker with an item spawn button
        /// </summary>
        private void ReplaceMarkerWithItemButton(WallMarker marker, ItemData itemData)
        {
            if (marker == null || itemData == null)
            {
                return;
            }
            
            if (roomConfig.wallCubeWithButtonPrefab == null)
            {
                Debug.LogError("[RoomGenerator] wallCubeWithButtonPrefab is not assigned!");
                return;
            }
            
            // Instantiate button at marker position
            GameObject button = Instantiate(
                roomConfig.wallCubeWithButtonPrefab,
                marker.transform.position,
                marker.transform.rotation
            );
            
            button.name = $"ItemButton_{itemData.itemName}";
            
            // Set item data on spawn button
            var spawnButton = button.GetComponent<SpawnButton>();
            if (spawnButton != null)
            {
                spawnButton.SetItemData(itemData);
            }
            else
            {
                Debug.LogWarning($"[RoomGenerator] Button prefab does not have SpawnButton component!");
            }
            
            // Network spawn
            NetworkObject netObj = button.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
            }
            else
            {
                Debug.LogWarning($"[RoomGenerator] Button prefab does not have NetworkObject component!");
            }
            
            // Disable marker
            marker.DisableMarker();
            
            // Track
            generatedObjects.Add(button);
            
            Log($"Replaced marker #{marker.markerId} with item button: {itemData.itemName}");
        }
        
        /// <summary>
        /// Replace a marker with an enemy spawn button
        /// </summary>
        private void ReplaceMarkerWithEnemyButton(WallMarker marker, EnemyData enemyData)
        {
            if (marker == null || enemyData == null)
            {
                return;
            }
            
            if (roomConfig.wallCubeWithEnemyButtonPrefab == null)
            {
                Debug.LogError("[RoomGenerator] wallCubeWithEnemyButtonPrefab is not assigned!");
                return;
            }
            
            // Instantiate enemy button at marker position
            GameObject button = Instantiate(
                roomConfig.wallCubeWithEnemyButtonPrefab,
                marker.transform.position,
                marker.transform.rotation
            );
            
            button.name = $"EnemyButton_{enemyData.enemyName}";
            
            // Set enemy data on spawn button
            var enemySpawnButton = button.GetComponent<EnemySpawnButton>();
            if (enemySpawnButton != null)
            {
                enemySpawnButton.SetEnemyData(enemyData);
            }
            else
            {
                Debug.LogWarning($"[RoomGenerator] Enemy button prefab does not have EnemySpawnButton component!");
            }
            
            // Network spawn
            NetworkObject netObj = button.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
            }
            else
            {
                Debug.LogWarning($"[RoomGenerator] Enemy button prefab does not have NetworkObject component!");
            }
            
            // Disable marker
            marker.DisableMarker();
            
            // Track
            generatedObjects.Add(button);
            
            Log($"Replaced marker #{marker.markerId} with enemy button: {enemyData.enemyName}");
        }
        
        /// <summary>
        /// Place events in prefab room using markers
        /// </summary>
        private void PlaceEventsInPrefabRoom(List<WallMarker> markers)
        {
            if (roomConfig.eventPool == null)
            {
                Log("No event pool configured, skipping event placement");
                return;
            }
            
            if (!roomConfig.eventPool.Validate())
            {
                Debug.LogError("[RoomGenerator] Event pool validation failed!");
                return;
            }
            
            List<EventData> eventsToPlace = new List<EventData>();
            
            // Add required events first
            if (roomConfig.eventPool.requiredEvents != null)
            {
                foreach (var eventData in roomConfig.eventPool.requiredEvents)
                {
                    if (eventData != null)
                    {
                        eventsToPlace.Add(eventData);
                    }
                }
            }
            
            // Add random events
            int randomEventCount = Random.Range(
                roomConfig.eventPool.minRandomEvents,
                roomConfig.eventPool.maxRandomEvents + 1
            );
            
            for (int i = 0; i < randomEventCount; i++)
            {
                EventData randomEvent = roomConfig.eventPool.GetRandomEvent();
                if (randomEvent != null)
                {
                    eventsToPlace.Add(randomEvent);
                }
            }
            
            Log($"Placing {eventsToPlace.Count} events ({roomConfig.eventPool.requiredEvents?.Count ?? 0} required, {randomEventCount} random)");
            
            // Try to place each event using markers
            foreach (var eventData in eventsToPlace)
            {
                if (TryPlaceEventOnMarker(eventData, markers))
                {
                    // Add event's required items to item pool for buttons
                    if (eventData.HasRequiredItems)
                    {
                        AssignRequiredItemsToButtons(eventData);
                    }
                }
                else
                {
                    Debug.LogWarning($"[RoomGenerator] Failed to place event: {eventData.eventName}");
                }
            }
        }
        
        /// <summary>
        /// Try to place an event on a marker position
        /// </summary>
        private bool TryPlaceEventOnMarker(EventData eventData, List<WallMarker> markers)
        {
            if (eventData == null || eventData.eventPrefab == null)
            {
                return false;
            }
            
            // Find available markers (not already used)
            List<WallMarker> availableMarkers = new List<WallMarker>();
            foreach (var marker in markers)
            {
                if (marker != null && marker.gameObject.activeSelf && marker.markerRenderer != null && marker.markerRenderer.enabled)
                {
                    availableMarkers.Add(marker);
                }
            }
            
            if (availableMarkers.Count == 0)
            {
                Debug.LogWarning($"[RoomGenerator] No available markers for event: {eventData.eventName}");
                return false;
            }
            
            // Shuffle for randomness
            ShuffleList(availableMarkers);
            
            // Try to find space for multi-block event
            WallMarker selectedMarker = null;
            List<WallMarker> requiredMarkers = new List<WallMarker>();
            
            foreach (var marker in availableMarkers)
            {
                // Check if we can place event here (considering size)
                if (CanPlaceEventAtMarker(marker, eventData, availableMarkers, out requiredMarkers))
                {
                    selectedMarker = marker;
                    break;
                }
            }
            
            if (selectedMarker == null)
            {
                Debug.LogWarning($"[RoomGenerator] No suitable marker found for event: {eventData.eventName} (size: {eventData.size})");
                return false;
            }
            
            // Calculate proper position and rotation for event
            Vector3 eventPosition = CalculateEventPosition(selectedMarker, eventData);
            Quaternion eventRotation = CalculateEventRotation(selectedMarker, eventData);
            
            // Instantiate event with calculated position and rotation
            GameObject eventObj = Instantiate(
                eventData.eventPrefab,
                eventPosition,
                eventRotation
            );
            
            eventObj.name = $"Event_{eventData.eventName}";
            
            // Set required items on the event if it supports it
            var interactableEvent = eventObj.GetComponent<Interactables.InteractableEvent>();
            if (interactableEvent != null && eventData.HasRequiredItems)
            {
                interactableEvent.SetRequiredItems(eventData.requiredItems);
            }
            
            // Network spawn
            NetworkObject netObj = eventObj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
            }
            else
            {
                Debug.LogWarning($"[RoomGenerator] Event {eventData.eventName} has no NetworkObject component!");
            }
            
            // Disable all markers used by this event
            foreach (var marker in requiredMarkers)
            {
                marker.DisableMarker();
            }
            
            // Track
            spawnedEvents.Add(eventObj);
            generatedObjects.Add(eventObj);
            
            Log($"Placed event '{eventData.eventName}' at marker position (size: {eventData.size}, markers used: {requiredMarkers.Count})");
            
            return true;
        }
        
        /// <summary>
        /// Check if event can be placed at marker (considering size)
        /// </summary>
        private bool CanPlaceEventAtMarker(WallMarker marker, EventData eventData, List<WallMarker> availableMarkers, out List<WallMarker> requiredMarkers)
        {
            requiredMarkers = new List<WallMarker>();
            requiredMarkers.Add(marker);
            
            // If event is 1x1, we only need one marker
            if (eventData.size.x <= 1 && eventData.size.y <= 1 && eventData.size.z <= 1)
            {
                return true;
            }
            
            // For multi-block events, find adjacent markers using world space grid search
            Vector3 markerPos = marker.transform.position;
            float cubeSize = roomConfig.cubeSize;
            float searchRadius = cubeSize * 0.4f; // Tolerance for finding adjacent markers
            
            // Determine wall orientation from marker rotation
            Vector3 markerForward = marker.transform.forward;
            Vector3 markerRight = marker.transform.right;
            Vector3 markerUp = marker.transform.up;
            
            // Calculate required positions based on event size
            // Event size: (width, height, depth)
            // For walls: width = horizontal, height = vertical, depth = into wall (usually 1)
            int requiredCount = eventData.size.x * eventData.size.y;
            
            // Try to find adjacent markers in a grid pattern
            // Start from base marker and search right (width) and up (height)
            for (int w = 0; w < eventData.size.x; w++)
            {
                for (int h = 0; h < eventData.size.y; h++)
                {
                    if (w == 0 && h == 0) continue; // Already have the first marker
                    
                    // Calculate target position using marker's local axes
                    Vector3 targetPos = markerPos + (markerRight * w * cubeSize) + (markerUp * h * cubeSize);
                    
                    // Find marker at this position
                    WallMarker adjacentMarker = FindMarkerAtPosition(targetPos, availableMarkers, searchRadius);
                    if (adjacentMarker != null && !requiredMarkers.Contains(adjacentMarker))
                    {
                        requiredMarkers.Add(adjacentMarker);
                    }
                }
            }
            
            // For multi-block events, we need ALL required markers
            // Otherwise the event will be placed incorrectly
            bool hasAllMarkers = requiredMarkers.Count >= requiredCount;
            
            if (!hasAllMarkers)
            {
                // Debug info
                Log($"Event '{eventData.eventName}' size {eventData.size} needs {requiredCount} markers, found {requiredMarkers.Count} at position {markerPos}");
            }
            
            return hasAllMarkers;
        }
        
        /// <summary>
        /// Find marker at specific position
        /// </summary>
        private WallMarker FindMarkerAtPosition(Vector3 position, List<WallMarker> markers, float tolerance)
        {
            foreach (var marker in markers)
            {
                if (marker != null && Vector3.Distance(marker.transform.position, position) < tolerance)
                {
                    return marker;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Calculate event position with proper offset
        /// </summary>
        private Vector3 CalculateEventPosition(WallMarker marker, EventData eventData)
        {
            Vector3 basePos = marker.transform.position;
            
            // Apply offset based on event size
            // Center the event on the marker
            Vector3 markerRight = marker.transform.right;
            Vector3 markerUp = marker.transform.up;
            Vector3 markerForward = marker.transform.forward;
            
            float cubeSize = roomConfig.cubeSize;
            
            // Offset to center multi-block events
            Vector3 centerOffset = Vector3.zero;
            
            if (eventData.size.x > 1)
            {
                centerOffset += markerRight * (eventData.size.x - 1) * cubeSize * 0.5f;
            }
            
            if (eventData.size.y > 1)
            {
                centerOffset += markerUp * (eventData.size.y - 1) * cubeSize * 0.5f;
            }
            
            // Small forward offset to prevent z-fighting with wall
            centerOffset += markerForward * 0.01f;
            
            return basePos + centerOffset;
        }
        
        /// <summary>
        /// Calculate event rotation based on marker orientation
        /// </summary>
        private Quaternion CalculateEventRotation(WallMarker marker, EventData eventData)
        {
            // Use marker's rotation directly
            // Marker should already be oriented correctly for the wall it's on
            return marker.transform.rotation;
        }
        
        /// <summary>
        /// Calculate room center from prefab bounds
        /// </summary>
        private void CalculateRoomCenterFromPrefab(GameObject roomInstance)
        {
            // Get all renderers in the room
            Renderer[] renderers = roomInstance.GetComponentsInChildren<Renderer>();
            
            if (renderers.Length == 0)
            {
                roomCenter = roomInstance.transform.position;
                Log($"Room center (no renderers): {roomCenter}");
                return;
            }
            
            // Calculate bounds
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            
            roomCenter = bounds.center;
            Log($"Room center calculated from bounds: {roomCenter}");
        }
        
        #endregion
    }
}

