using System;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PlayerAimWeapon : MonoBehaviour
{
    public Transform aimTransform;
    private Animator aimAnim;
    //private LineRenderer aimLine;

    private void Awake()
    {
        aimAnim = aimTransform.GetComponentInChildren<Animator>();
        //aimLine = aimTransform.GetComponent<LineRenderer>();
        //aimLine.positionCount = 2;
        //aimLine.startWidth = 0.05f;
        //aimLine.endWidth = 0.05f;
    }

    void Update()
    {
        Aiming();
        //Shooting();
    }

    private void Aiming()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Vector3 aimDir = (mousePos - aimTransform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        aimTransform.eulerAngles = new Vector3(0, 0, angle);

        Vector3 localScale = Vector3.one;
        localScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
        aimTransform.localScale = localScale;

        //aimLine.SetPosition(0, aimTransform.position);
        //aimLine.SetPosition(1, mousePos);
    }

    private void Shooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            aimAnim.SetTrigger("Shoot");
            TempCamShake.Instance.Shake(0.1f, 0.1f);
        }
    }
}
