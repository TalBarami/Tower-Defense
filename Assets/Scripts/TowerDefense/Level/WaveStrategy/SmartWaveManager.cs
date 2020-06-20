﻿using System;
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
        protected int BossCycle = 10;

        public override void StartWaves()
        {
            Random = new Random();
            notifier = StrategyNotifierObject.GetComponent<StrategyGUI>();
            PhaseLength = waves.Count;
            Agents = new List<Configuration>()
            {
                new Configuration(AgentConfigurations[0], 1), // Buggy
                new Configuration(AgentConfigurations[1], 2), // Copter
                new Configuration(AgentConfigurations[2], 3), // Tank
                new Configuration(AgentConfigurations[3], BossCycle), // Boss
                new Configuration(AgentConfigurations[4], 2), // Super Buggy
                new Configuration(AgentConfigurations[5], 4), // Super Copter
                new Configuration(AgentConfigurations[6], 6), // Super Tank
                new Configuration(AgentConfigurations[7], 3 * BossCycle), // Super Boss
            };

            Strategies = new List<Strategy>()
            {
                new Strategy("Frontal Assault",
                    Tuple.Create(0, 3),
                    new List<Node>() {StartingNodes[0]},
                    new List<Configuration>() {Agents[0], Agents[1], Agents[2], Agents[4], Agents[5], Agents[6]}),
                new Strategy("Flanks Assault",
                    Tuple.Create(0, 3),
                    new List<Node>() {StartingNodes[2], StartingNodes[4], StartingNodes[5]},
                    new List<Configuration>() {Agents[0], Agents[1], Agents[2], Agents[4], Agents[5], Agents[6]}),
                new Strategy("Sneak Assault",
                    Tuple.Create(1, 3),
                    new List<Node>() {StartingNodes[3]},
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
                new BossFight("Boss Fight",
                    Tuple.Create(0, 3),
                    StartingNodes,
                    new List<Configuration>() {Agents[0], Agents[1], Agents[2], Agents[4], Agents[5], Agents[6]},
                    Tuple.Create(5, 5),
                    new List<Node>() {StartingNodes[0]},
                    new List<Configuration>() {Agents[3]}),
                new BossFight("Double Boss Fight",
                    Tuple.Create(0, 3),
                    StartingNodes,
                    new List<Configuration>() {Agents[0], Agents[1], Agents[2], Agents[4], Agents[5], Agents[6]},
                    Tuple.Create(5, 5),
                    new List<Node>() {StartingNodes[0]},
                    new List<Configuration>() {Agents[3]}),
                new BossFight("Super Boss Fight",
                    Tuple.Create(0, 3),
                    StartingNodes,
                    new List<Configuration>() {Agents[0], Agents[1], Agents[2], Agents[4], Agents[5], Agents[6]},
                    Tuple.Create(5, 5),
                    new List<Node>() {StartingNodes[0]},
                    new List<Configuration>() {Agents[7]}),
                new BossFight("Ultimate Boss Fight",
                    Tuple.Create(0, 3),
                    StartingNodes,
                    new List<Configuration>() {Agents[0], Agents[1], Agents[2], Agents[4], Agents[5], Agents[6]},
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
                Debug.Log("Changing Strategy");
                ChangeStrategy(EliminatedWaves.Count);

                InitCurrentWave();
            }
            else
            {
                base.NextWave();
            }
        }

        protected Strategy ChooseStrategy(List<Strategy> possibleStrategies)
        {
            foreach (var s in possibleStrategies)
            {
                Debug.Log($"Possible Strategy: {s} with score {s.Score}");
            }
            var sumOfScores = possibleStrategies.Sum(s => s.Score);
            var roll = Random.Next(sumOfScores);
            var acc = 0;
            foreach (var s in possibleStrategies)
            {
                acc += s.Score;
                if (roll < acc)
                {
                    return s;
                }
            }
            Debug.Log($"Rolled {roll}, acc {acc}, sum of scores {sumOfScores}");

            throw new Exception("Possible strategies is empty");
        }

        protected void ChangeStrategy(int waveNum)
        {
            Strategy strategy;
            if (waveNum % BossCycle == 0) // Boss Fight
            {
                var selectionIdx = Math.Min(waveNum / BossCycle - 1, BossFights.Count-1);
                strategy = BossFights[selectionIdx];

                Debug.Log($"Generating for {strategy}");
                waves.Add(GenerateWave(strategy, waveNum));
                for (var i = 1; i < PhaseLength; i++)
                {
                    var randomStrategy = Strategies[Random.Next(Strategies.Count)];
                    waves.Add(GenerateWave(randomStrategy, waveNum + i));
                }
            }
            else
            {
                var possibleStrategies = Strategies.FindAll(s => s.CanSet(waveNum));
                strategy = ChooseStrategy(possibleStrategies);
                for (var i = 0; i < PhaseLength; i++)
                {
                    waves.Add(GenerateWave(strategy, waveNum + i));
                }
            }

            Debug.Log($"Changed Strategy to {strategy}");
            notifier.Set(strategy.Name);
        }

        protected virtual Wave GenerateWave(Strategy strategy, int waveIndex)
        {
            Debug.Log("");
            var waveObject = new GameObject($"Wave {m_CurrentIndex + 1}");
			waveObject.transform.parent = gameObject.transform;

            var simpleWave = waveObject.AddComponent<SimpleWave>();
            Instantiate(simpleWave, new Vector3(0, 0, 0), Quaternion.identity);

            var spawnInstructions = strategy.ProduceEnemies(waveIndex);

            simpleWave.spawnInstructions = spawnInstructions;

            simpleWave.destinationReached += () =>
            {
                strategy.Score += 1;
                Debug.Log($"Destination reached for {strategy}, new score: {strategy.Score}");
            };

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
            public int Score { get; set; }

            protected Tuple<int, int> DelayRange;
            protected List<Node> Nodes;
            protected List<Configuration> Enemies;

            public Strategy(string name, Tuple<int, int> delayRange, List<Node> nodes, List<Configuration> enemies)
            {
                Name = name;
                DelayRange = delayRange;
                Nodes = nodes;
                Enemies = enemies;
                Score = 1;
            }

            public virtual bool CanSet(int waveIndex)
            {
                return Enemies.Any(x => x.Price <= waveIndex);
            }

            public virtual List<SpawnInstruction> ProduceEnemies(int waveIndex)
            {
                Debug.Log("Normal Produce Enemies");
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
                Debug.Log("Boss Produce Enemies");
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

                    var delay = Random.NextDouble() * (BossDelayRange.Item2 - BossDelayRange.Item1) + BossDelayRange.Item1;
                    var node = BossNodes[Random.Next(BossNodes.Count)];

                    result.Add(new SpawnInstruction(selected.AgentConfiguration, delay, node));
                    Debug.Log("Boss added");
                }

                var trash = base.ProduceEnemies(waveIndex / 2);
                result.AddRange(trash);
                Debug.Log($"Trash added: {trash.Count}");
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