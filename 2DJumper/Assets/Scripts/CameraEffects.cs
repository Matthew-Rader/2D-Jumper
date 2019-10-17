using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraEffects : MonoBehaviour
{
	[SerializeField] private CinemachineBrain _CamBrain;
	[SerializeField] private float cameraShakeDuration;
	[SerializeField] private float cameraShakeAmount;
	CinemachineVirtualCamera vcam;
	CinemachineBasicMultiChannelPerlin noise;

	public void ApplyCameraShake () {
		vcam = (CinemachineVirtualCamera)_CamBrain.ActiveVirtualCamera;
		noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

		StartCoroutine(CameraShake());
	}

	IEnumerator CameraShake () {
		noise.m_AmplitudeGain = cameraShakeAmount;
		noise.m_FrequencyGain = cameraShakeAmount;
		yield return new WaitForSeconds(cameraShakeDuration);
		noise.m_AmplitudeGain = 0;
		noise.m_FrequencyGain = 0;
	}
}
