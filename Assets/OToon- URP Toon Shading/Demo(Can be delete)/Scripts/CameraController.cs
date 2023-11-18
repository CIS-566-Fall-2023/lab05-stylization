using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OToon
{

    public class CameraController : MonoBehaviour
    {
        public Transform Target;
        public Transform[] Rotator;
        public float Speed = 50f;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (Target != null)
                    Target.Rotate(new Vector3(0, Speed, 0) * Time.deltaTime);
                foreach (var obj in Rotator)
                {
                    obj.Rotate(new Vector3(0, Speed, 0) * Time.deltaTime);
                }
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (Target != null)
                    Target.Rotate(new Vector3(0, -Speed, 0) * Time.deltaTime);
                foreach (var obj in Rotator)
                {
                    obj.Rotate(new Vector3(0, -Speed, 0) * Time.deltaTime);
                }
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.position += Vector3.forward * 5 * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.position -= Vector3.forward * 5 * Time.deltaTime;
            }
        }
    }

}