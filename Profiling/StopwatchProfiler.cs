	using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class StopwatchProfiler : System.IDisposable
{
	#region Static
	public static readonly Stack<StopwatchProfiler> ProfilerStack = new Stack<StopwatchProfiler>();

	/// <summary>
	/// Initializes static profiler stack with a sentry null, allows for peeking when stack is empty.
	/// </summary>
	static StopwatchProfiler()
	{
		ProfilerStack.Push(null);
	}

	#endregion Static

	#region Fields

	private Stopwatch stopWatch = new Stopwatch();

	private int nestingLevel = 0;

	private double childrenElapsedMilliseconds = 0f;

	private double lastElapsedMilliseconds = 0f;

	public StopwatchProfiler()
	{
		NumberOfCalls = 0;
	}

	#endregion Fields

	#region Properties

	public double ElapsedMilliseconds
	{
		get { return stopWatch.Elapsed.TotalMilliseconds; }
	}

	public double ElapsedMillisecondsSelf
	{
		get { return ElapsedMilliseconds - childrenElapsedMilliseconds; }
	}

	public int NumberOfCalls { get; private set; }

	#endregion Properties

	public void Start()
	{
		StopwatchProfiler lastProfiler = ProfilerStack.Peek();
		if (lastProfiler != this) ProfilerStack.Push(this);

		nestingLevel++;
		NumberOfCalls++;

		if (nestingLevel == 1) stopWatch.Start();
	}

	public void Stop()
	{
		if (nestingLevel == 1) 
		{
			stopWatch.Stop();

			StopwatchProfiler previousProfiler = ProfilerStack.Peek();
			if (previousProfiler == this) { ProfilerStack.Pop(); }

			previousProfiler = ProfilerStack.Peek();
			if (previousProfiler != null) previousProfiler.childrenElapsedMilliseconds += (ElapsedMilliseconds - lastElapsedMilliseconds);
			lastElapsedMilliseconds = ElapsedMilliseconds;
		}
		nestingLevel--;
	}

	public void ForceStop()
	{
		stopWatch.Stop();
		nestingLevel = 0;
	}

	public void Reset()
	{
		stopWatch.Reset();
		nestingLevel = 0;
		NumberOfCalls = 0;
		childrenElapsedMilliseconds = 0f;
		lastElapsedMilliseconds = 0f;
	}

	public void Dispose()
	{
		Stop();
	}
}
