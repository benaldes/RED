using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TorchTest
{
   
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TorchTestWithEnumeratorPasses()
    {
        var gameObject = new GameObject();
        var objectT = gameObject.AddComponent<Torch>();
        objectT.LightUp();
        yield return new WaitForSeconds(0.1f);
        Assert.IsTrue(objectT.isLit);
    }
    
    [UnityTest]
    public IEnumerator TorchTestColorChanges()
    {
        var gameObject = new GameObject();
        var objectT = gameObject.AddComponent<Torch>();
        objectT.LightUp();
        yield return new WaitForSeconds(0.1f);
        Assert.IsTrue(objectT.myColor == Color.yellow);
    }
    
    [UnityTest]
    public IEnumerator PlayerInteractionIntegrationTest()
    {
        var gameObject = new GameObject();
        var objectT = gameObject.AddComponent<Torch>();
        gameObject.AddComponent<BoxCollider>();
        var player = new GameObject();
        var playerInteraction = player.AddComponent<PlayerTorchInteractor>();
        playerInteraction.LightTorch();
        yield return new WaitForSeconds(0.1f);
        Assert.IsTrue(objectT.myColor == Color.yellow);
    }
    
    [UnityTest]
    public IEnumerator TorchSmokeTest()
    {
        var gameObject = new GameObject();
        var objectT = gameObject.AddComponent<Torch>();
        gameObject.AddComponent<BoxCollider>();
        var player = new GameObject();
        var playerInteraction = player.AddComponent<PlayerTorchInteractor>();
        playerInteraction.LightTorch();
        yield return new WaitForSeconds(0.1f);
        Assert.IsTrue(player);
        Assert.IsTrue(objectT);
        Assert.IsTrue(playerInteraction);
        Assert.IsTrue(objectT);
    }
}
