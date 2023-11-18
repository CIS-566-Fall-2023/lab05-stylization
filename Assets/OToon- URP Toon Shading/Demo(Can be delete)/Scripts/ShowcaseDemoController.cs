using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OToon
{
    public class ShowcaseDemoController : MonoBehaviour
    {

        public Animator[] m_models;
        public Transform m_modelRoot;
        public GameObject m_lightsRoot;
        public Transform[] m_camPoints;
        public Light m_mainLight;
        public Slider m_lightDirectionSlider;
        private Transform m_camTrans;
        public GameObject[] m_cameraHints;
        public GameObject[] m_modelButtons;
        private int m_index = 0;
        private int m_camIndex = 0;
        private bool m_lightsOn = true;
        private bool m_isfirstTimeSwitch = true;
        private bool[] m_hintsOpened;
        private void Start()
        {
            m_hintsOpened = new bool[m_cameraHints.Length];
            for (int i = 0; i < m_hintsOpened.Length; i++)
            {
                m_hintsOpened[i] = false;
            }
            m_camTrans = Camera.main.transform;
            m_lightDirectionSlider.onValueChanged.AddListener(OnMainLightDirectionSliderUpdate);
            EnableAdditionalLights(m_lightsOn);
            m_models = m_modelRoot.GetComponentsInChildren<Animator>(true);
            SelectModule(m_index);
            m_hintsOpened[0] = true;
        }

        private void Update()
        {
            m_camTrans.position = Vector3.Lerp(m_camTrans.position, m_camPoints[m_camIndex].position, 1 - Mathf.Exp(-2f * Time.deltaTime));
            m_camTrans.rotation = Quaternion.Slerp(m_camTrans.rotation, m_camPoints[m_camIndex].rotation, 1 - Mathf.Exp(-2f * Time.deltaTime));
        }

        private void SelectModule(int index)
        {
            m_models[index].gameObject.SetActive(true);
        }

        public void OnMainLightDirectionSliderUpdate(float value)
        {
            m_mainLight.transform.eulerAngles = new Vector3(50, -30 + value * 180, 0);
        }

        private void EnableAdditionalLights(bool enabled)
        {
            foreach (var light in m_lightsRoot.GetComponentsInChildren<Light>())
            {
                light.enabled = enabled;
            }

            foreach (var text in m_lightsRoot.GetComponentsInChildren<TMPro.TextMeshPro>())
            {
                text.enabled = enabled;
            }

            foreach (var renderer in m_lightsRoot.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = enabled;
            }
        }

        public void SwitchCameraPos()
        {
            m_cameraHints[m_camIndex].SetActive(false);
            m_camIndex++;
            if (m_camIndex >= m_camPoints.Length)
            {
                m_camIndex = 0;
            }
            if (m_hintsOpened[m_camIndex] == false)
            {
                m_hintsOpened[m_camIndex] = true;
                m_cameraHints[m_camIndex].SetActive(true);
            }
            foreach (var btn in m_modelButtons)
            {
                btn.SetActive(m_camIndex == 0);
            }
        }

        public void ToggleAdditionalLightEnabled()
        {
            m_lightsOn = !m_lightsOn;
            EnableAdditionalLights(m_lightsOn);
        }

        public void OnNext()
        {
            m_models[m_index].gameObject.SetActive(false);
            m_index++;
            if (m_index >= m_models.Length)
            {
                m_index = 0;
            }
            SelectModule(m_index);
        }

        public void OnPrevious()
        {
            m_models[m_index].gameObject.SetActive(false);
            m_index--;
            if (m_index < 0)
            {
                m_index = m_models.Length - 1;
            }
            SelectModule(m_index);
        }
    }

}
