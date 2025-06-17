using UnityEngine;
using System.Collections;

public class PooledEffect : MonoBehaviour
{
    private ObjectPool pool;

    public void Init(ObjectPool pool, float lifetime)
    {
        this.pool = pool;
        StartCoroutine(ReturnToPoolAfterSeconds(lifetime));
    }

    private IEnumerator ReturnToPoolAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Return(gameObject);
    }
}
