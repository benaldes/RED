using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

/// <summary>
/// Stress tests for the Torch class, specifically for performance under heavy load.
/// </summary>
public class StressTests
{
	private const int STRESS_MAX_TORCH_CAPACITY = 500_000;

	private readonly List<Torch> _torches = new(STRESS_MAX_TORCH_CAPACITY);

	/// <summary>
	/// Spawns and lights up to 5,000 torches.
	/// </summary>
	[UnityTest]
	public IEnumerator SpawnAndLight5kTorches() => SpawnAndLightSingleTorch(5_000);

	/// <summary>
	/// Spawns and lights up to 50,000 torches.
	/// </summary>
	[UnityTest]
	public IEnumerator SpawnAndLight50kTorches() => SpawnAndLightSingleTorch(50_000);

	/// <summary>
	/// Spawns and lights up to 500,000 torches.
	/// </summary>
	[UnityTest]
	public IEnumerator SpawnAndLight500kTorches() => SpawnAndLightSingleTorch(500_000);

	/// <summary>
	/// Spawns and lights a specified number of torches.
	/// </summary>
	/// <param name="iterations">The number of iterations to perform.</param>
	/// <returns>A coroutine to be executed by Unity.</returns>
	private IEnumerator SpawnAndLightSingleTorch(int iterations) => LoadTests.SpawnAndLightTorch(_torches, iterations);

}