using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Performance tests for torches.
/// </summary>
public class LoadTests
{
	private const int MAX_TORCH_CAPACITY = 500;

	private readonly List<Torch> _testTorches = new(MAX_TORCH_CAPACITY);

	/// <summary>
	/// Spawns and lights up to 50 torches.
	/// </summary>
	[UnityTest]
	public IEnumerator SpawnAndLightTorch50() => SpawnAndLightTorch(_testTorches, 50);

	/// <summary>
	/// Spawns and lights up to 500 torches.
	/// </summary>
	[UnityTest]
	public IEnumerator SpawnAndLightTorchMaxCapacity() => SpawnAndLightTorch(_testTorches, MAX_TORCH_CAPACITY);

	/// <summary>
	/// Spawns and lights the specified number of torches.
	/// </summary>
	/// <param name="torchs">The list of torches to spawn and light.</param>
	/// <param name="iterations">The number of iterations to perform.</param>
	/// <returns>A coroutine to be executed by Unity.</returns>
	public static IEnumerator SpawnAndLightTorch(IList<Torch> torches, int iterations)
	{
		//fill the list with an [iterations] amount of torches
		for (int i = 0; i < iterations; i++)
			torches.Add(TorchTest.CreateTorch());
		yield return null;
		//light them up
		foreach (var t in torches)
			t.LightUp();
		yield return null;
		//destroy them in the end and clear the list
		foreach (var t in torches)
			Object.Destroy(t.gameObject);
		torches.Clear();
	}

}