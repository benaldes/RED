using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class LoadTests
{
	private const int MAX_TORCH_COUNT = 100;

	private readonly List<Torch> _torches = new(MAX_TORCH_COUNT);

	public static IEnumerator SpawnAndLitTorch(IList<Torch> torches, int iterations)
	{
		for (int i = 0; i < iterations; i++)
			torches.Add(TorchTest.CreateTorch());
		yield return null;
		foreach (var t in torches)
			t.LightUp();
		yield return null;
		foreach (var t in torches)
			Object.Destroy(t.gameObject);
		torches.Clear();
	}

	private IEnumerator SpawnAndLitTorch(int iterations) => SpawnAndLitTorch(_torches, iterations);

	[UnityTest]
	public IEnumerator SpawnAndLitTorch10() => SpawnAndLitTorch(10);

	[UnityTest]
	public IEnumerator SpawnAndLitTorch100() => SpawnAndLitTorch(MAX_TORCH_COUNT);

}
