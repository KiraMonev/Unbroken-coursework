using UnityEngine;

public class DiamondCollectible : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ScoreManager.Instance.AddDiamond();
            Destroy(gameObject);
        }
    }
}