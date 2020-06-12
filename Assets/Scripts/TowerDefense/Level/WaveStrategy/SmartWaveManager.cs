using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
using TowerDefense.Agents.Data;
using TowerDefense.Level;
using TowerDefense.Nodes;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.TowerDefense.Level.WaveStrategy
{
	/// <summary>
	/// SimpleWaveManager - handles wave initialization and completion
	/// </summary>
	public class SmartWaveManager : WaveManager
    {
        public GameObject StrategyNotifierObject;
        private StrategyGUI notifier;

        public List<Node> StartingNodes; // v1's of each path.
        public List<AgentConfiguration> AgentConfigurations; // 0-buggy, 1-copter, 2-tank, 3-boss, 4-5-6-7-supers.

        protected Dictionary<Node, int> NodeInvestments;

        protected List<Configuration> Agents;
        protected List<Strategy> Strategies;
        protected Random Random;

        protected List<Wave> EliminatedWaves = new List<Wave>();


        /// <summary>
        /// The waves to run in order
        /// </summary>
        [Tooltip("Specify this list in order")]
        public List<Wave> Waves = new List<Wave>();

        public override List<Wave> waves => Waves;
        public override int totalWaves => waves.Count;
        private int wavesCount;


        public override void StartWaves()
        {
            Random = new Random();
            notifier = StrategyNotifierObject.GetComponent<StrategyGUI>();
            wavesCount = waves.Count;
            Agents = new List<Configuration>()
            {
                new Configuration(AgentConfigurations[0], 1),
                new Configuration(AgentConfigurations[1], 2),
                new Configuration(AgentConfigurations[2], 4),
                new Configuration(AgentConfigurations[3], 8),
                new Configuration(AgentConfigurations[4], 3),
                new Configuration(AgentConfigurations[5], 5),
                new Configuration(AgentConfigurations[6], 7),
                new Configuration(AgentConfigurations[7], 14),
            };

            Strategies = new List<Strategy>()
            {
                new Strategy("Frontal Assault", new List<Node>(){ StartingNodes[0], StartingNodes[1]}),
                new Strategy("Flanks Assault", new List<Node>(){ StartingNodes[2], StartingNodes[3], StartingNodes[4], StartingNodes[5]}),
                new Strategy("Brutal Assault", new List<Node>(StartingNodes)),
            };

            notifier.Set($"Hello World {Random.Next()}");

            base.StartWaves();
        }

        private bool explored;
        /// <summary>
		/// Sets up the next wave
		/// </summary>
		protected override void NextWave()
		{
            if (m_CurrentIndex + 1 == wavesCount)
            {
                explored = true;
            }

            if (explored)
            {
                notifier.Set($"Hello World {Random.Next()}");
                EliminatedWaves.AddRange(waves);
                waves[m_CurrentIndex].waveCompleted -= NextWave;

                waves.Clear();
                m_CurrentIndex = 0;
                for (var i = 0; i < wavesCount; i++)
                {
                    waves.Add(GenerateWave());
                }

                InitCurrentWave();
            }
            else
            {
                base.NextWave();
            }
        }

        protected virtual Wave GenerateWave()
        {
            var newWave = new GameObject($"Wave {m_CurrentIndex + 1}");
			newWave.transform.parent = gameObject.transform;

            var w = newWave.AddComponent<SimpleWave>();
            Instantiate(w, new Vector3(0, 0, 0), Quaternion.identity);
            var s = new List<SpawnInstruction>();
            w.spawnInstructions = s;
            for (var i = 0; i < 6; i++)
            {
                s.Add(new SpawnInstruction(AgentConfigurations[m_CurrentIndex + 1], i == 0 ? 0 : 2, StartingNodes[0]));
            }

            return w;
        }

        protected List<AgentConfiguration> produceEnemies(int waveIndex)
        {
            var sum = 0;
            var result = new List<AgentConfiguration>();
            while (sum != waveIndex)
            {
                var possibleConfigurations = Agents.FindAll(x => x.Price < waveIndex-sum);
                var selected= possibleConfigurations[Random.Next(possibleConfigurations.Count)];
                sum += selected.Price;
                result.Add(selected.AgentConfiguration);
            }

            return result;
        }

        protected class Configuration
        {
            public AgentConfiguration AgentConfiguration;
            public int Price;

            public Configuration(AgentConfiguration agentConfiguration, int price)
            {
                AgentConfiguration = agentConfiguration;
                Price = price;
            }
        }

        protected class Strategy
        {
            public string Name;
            public List<Node> Nodes;
            public double Probability;

            public Strategy(string name, List<Node> nodes)
            {
                Name = name;
                Nodes = nodes;
                Probability = 1;
            }

        }
	}
}