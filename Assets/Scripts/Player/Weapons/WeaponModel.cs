using System.Collections;
using UnityEngine;

public class WeaponModel : MonoBehaviour
{
    [Section("Dissolve Settings")]
    [SerializeField] private float dissolveDuration = 1f;

    [SerializeField] private MeshRenderer renderers;
    [SerializeField] private MaterialPropertyBlock propertyBlock;
    [SerializeField] private Coroutine dissolveCoroutine;

    [SerializeField] private static readonly int DissolveID =
        Shader.PropertyToID("_Dissolve_Strength");

    private void Awake()
    {
        renderers = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();

        SetDissolve(0f);
    }

    public void DissolveIn()
    {
        Debug.Log("DissolveIn called: " + Time.time);

        AnimateDissolve(0f, 1f);    
    }

    public void DissolveOut()
    {
        Debug.Log("DissolveOut called: " + Time.time);
        AnimateDissolve(1f, 0f);
    }

    private void AnimateDissolve(float start, float end)
    {
        if (dissolveCoroutine != null)
            StopCoroutine(dissolveCoroutine);

        dissolveCoroutine = StartCoroutine(
            DissolveRoutine(start, end)
        );
    }

    private IEnumerator DissolveRoutine(float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / dissolveDuration);

            SetDissolve(Mathf.Lerp(start, end, t));

            yield return null;
        }

        SetDissolve(end);
        dissolveCoroutine = null;
    }

    private void SetDissolve(float amount)
    {
        
      renderers.GetPropertyBlock(propertyBlock);
      propertyBlock.SetFloat(DissolveID, amount);
      renderers.SetPropertyBlock(propertyBlock);
       
    }
}
