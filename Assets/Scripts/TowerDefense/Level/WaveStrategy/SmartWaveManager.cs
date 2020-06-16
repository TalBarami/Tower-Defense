using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
using TowerDefense.Agents.Data;
using TowerDefense.Level;
using TowerDefense.Nodes;
using UnityEngine;
using UnityEngine.AI;
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

        public List<Node> StartingNodes; // 0-Long, 1-Short, 2-TopLeft, 3-BotLeft, 4-TopRight, 5-BotRight
        public List<AgentConfiguration> AgentConfigurations; // 0-buggy, 1-copter, 2-tank, 3-boss, 4-5-6-7-supers.

        protected List<Configuration> Agents;
        protected List<Strategy> Strategies;
        protected List<Strategy> BossFights;
        protected Random Random;

        protected List<Wave> EliminatedWaves = new List<Wave>();


        /// <summary>
        /// The waves to run in order
        /// </summary>
        [Tooltip("Specify this list in order")]
        public List<Wave> Waves = new List<Wave>();

        public override List<Wave> waves => Waves;
        public override int totalWaves => waves.Count;
        
        protected int PhaseLength;

        public override void StartWaves()
        {
            Random = new Random();
            notifier = StrategyNotifierObject.GetComponent<StrategyGUI>();
            PhaseLength = waves.Count;
            Agents = new List<Configuration>()
            {
                new Configuration(AgentConfigurations[0], 1),   // Buggy
                new Configuration(AgentConfigurations[1], 2),   // Copter
                new Configuration(AgentConfigurations[2], 3),   // Tank
                new Configuration(AgentConfigurations[3], 25),  // Boss
                new Configuration(AgentConfigurations[4], 2),   // Super Buggy
                new Configuration(AgentConfigurations[5], 4),   // Super Copter
                new Configuration(AgentConfigurations[6], 6),   // Super Tank
                new Configuration(AgentConfigurations[7], 50),  // Super Boss
            };

            Strategies = new List<Strategy>()
            {
                new Strategy("Frontal Assault",
                    Tuple.Create(0, 3),
                    new List<Node>(){ StartingNodes[0]},
                    new List<Configuration>() {Agents[0], Agents[1], Agents[2], Agents[4], Agents[5], Agents[6]}),
                new Strategy("Flanks Assault",
                    Tuple.Create(0, 3),
                    new List<Node>(){ StartingNodes[2], StartingNodes[4], StartingNodes[5]},
                    new List<Configuration>() {Agents[0], Agents[1], Agents[2], Agents[4], Agents[5], Agents[6]}),
                new Strategy("Sneak Assault",
                    Tuple.Create(1, 3),
                    new List<Node>(){ StartingNodes[3]},
                    new List<Configuration>() {Agents[0], Agents[1], Agents[4], Agents[5]}),

                new Strategy("Quick Assault",
                    Tuple.Create(0, 2),
                    new List<Node>(StartingNodes),
                    new List<Configuration>() {Agents[0], Agents[1], Agents[4], Agents[5]}),
                new Strategy("Brutal Assault",
                    Tuple.Create(1, 4),
                    new List<Node>(StartingNodes),
                    new List<Configuration>() {Agents[2], Agents[6]}),
                new Strategy("Air Assault",
                    Tuple.Create(1, 3),
                    new List<Node>(StartingNodes),
                    new List<Configuration>() {Agents[1], Agents[5]}),
            };

            BossFights = new List<Strategy>()
            {
                new Strategy("Boss Fight",
                    Tuple.Create(5, 5),
                    new List<Node>() {StartingNodes[0]},
                    new List<Configuration>() {Agents[3]}),
                new Strategy("Super Boss Fight",
                    Tuple.Create(5, 5),
                    new List<Node>() {StartingNodes[0]},
                    new List<Configuration>() {Agents[7]}),
                new Strategy("Ultimate Boss Fight",
                    Tuple.Create(5, 5),
                    new List<Node>() {StartingNodes[0]},
                    new List<Configuration>() {Agents[3], Agents[7]}),
            };

            base.StartWaves();
        }


        /// <summary>
		/// Sets up the next wave
		/// </summary>
		protected override void NextWave()
        {
            if (m_CurrentIndex + 1 == PhaseLength)
            {
                EliminatedWaves.AddRange(waves);
                waves[m_CurrentIndex].waveCompleted -= NextWave;
                waves.Clear();
                m_CurrentIndex = 0;

                ChooseStrategy(EliminatedWaves.Count);

                InitCurrentWave();
            }
            else
            {
                base.NextWave();
            }
        }

        protected void ChooseStrategy(int waveNum)
        {
            var strategies = waveNum % 10 == 0 ? BossFights : Strategies;
            var phaseLength = waveNum % 10 == 0 ? 1 : PhaseLength;

            var possibleStrategies = strategies.FindAll(s => s.CanSet(waveNum));
            Debug.Log($"Possible Strategies: {possibleStrategies}");
            var strategy = possibleStrategies[Random.Next(possibleStrategies.Count)];

            Debug.Log($"Changed Strategy to {strategy}");
            notifier.Set(strategy.Name);

            for (var i = 0; i < phaseLength; i++)
            {
                waves.Add(GenerateWave(strategy, waveNum + i));
            }
        }

        protected virtual Wave GenerateWave(Strategy strategy, int waveIndex)
        {
            var waveObject = new GameObject($"Wave {m_CurrentIndex + 1}");
			waveObject.transform.parent = gameObject.transform;

            var simpleWave = waveObject.AddComponent<SimpleWave>();
            Instantiate(simpleWave, new Vector3(0, 0, 0), Quaternion.identity);

            var spawnInstructions = strategy.ProduceEnemies(waveIndex);

            simpleWave.spawnInstructions = spawnInstructions;

            return simpleWave;
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
            public static Random Random = new Random();
            public string Name { get; }
            protected Tuple<int, int> DelayRange;
            protected List<Node> Nodes;
            protected List<Configuration> Enemies;

            public Strategy(string name, Tuple<int, int> delayRange, List<Node> nodes, List<Configuration> enemies)
            {
                Name = name;
                DelayRange = delayRange;
                Nodes = nodes;
                Enemies = enemies;
            }

            public virtual bool CanSet(int waveIndex)
            {
                return Enemies.Any(x => x.Price <= waveIndex);
            }

            public virtual List<SpawnInstruction> ProduceEnemies(int waveIndex)
            {
                var sum = 0;
                var result = new List<SpawnInstruction>();

                while (sum < waveIndex)
                {
                    var possibleConfigurations = Enemies.FindAll(x => x.Price <= waveIndex - sum);
                    if (possibleConfigurations.Count == 0)
                    {
                        if (result.Count == 0)
                        {
                            Debug.Log($"Critical Error: No enemies found at all for wave {waveIndex} with strategy {Name}");
                        }
                        break;
                    }
                    var selected = possibleConfigurations[Random.Next(possibleConfigurations.Count)];
                    sum += selected.Price;

                    var delay = Random.NextDouble() * (DelayRange.Item2 - DelayRange.Item1) + DelayRange.Item1;
                    var node = Nodes[Random.Next(Nodes.Count)];

                    result.Add(new SpawnInstruction(selected.AgentConfiguration, delay, node));
                }

                return result;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        class BossFight : Strategy
        {
            protected Tuple<int, int> BossDelayRange;
            protected List<Node> BossNodes;
            protected readonly List<Configuration> Bosses;


            public override List<SpawnInstruction> ProduceEnemies(int waveIndex)
            {
                var result = new List<SpawnInstruction>();
                var sum = 0;
                while (sum < waveIndex)
                {
                    var possibleConfigurations = Bosses.FindAll(x => x.Price <= waveIndex - sum);
                    if (possibleConfigurations.Count == 0)
                    {
                        if (result.Count == 0)
                        {
                            Debug.Log($"Critical Error: No enemies found at all for wave {waveIndex} with strategy {Name}");
                        }
                        break;
                    }
                    var selected = possibleConfigurations[Random.Next(possibleConfigurations.Count)];
                    sum += selected.Price;

                    var delay = Random.NextDouble() * (DelayRange.Item2 - DelayRange.Item1) + DelayRange.Item1;
                    var node = Nodes[Random.Next(Nodes.Count)];

                    result.Add(new SpawnInstruction(selected.AgentConfiguration, delay, node));
                }

                var trash = base.ProduceEnemies(waveIndex / 5);
                result.AddRange(trash);
                return result;
            }

            public override bool CanSet(int waveIndex)
            {
                return Bosses.Any(x => x.Price <= waveIndex);
            }

            public BossFight(string name, Tuple<int, int> delayRange, List<Node> nodes, List<Configuration> enemies, Tuple<int, int> bossDelayRange, List<Node> bossNodes, List<Configuration> bosses)
                : base(name, delayRange, nodes, enemies)
            {
                this.BossDelayRange = bossDelayRange;
                this.BossNodes = bossNodes;
                this.Bosses = bosses;

            }
        }
    }
}