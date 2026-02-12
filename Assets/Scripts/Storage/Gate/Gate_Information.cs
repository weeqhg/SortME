using UnityEngine;
using UnityEngine.Localization.Components;

public class Gate_Information : MonoBehaviour
{
    public int gateIndex;

    void Start()
    {
        GetComponent<LocalizeStringEvent>()
            .StringReference.Arguments = new object[] { gateIndex };
    }
}
