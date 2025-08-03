using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.PerformanceTesting;

public class PerformanceTests
{
	private Torch _torch;

	
	[UnitySetUp]
	public IEnumerator SetUp()
	{
		yield return null;
		_torch = TorchTest.CreateTorch();
	}

	[UnityTearDown]
	public IEnumerator TearDown()
	{
		yield return null;
		Object.Destroy(_torch.gameObject);
	}

	[UnityTest, Performance]
	public IEnumerator LitTorch()
	{
		yield return null;
		Measure.Method(() => _torch.LightUp()).Run();
	}

	[UnityTest, Performance]
	public IEnumerator LitTorchX50()
	{
		yield return null;
		Measure.Method(() => _torch.LightUp()).WarmupCount(10).MeasurementCount(50).Run();
	}
}
