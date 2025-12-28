using UnityEngine;

public class AutoDisable : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f;

    private void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(Disable), lifeTime);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
        transform.SetParent(null);
    }
}
