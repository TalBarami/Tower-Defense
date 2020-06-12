using System.Collections.Generic;
using TowerDefense.Level;
using UnityEngine;

namespace Assets.Scripts.TowerDefense.Level.WaveStrategy
{
	/// <summary>
	/// SimpleWaveManager - handles wave initialisation and completion
	/// </summary>
	public class SimpleWaveManager : WaveManager
	{
        /// <summary>
        /// The waves to run in order
        /// </summary>
        [Tooltip("Specify this list in order")]
        public List<Wave> DefinedWaves = new List<Wave>();

        public override List<Wave> waves => DefinedWaves;
        public override int totalWaves => DefinedWaves.Count;
    }
}