using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//To scale the hp bar (green color) depending on hpNormalized value in battle
public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public bool IsUpdating { get; private set; }

    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }
    public IEnumerator SetHPSmooth(float newHp)
    {
        IsUpdating = true;
        float curHp = health.transform.localScale.x;
        float changeAmt = curHp - newHp;
        while (curHp - newHp > Mathf.Epsilon)
    {
            curHp -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHp, 1f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHp, 1f);

        IsUpdating = false;
    }
}
