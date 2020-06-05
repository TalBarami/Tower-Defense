using System;
using System.Collections.Generic;
using Core.Extensions;
using TowerDefense.Agents.Data;
using TowerDefense.Nodes;
using UnityEngine;

namespace TowerDefense.Level
{
	/// <summary>
	/// SimpleWaveManager - handles wave initialisation and completion
	/// </summary>
	public class SmartWaveManager : WaveManager
    {
        public List<Node> startingNodes; // v1's of each path.
        public List<AgentConfiguration> agents; // 0-buggy, 1-copter, 2-tank, 3-boss, 4-5-6-7-supers.

        protected List<Wave> generatedWaves = new List<Wave>();

        public override List<Wave> waves => generatedWaves;
        public override int totalWaves => 10;

        /// <summary>
		/// Starts the waves
		/// </summary>
		public override void StartWaves()
		{
            waves.Add(InitSimpleWave());
			if (waves.Count > 0)
			{
				Debug.Log("Initializing current wave");
				InitCurrentWave();
			}
			else
			{
				Debug.LogWarning("[LEVEL] No Waves on wave manager. Calling spawningCompleted");
				SafelyCallSpawningCompleted();
			}
		}

		/// <summary>
		/// Sets up the next wave
		/// </summary>
		protected override void NextWave()
		{
            waves.Add(InitSimpleWave());
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

        protected virtual SimpleWave InitSimpleWave()
        {
            GameObject newWave = new GameObject($"Wave {m_CurrentIndex}");
			newWave.transform.parent = gameObject.transform;

            SimpleWave w = newWave.AddComponent<SimpleWave>();
            Instantiate(w, new Vector3(0, 0, 0), Quaternion.identity);
            List<SpawnInstruction> s = new List<SpawnInstruction>();
            w.spawnInstructions = s;
            for (var i = 0; i < 6; i++)
            {
                SpawnInstruction si = new SpawnInstruction
                {
                    agentConfiguration = agents[m_CurrentIndex],
                    delayToSpawn = i == 0 ? 0 : 2,
                    startingNode = startingNodes[0]
                };
				s.Add(si);
            }

            return w;
        }
	}
}