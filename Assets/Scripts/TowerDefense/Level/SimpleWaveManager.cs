using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Core.Extensions;
using UnityEngine;

namespace TowerDefense.Level
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
        public List<Wave> definedWaves = new List<Wave>();

        public override List<Wave> waves => definedWaves;
        public override int totalWaves => definedWaves.Count;
    }
}