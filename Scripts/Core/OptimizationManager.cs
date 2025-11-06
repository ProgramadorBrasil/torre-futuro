using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections.Generic;

namespace SpaceRPG.Core
{
    /// <summary>
    /// Sistema de otimização usando Burst Compiler e Jobs System
    /// Otimiza física, colisões e processamento em larga escala
    /// </summary>
    public class OptimizationManager : MonoBehaviour
    {
        private static OptimizationManager _instance;
        public static OptimizationManager Instance => _instance;

        [Header("Performance Settings")]
        [SerializeField] private bool enableBurstCompilation = true;
        [SerializeField] private bool enableJobSystem = true;
        [SerializeField] private int batchSize = 64;
        [SerializeField] private int targetFrameRate = 60;

        [Header("Collision Optimization")]
        [SerializeField] private bool optimizeCollisions = true;
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private float collisionCheckRadius = 100f;
        [SerializeField] private int maxCollisionChecksPerFrame = 100;

        [Header("Entity Management")]
        [SerializeField] private int maxActiveEntities = 500;
        [SerializeField] private float cullingDistance = 200f;

        [Header("Performance Monitoring")]
        [SerializeField] private bool showPerformanceStats = true;
        [SerializeField] private float updateStatsInterval = 0.5f;

        // Native Arrays para Jobs
        private NativeArray<float3> entityPositions;
        private NativeArray<float3> entityVelocities;
        private NativeArray<float> entitySpeeds;
        private NativeArray<bool> entityActive;

        // Transform Access Array para Jobs
        private TransformAccessArray entityTransforms;

        // Lists de entidades
        private List<Transform> managedEntities;
        private List<Rigidbody> managedRigidbodies;

        // Performance Stats
        private float currentFPS;
        private float lastStatsUpdate;
        private int frameCount;

        // Job Handles
        private JobHandle movementJobHandle;
        private JobHandle collisionJobHandle;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystem();
        }

        private void Start()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;

            InitializeNativeArrays();
        }

        private void InitializeSystem()
        {
            managedEntities = new List<Transform>();
            managedRigidbodies = new List<Rigidbody>();
        }

        private void InitializeNativeArrays()
        {
            int capacity = maxActiveEntities;

            entityPositions = new NativeArray<float3>(capacity, Allocator.Persistent);
            entityVelocities = new NativeArray<float3>(capacity, Allocator.Persistent);
            entitySpeeds = new NativeArray<float>(capacity, Allocator.Persistent);
            entityActive = new NativeArray<bool>(capacity, Allocator.Persistent);
        }

        private void Update()
        {
            UpdatePerformanceStats();

            if (enableJobSystem)
            {
                ScheduleJobs();
            }
        }

        private void LateUpdate()
        {
            if (enableJobSystem)
            {
                CompleteJobs();
            }
        }

        private void ScheduleJobs()
        {
            // Complete previous jobs
            movementJobHandle.Complete();
            collisionJobHandle.Complete();

            // Update entity data
            UpdateEntityData();

            // Schedule movement job
            if (enableBurstCompilation)
            {
                var movementJob = new EntityMovementJob
                {
                    positions = entityPositions,
                    velocities = entityVelocities,
                    speeds = entitySpeeds,
                    active = entityActive,
                    deltaTime = Time.deltaTime
                };

                movementJobHandle = movementJob.Schedule(entityPositions.Length, batchSize);
            }

            // Schedule collision detection job
            if (optimizeCollisions)
            {
                var collisionJob = new OptimizedCollisionJob
                {
                    positions = entityPositions,
                    active = entityActive,
                    checkRadius = collisionCheckRadius,
                    collisions = new NativeArray<int>(entityPositions.Length, Allocator.TempJob)
                };

                collisionJobHandle = collisionJob.Schedule(entityPositions.Length, batchSize, movementJobHandle);
            }
        }

        private void CompleteJobs()
        {
            movementJobHandle.Complete();
            collisionJobHandle.Complete();

            // Apply results back to transforms
            ApplyJobResults();
        }

        private void UpdateEntityData()
        {
            for (int i = 0; i < managedEntities.Count && i < entityPositions.Length; i++)
            {
                if (managedEntities[i] != null)
                {
                    entityPositions[i] = managedEntities[i].position;

                    if (managedRigidbodies[i] != null)
                    {
                        entityVelocities[i] = managedRigidbodies[i].velocity;
                        entitySpeeds[i] = managedRigidbodies[i].velocity.magnitude;
                    }

                    entityActive[i] = managedEntities[i].gameObject.activeInHierarchy;
                }
            }
        }

        private void ApplyJobResults()
        {
            for (int i = 0; i < managedEntities.Count && i < entityPositions.Length; i++)
            {
                if (managedEntities[i] != null && entityActive[i])
                {
                    // Aplicar novas posições (se não controladas por física)
                    if (managedRigidbodies[i] == null)
                    {
                        managedEntities[i].position = entityPositions[i];
                    }
                }
            }
        }

        public void RegisterEntity(Transform entity, Rigidbody rb = null)
        {
            if (managedEntities.Count >= maxActiveEntities)
            {
                Debug.LogWarning("Max entities reached!");
                return;
            }

            if (!managedEntities.Contains(entity))
            {
                managedEntities.Add(entity);
                managedRigidbodies.Add(rb);
            }
        }

        public void UnregisterEntity(Transform entity)
        {
            int index = managedEntities.IndexOf(entity);
            if (index >= 0)
            {
                managedEntities.RemoveAt(index);
                managedRigidbodies.RemoveAt(index);
            }
        }

        private void UpdatePerformanceStats()
        {
            if (!showPerformanceStats) return;

            frameCount++;
            float timeSinceLastUpdate = Time.time - lastStatsUpdate;

            if (timeSinceLastUpdate >= updateStatsInterval)
            {
                currentFPS = frameCount / timeSinceLastUpdate;
                frameCount = 0;
                lastStatsUpdate = Time.time;
            }
        }

        public void OptimizeCollider(Collider collider)
        {
            if (collider == null) return;

            // Simplificar collider se possível
            if (collider is MeshCollider meshCollider)
            {
                meshCollider.convex = true;
                meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation |
                                               MeshColliderCookingOptions.EnableMeshCleaning |
                                               MeshColliderCookingOptions.WeldColocatedVertices;
            }

            // Desabilitar queries desnecessárias
            collider.hasModifiableContacts = false;
        }

        public void OptimizeRigidbody(Rigidbody rb)
        {
            if (rb == null) return;

            // Otimizações de física
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.sleepThreshold = 0.5f;

            // Usar continuous para objetos rápidos
            if (rb.velocity.magnitude > 50f)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        public void SetPhysicsUpdateRate(int fixedTimestepHz)
        {
            Time.fixedDeltaTime = 1f / fixedTimestepHz;
        }

        public void EnableOcclusionCulling(bool enable)
        {
            Camera.main.useOcclusionCulling = enable;
        }

        public void OptimizeLighting()
        {
            // Usar baked lighting
            Light[] lights = FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    light.shadows = LightShadows.Soft;
                }
                else
                {
                    light.shadows = LightShadows.None;
                }
            }
        }

        public float GetCurrentFPS()
        {
            return currentFPS;
        }

        public int GetActiveEntityCount()
        {
            return managedEntities.Count;
        }

        public void ClearInactiveEntities()
        {
            managedEntities.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
            managedRigidbodies.RemoveAll(rb => rb == null);
        }

        private void OnDestroy()
        {
            // Dispose native arrays
            if (entityPositions.IsCreated) entityPositions.Dispose();
            if (entityVelocities.IsCreated) entityVelocities.Dispose();
            if (entitySpeeds.IsCreated) entitySpeeds.Dispose();
            if (entityActive.IsCreated) entityActive.Dispose();

            // Complete any pending jobs
            movementJobHandle.Complete();
            collisionJobHandle.Complete();
        }

        private void OnGUI()
        {
            if (!showPerformanceStats) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = currentFPS >= targetFrameRate ? Color.green : Color.red;

            GUI.Label(new Rect(10, 10, 200, 30), $"FPS: {currentFPS:F1}", style);
            GUI.Label(new Rect(10, 40, 200, 30), $"Entities: {managedEntities.Count}", style);
        }
    }

    // BURST COMPILED JOBS

    [BurstCompile]
    public struct EntityMovementJob : IJobParallelFor
    {
        public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<float3> velocities;
        [ReadOnly] public NativeArray<float> speeds;
        [ReadOnly] public NativeArray<bool> active;
        public float deltaTime;

        public void Execute(int index)
        {
            if (!active[index]) return;

            // Atualizar posição baseada na velocidade
            positions[index] += velocities[index] * deltaTime;
        }
    }

    [BurstCompile]
    public struct OptimizedCollisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<bool> active;
        public float checkRadius;
        [WriteOnly] public NativeArray<int> collisions;

        public void Execute(int index)
        {
            if (!active[index])
            {
                collisions[index] = 0;
                return;
            }

            int collisionCount = 0;
            float3 pos = positions[index];
            float radiusSq = checkRadius * checkRadius;

            // Check collisions with other entities
            for (int i = 0; i < positions.Length; i++)
            {
                if (i == index || !active[i]) continue;

                float3 diff = positions[i] - pos;
                float distSq = math.lengthsq(diff);

                if (distSq < radiusSq)
                {
                    collisionCount++;
                }
            }

            collisions[index] = collisionCount;
        }
    }

    [BurstCompile]
    public struct PhysicsSimulationJob : IJobParallelFor
    {
        public NativeArray<float3> positions;
        public NativeArray<float3> velocities;
        [ReadOnly] public NativeArray<float3> forces;
        [ReadOnly] public NativeArray<float> masses;
        [ReadOnly] public NativeArray<bool> active;
        public float deltaTime;
        public float drag;

        public void Execute(int index)
        {
            if (!active[index]) return;

            // F = ma -> a = F/m
            float3 acceleration = forces[index] / masses[index];

            // Update velocity
            velocities[index] += acceleration * deltaTime;

            // Apply drag
            velocities[index] *= (1f - drag * deltaTime);

            // Update position
            positions[index] += velocities[index] * deltaTime;
        }
    }
}
