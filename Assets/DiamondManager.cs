using UnityEngine;

public class DiamondManager : MonoBehaviour
{
    public static DiamondManager instance;

    public int crystalCount = 0;

    void Awake()
    {
        // Синглтон
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AddCrystal()
    {
        crystalCount++;
    }
}
