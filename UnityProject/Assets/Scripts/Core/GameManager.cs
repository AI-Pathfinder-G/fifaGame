using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoccerGame.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private readonly Dictionary<Type, IGameSystem> _systems = new Dictionary<Type, IGameSystem>();

        private IGameState _currentState;

        public GameStateType CurrentStateType { get; private set; }
        public IGameState CurrentState => _currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            RegisterSystems();
            InitializeSystems();

            ChangeState(GameStateType.Boot);
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            ShutdownSystems();
            GameEvents.ClearAll();
            Instance = null;
        }

        private void Update()
        {
            if (_currentState == null)
                return;

            _currentState.UpdateState();

            GameStateType nextState = _currentState.NextState();
            if (nextState != CurrentStateType)
                ChangeState(nextState);
        }

        protected virtual void RegisterSystems()
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IGameSystem system)
                    RegisterSystem(system);
            }
        }

        public void RegisterSystem<T>(T system) where T : class, IGameSystem
        {
            if (system == null)
                return;

            Type type = system.GetType();
            if (_systems.ContainsKey(type))
            {
                Debug.LogWarning($"[GameManager] System already registered: {type.Name}");
                return;
            }

            _systems.Add(type, system);
        }

        public T GetSystem<T>() where T : class, IGameSystem
        {
            if (_systems.TryGetValue(typeof(T), out IGameSystem system))
                return system as T;

            foreach (IGameSystem candidate in _systems.Values)
            {
                if (candidate is T match)
                    return match;
            }

            Debug.LogWarning($"[GameManager] System not found: {typeof(T).Name}");
            return null;
        }

        public void ChangeState(GameStateType newStateType)
        {
            _currentState?.Exit();

            CurrentStateType = newStateType;
            _currentState = StateFactory.CreateState(newStateType);

            _currentState.Enter();
        }

        public T GetCurrentState<T>() where T : class, IGameState
        {
            return _currentState as T;
        }

        private void InitializeSystems()
        {
            foreach (IGameSystem system in _systems.Values)
            {
                if (!system.IsInitialized)
                    system.Initialize();
            }
        }

        private void ShutdownSystems()
        {
            foreach (IGameSystem system in _systems.Values)
            {
                if (system.IsInitialized)
                    system.Shutdown();
            }

            _systems.Clear();
        }
    }
}
