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
        private bool _allSpawned;
        private int _totalDeaths;

		public override void Init()
        {
            base.Init();
            _allSpawned = false;
            _totalDeaths = 0;
		}

        public override float progress => (float)(_totalDeaths) / spawnInstructions.Count;

        protected override void SpawnCurrent()
		{
			Spawn();
			if (!TrySetupNextSpawn())
			{
				_allSpawned = true;
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

        protected void OnAgentRemove(DamageableBehaviour d)
		{
			_totalDeaths++;
			if(_allSpawned && _totalDeaths == spawnInstructions.Count)
			{
				SafelyBroadcastWaveCompletedEvent();
			}
		}
	}
}