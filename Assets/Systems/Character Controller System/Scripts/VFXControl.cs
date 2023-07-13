using DreamersInc.ComboSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXControl : MonoBehaviour
{
    ComboSO comboInstance;
    Animator anim;

    public  void Init( ComboSO instance)
    {
        anim = GetComponent<Animator>();
        comboInstance = instance;
    }

    public void TriggerVFX()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);
        var vfx = comboInstance.GetVFX(state);
        vfx.SpawnVFX(this.transform);
    }
}
