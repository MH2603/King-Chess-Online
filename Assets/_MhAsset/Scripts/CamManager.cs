using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    public Camera cam;
    public Transform posCamLook;


    private void Start()
    {
        cam.transform.LookAt(posCamLook);
    }
}
