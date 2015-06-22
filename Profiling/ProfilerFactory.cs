using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Usage:
/// using(ProfilerFactory.GetProfiler("profilerName"))
/// {
///		ExpensiveMethod();
///		// more code that you want to profile quickly
/// }
/// 
/// 
/// Add a call to ProfilerFactory.DumpProfilerInfo(); to log all measured data.
/// </summary>
public static class ProfilerFactory
{
	#region Constants

	private const string LogFormat = "Profiler {0} - Total: {1}ms | Self - {2}ms | Number of calls - {3}";

	#endregion

	#region Static

	private static Dictionary<string, StopwatchProfiler> profilers = new Dictionary<string, StopwatchProfiler>();

	public static readonly bool DoNotProfile = false;

	#endregion Static

	#region Methods

	public static StopwatchProfiler GetProfiler(string name)
	{
		if (DoNotProfile) return null;

		StopwatchProfiler profiler;
		profilers.TryGetValue(name, out profiler);

		if (profiler == null)
		{
			profiler = new StopwatchProfiler();
			profilers.Add(name, profiler);
		}

		return profiler;
	}

	public static StopwatchProfiler GetAndStartProfiler(object name)
	{
		if (DoNotProfile) return null;
		StopwatchProfiler profiler = GetProfiler(name.ToString());
		profiler.Start();
		return profiler;
	}

	public static void DumpProfilerInfo(float timeThresholdToLog = 0f, bool resetProfilers = true)
	{
		foreach (var pair in profilers)
		{
			pair.Value.ForceStop();
		}

		var list = new List<KeyValuePair<string, StopwatchProfiler>>();

		foreach (var pair in profilers)
		{
			list.Add(pair);
		}

		list.Sort(delegate(KeyValuePair<string, StopwatchProfiler> a, KeyValuePair<string, StopwatchProfiler> b) { return a.Value.ElapsedMilliseconds.CompareTo(b.Value.ElapsedMilliseconds); });

		double fullTime = 0;
		foreach (var pair in list)
		{
			double time = pair.Value.ElapsedMillisecondsSelf;
			fullTime += time;
			if (pair.Value.NumberOfCalls > 0 && time > timeThresholdToLog) 
			{
				Debug.Log(string.Format(LogFormat, pair.Key, pair.Value.ElapsedMilliseconds, pair.Value.ElapsedMillisecondsSelf.ToString("0.00"), pair.Value.NumberOfCalls));
			}
			if (resetProfilers) pair.Value.Reset();
		}

		if (fullTime > timeThresholdToLog) { UnityEngine.Debug.Log("Full stopwatch measured(ms): " + fullTime); }
	}

	#endregion Methods
}
