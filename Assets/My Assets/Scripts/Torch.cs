using UnityEngine;

public class Torch : MonoBehaviour
{
    public bool isLit = false;
    public Renderer rend;
    public Color myColor = Color.red;

    void Start()
    {
        rend = GetComponent<Renderer>();
        isLit = false;
        UpdateVisual();
    }

    [ContextMenu("lit")]
    public void LightUp()
    {
        isLit = !isLit;
        UpdateVisual();
    }

    void UpdateVisual()
    {
            myColor = isLit ? Color.yellow : Color.red;
            rend.material.color = myColor;
    }
}
