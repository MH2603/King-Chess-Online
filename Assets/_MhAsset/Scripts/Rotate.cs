using UnityEngine;
using MH;

namespace MH
{

    public class Rotate : MonoBehaviour
    {
        public Vector3 RotateSpeed= Vector3.one;    

        public void FixedUpdate()
        {
            transform.Rotate(RotateSpeed * Time.fixedDeltaTime);
        }
    }

}
