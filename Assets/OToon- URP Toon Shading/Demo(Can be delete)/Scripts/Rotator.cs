using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OToon
{

    public class Rotator : MonoBehaviour
    {
        public Vector3 Rotation;
        private float Elapsed;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Elapsed += Time.deltaTime;
            if (Elapsed >= 100)
            {
                Elapsed = 0;
            }
            transform.rotation = Quaternion.Euler(50, 50 * Mathf.Sin(Elapsed * 2), 0);
            //   transform.Rotate(Rotation * Time.deltaTime * Mathf.Sin(Elapsed));
        }
    }

}