using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public Animator CamAnim;

    public void cameraShake()
    {
        CamAnim.SetTrigger("Shake");
    }


}
