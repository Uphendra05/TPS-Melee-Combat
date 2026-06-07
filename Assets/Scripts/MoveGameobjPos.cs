using System;
using System.Collections;
using UnityEngine;

public class MoveGameobjPos : MonoBehaviour
{
    private Vector3 originalPos;
    public GameObject dagger;
    public GameObject spear;
    public GameObject particleSystemObjLocal;
    public GameObject particleSystemObjWorld;

    private bool isPressed;
    private void Awake()
    {
        originalPos = dagger.transform.position;
        particleSystemObjLocal.SetActive(true);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
       
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dagger.transform.position = originalPos;

        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isPressed = !isPressed;
            if (isPressed)
            {
                StopAllCoroutines();
                StartCoroutine(SwapSDF(spear.transform));
               
            }
            else
            {
                StopAllCoroutines();

                StartCoroutine(SwapSDF(dagger.transform));



            }
        }

    }




    IEnumerator SwapSDF(Transform parent)
    {

        particleSystemObjLocal.SetActive(false);
        particleSystemObjWorld.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        particleSystemObjWorld.transform.SetParent(parent, false);
        particleSystemObjLocal.transform.SetParent(parent, false);
        particleSystemObjLocal.SetActive(true);

        yield return new WaitForSeconds(2f);

        particleSystemObjWorld.SetActive(false);

        yield return null;
    }


}
