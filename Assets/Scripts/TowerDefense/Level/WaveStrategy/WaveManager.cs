using System;
using System.Collections.Generic;
using Core.Extensions;
using TowerDefense.Level;
using UnityEngine;

namespace Assets.Scripts.TowerDefense.Level.WaveStrategy
{
    public abstract class WaveManager : MonoBehaviour
    {
        /// <summary>
        /// Current wave being used
        /// </summary>
        protected int m_CurrentIndex;

        /// <summary>
        /// Whether the SimpleWaveManager starts waves on Awake - defaulted to null since the LevelManager should call this function
        /// </summary>
        public bool startWavesOnAwake;

        /// <summary>
        /// The waves to run in order
        /// </summary>
        public abstract List<Wave> waves { get; }

        /// <summary>
        /// The current wave number
        /// </summary>
        public int waveNumber => m_CurrentIndex + 1;

        /// <summary>
        /// The total number of waves
        /// </summary>
        public abstract int totalWaves { get; }

        public int score { get; set; }

        public float waveProgress
        {
            get
            {
                if (waves == null || waves.Count <= m_CurrentIndex)
                {
                    return 0;
                }
                return waves[m_CurrentIndex].progress;
            }
        }

        /// <summary>
        /// Called when a wave begins
        /// </summary>
        public event Action waveChanged;

        /// <summary>
        /// Called when all waves are finished
        /// </summary>
        public event Action spawningCompleted;

        /// <summary>
        /// Starts the waves
        /// </summary>
        public virtual void StartWaves()
        {
            if (waves.Count > 0)
            {
                InitCurrentWave();
            }
            else
            {
                Debug.LogWarning("[LEVEL] No Waves on wave manager. Calling spawningCompleted");
                SafelyCallSpawningCompleted();
            }
        }

        /// <summary>
        /// Inits the first wave
        /// </summary>
        protected virtual void Awake()
        {
            if (startWavesOnAwake)
            {
                StartWaves();
            }
        }

        /// <summary>
        /// Sets up the next wave
        /// </summary>
        protected virtual void NextWave()
        {
            waves[m_CurrentIndex].waveCompleted -= NextWave;
            if (waves.Next(ref m_CurrentIndex))
            {
                InitCurrentWave();
            }
            else
            {
                SafelyCallSpawningCompleted();
            }
        }

        /// <summary>
        /// Initialize the current wave
        /// </summary>
        protected virtual void InitCurrentWave()
        {
            var wave = waves[m_CurrentIndex];
            wave.waveCompleted += NextWave;
            wave.Init();
            waveChanged?.Invoke();
        }

        /// <summary>
        /// Calls spawningCompleted event
        /// </summary>
        protected virtual void SafelyCallSpawningCompleted()
        {
            spawningCompleted?.Invoke();
        }
    }
}
