using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

public class StressTests
{
	private const int MAX_TORCH_COUNT = 100_000;

	private readonly List<Torch> _torches = new(MAX_TORCH_COUNT);

	private IEnumerator SpawnAndLitTorch(int iterations) => LoadTests.SpawnAndLitTorch(_torches, iterations);

	[UnityTest]
	public IEnumerator SpawnAndLitTorch1K() => SpawnAndLitTorch(1_000);

	[UnityTest]
	public IEnumerator SpawnAndLitTorch10K() => SpawnAndLitTorch(10_000);

	[UnityTest]
	public IEnumerator SpawnAndLitTorch100K() => SpawnAndLitTorch(100_000);

}
