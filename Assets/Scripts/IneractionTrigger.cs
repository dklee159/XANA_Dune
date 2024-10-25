using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IneractionTrigger : MonoBehaviour
{
    [SerializeField] Button triggerBtn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PhotonLocalPlayer"))
        {
            triggerBtn.gameObject.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PhotonLocalPlayer"))
        {
            triggerBtn.transform.LookAt(Camera.main.transform);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PhotonLocalPlayer"))
        {
            triggerBtn.gameObject.SetActive(false);
        }
    }
}
