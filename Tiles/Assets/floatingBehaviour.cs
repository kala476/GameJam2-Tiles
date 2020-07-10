using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class floatingBehaviour : MonoBehaviour
{
    [SerializeField] Rigidbody rigid;
    public bool activateFloating;
    public int startPushStrength;
    public float torqueStrength;

    public float duration;
    public float moveDistanceY;
    private bool isFloating;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        // when activatefloating is true and object not already floating
        if (activateFloating && !isFloating)
        {

            float randomRange1 = UnityEngine.Random.Range(0, 10);
            float randomRange2 = UnityEngine.Random.Range(0, 10);
            float randomRange3 = UnityEngine.Random.Range(0, 10);

            //this.rigid.AddForce(new Vector3(0, randomRange1, 0)* startPushStrength);
            //Debug.LogError(new Vector3(randomRotation1, randomRotation2, randomRotation3) * torqueStrength);
            this.rigid.angularVelocity = new Vector3(randomRange1, randomRange2, randomRange3) * torqueStrength;

            rigid.useGravity = false;
            StartCoroutine(Float());
            isFloating = true;

        }
        if (!activateFloating)
        {
            StopCoroutine(Float());
            isFloating = false;
            rigid.useGravity = true;


        }
        if (rigid.isKinematic)
        {
            StopCoroutine(Float());
            isFloating = false;

            this.rigid.velocity = Vector3.zero;
            this.rigid.angularVelocity = Vector3.zero;
        }
    }

    private IEnumerator Float()
    {
        isFloating = true;

        while (activateFloating && !rigid.isKinematic)
        {

            Debug.LogError(Float());
            this.transform.DOBlendableMoveBy(new Vector3(0, -1 * moveDistanceY, 0), duration).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(duration);
            this.transform.DOBlendableMoveBy(new Vector3(0, 1 * moveDistanceY, 0), duration).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(duration);

        }

        //yield break;
        


    }
}
