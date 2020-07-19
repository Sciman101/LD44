using UnityEngine;
using UnityEngine.Events;

public class ReadyRegion : MonoBehaviour
{
    public UnityEvent onPlayerEnter;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            onPlayerEnter.Invoke();
        }
    }
}
