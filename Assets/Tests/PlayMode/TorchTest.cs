using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TorchTest
{
    private static readonly Shader TORCH_SHADER = Shader.Find("Universal Render Pipeline/Lit");
    private static readonly System.Type[] REQUIRED_COMPONENTS = { typeof(Torch), typeof(MeshRenderer) };
    
    public static Torch CreateTorch()
    {
        var go = new GameObject("Test Torch", REQUIRED_COMPONENTS);
        var torch = go.GetComponent<Torch>();
        torch.rend = go.GetComponent<MeshRenderer>();
        torch.rend.material = new(TORCH_SHADER);
        return torch;
    }
    
    /*// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
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
    }*/
}

