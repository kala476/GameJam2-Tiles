using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FloatingBehavior : MonoBehaviour
{
    public bool activateFloating = true;
    //public int startPushStrength;
    public float torqueStrength;
    public float duration;
    public float moveDistanceY;
    private Rigidbody rigid;
    private Coroutine lastRoutine;
    private bool isFloating;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // when activatefloating is true and object not already floating
        if (activateFloating && !isFloating)
        {

            float randomRange1 = Random.Range(0, 10);
            float randomRange2 = Random.Range(0, 10);
            float randomRange3 = Random.Range(0, 10);

            rigid.angularVelocity = new Vector3(randomRange1, randomRange2, randomRange3) * torqueStrength;

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
            isFloating = false;
            rigid.useGravity = true;


        }
        if (rigid.isKinematic)
        {
            StopCoroutine(lastRoutine);
            isFloating = false;

            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    private IEnumerator Float()
    {
        isFloating = true;

        while (activateFloating && !rigid.isKinematic)
        {
            transform.DOBlendableMoveBy(new Vector3(0, -1 * moveDistanceY, 0), duration).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(duration);
            transform.DOBlendableMoveBy(new Vector3(0, 1 * moveDistanceY, 0), duration).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(duration);
        }
        yield return null;
    }
}
