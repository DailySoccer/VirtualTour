using UnityEngine;
using System.Collections.Generic;
using UMA;
using System.Threading;
using System;

namespace UMA.PowerTools
{
	public class UMAGeneratorThreaded : UMAGenerator
	{
		public bool logSpikes;
		PoliteThreadAbortHandler abortHandler;
		public bool runSingleThreaded;

		public void Awake()
		{
			Debug.LogWarning("UMAGeneratorThreaded for uma2 is delayed, falling back to the default generator.");
			base.Awake();
		}

		public int ticks = 100000;
		public long threadedTimeUse;

		internal void Work()
		{
			throw new NotImplementedException("UMAGeneratorThreaded for uma2 is delayed, falling back to the default generator.");
		}
	}
}
