using System;
using UnityEngine;

namespace _Scripts
{
    public class CalibrateHead : MonoBehaviour
    {

        private void Start() {
           Invoke(nameof(Correction), 0.5f);
        }

        private void Correction()
        {
            var camRot = Camera.main.transform.rotation;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, camRot.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }
}