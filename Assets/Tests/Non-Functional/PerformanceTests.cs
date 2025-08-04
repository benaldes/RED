using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.PerformanceTesting;

/// <summary>
/// Tests the performance impact of lighting up and destroying torches
/// </summary>
public class PerformanceTests
{
	private Torch _torch;

	/// <summary>
	/// Sets up the test environment.
	/// </summary>
	/// <returns>An IEnumerator containing the setup steps.</returns>
	[UnitySetUp]
	public IEnumerator SetUp()
	{
		yield return null;
		_torch = TorchTest.CreateTorch();
	}

	/// <summary>
	/// Tears down the test environment.
	/// </summary>
	/// <returns>An IEnumerator containing the teardown steps.</returns>
	[UnityTearDown]
	public IEnumerator TearDown()
	{
		yield return null;
		Object.Destroy(_torch.gameObject);
	}

	/// <summary>
	/// Tests the performance of lighting up a single torch.
	/// </summary>
	/// <returns>An IEnumerator containing the test steps.</returns>
	[UnityTest, Performance]
	public IEnumerator LitTorch()
	{
		yield return null;
		Measure.Method(() => _torch.LightUp()).Run();
	}

	/// <summary>
	/// Tests the performance of lighting up 50 torches.
	/// </summary>
	/// <returns>An IEnumerator containing the test steps.</returns>
	[UnityTest, Performance]
	public IEnumerator LitTorchX50()
	{
		yield return null;
		Measure.Method(() => _torch.LightUp()).WarmupCount(10).MeasurementCount(50).Run();
	}
}