using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongMove : MonoBehaviour
{
    public float m_speed = 1f;
    public Vector3 m_movement;
    private Vector3 m_originPos;
    // Start is called before the first frame update
    void Start()
    {
        m_originPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Mathf.Sin(Time.time * m_speed) * m_movement + m_originPos;
    }
}
