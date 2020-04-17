using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
using Core.Health;
using Core.Utilities;
using TowerDefense.Agents;
using TowerDefense.Agents.Data;
using TowerDefense.Nodes;
using UnityEngine;

namespace TowerDefense.Level
{
	/// <summary>
	/// A Wave is a TimedBehaviour, that uses the RepeatingTimer to spawn enemies
	/// </summary>
	public class SimpleWave : Wave
	{
		bool all_spawned = false;
		int total_deaths = 0;

		public override float progress
		{
			get { return (float)(total_deaths) / spawnInstructions.Count; }
		}

		protected override void SpawnCurrent()
		{
			Spawn();
			if (!TrySetupNextSpawn())
			{
				all_spawned = true;
				// this is required so wave progress is still accurate
				m_CurrentIndex = spawnInstructions.Count;
				StopTimer(m_SpawnTimer);
			}
		}

		protected override Agent CreateAgent(Poolable pool)
		{
			var agent = pool.GetComponent<Agent>();
			agent.removed += OnAgentRemove;
			return agent;
		}

		void OnAgentRemove(DamageableBehaviour d)
		{
			total_deaths++;
			if(all_spawned && total_deaths == spawnInstructions.Count)
			{
				SafelyBroadcastWaveCompletedEvent();
			}
		}
	}
}