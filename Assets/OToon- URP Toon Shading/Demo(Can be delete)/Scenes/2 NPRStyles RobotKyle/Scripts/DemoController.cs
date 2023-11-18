using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour
{
    private Animator[] m_demoCharacters;
    private int m_currentIndex = 0;
    private float m_characterDemoStartTime;

    [ContextMenu("spread stand")]
    public void SpreadStandingPos()
    {
        var transArray = GetComponentsInChildren<Animator>();
        var currentRow = 0;
        for (int i = 0; i < transArray.Length; i++)
        {
            if (i % 5 == 0)
                currentRow++;
            transArray[i].transform.position = new Vector3(-3.2f + (i % 5) * 1.6f, 0, -8f + currentRow * 3f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_demoCharacters = GetComponentsInChildren<Animator>();
        m_characterDemoStartTime = Time.time;
        m_demoCharacters[m_currentIndex].GetComponentInChildren<Renderer>().enabled = true;
    }
}
