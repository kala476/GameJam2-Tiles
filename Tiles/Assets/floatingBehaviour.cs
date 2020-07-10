using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class floatingBehaviour : MonoBehaviour
{
    public bool activateFloating;
    //public int startPushStrength;
    public float torqueStrength;
    public float duration;
    public float moveDistanceY;
    private Rigidbody rigid;
    private Coroutine lastRoutine;
    private bool isFloating;
    private Tweener tween;

    // Start is called before the first frame update
    void Start()
    {
        rigid = this.transform.GetComponent<Rigidbody>();
        FloatCheck();

    }

    // Update is called once per frame
    void LateUpdate()
    {
        FloatCheck();
    }

    private IEnumerator Float()
    {
        isFloating = true;

        while (activateFloating && !rigid.isKinematic)
        {

            //Debug.LogError(" Float()");
            tween = this.transform.DOBlendableMoveBy(new Vector3(0, -1 * moveDistanceY, 0), duration).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(duration);
            tween = this.transform.DOBlendableMoveBy(new Vector3(0, 1 * moveDistanceY, 0), duration).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(duration);

        }
        yield return null;

    }

    private void FloatCheck()
    {
        // when activatefloating is true and object not already floating
        if (activateFloating && !isFloating && !rigid.isKinematic)
        {

            float randomRange1 = UnityEngine.Random.Range(0, 10);
            float randomRange2 = UnityEngine.Random.Range(0, 10);
            float randomRange3 = UnityEngine.Random.Range(0, 10);

            //this.rigid.AddForce(new Vector3(0, randomRange1, 0)* startPushStrength);
            //Debug.LogError(new Vector3(randomRotation1, randomRotation2, randomRotation3) * torqueStrength);
            this.rigid.angularVelocity = new Vector3(randomRange1, randomRange2, randomRange3) * torqueStrength;

            rigid.useGravity = false;
            lastRoutine = StartCoroutine(Float());
            isFloating = true;

        }
        if (!activateFloating)
        {
            if (lastRoutine != null)
            {
                StopCoroutine(lastRoutine);
            }
            DOTween.Kill(this.transform);
            isFloating = false;
            rigid.useGravity = true;


        }
        if (rigid.isKinematic)
        {

            if (lastRoutine != null)
            {
                StopCoroutine(lastRoutine);
            }
            DOTween.Kill(this.transform);

            isFloating = false;

            this.rigid.velocity = Vector3.zero;
            this.rigid.angularVelocity = Vector3.zero;
        }
    }
}
