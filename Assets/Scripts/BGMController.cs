using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
밤/낮에 따른 bgm 컨트롤

@author 진민준

@date 2019-11-29

@version 1.0.2

@details
최초 작성일: 191124

Recently modified list
- 노래가 끝나면 자동으로 밤으로 전환됩니다.

*/

public class BGMController : MonoBehaviour
{
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        audioSource.Stop();
    }

    private void Update()
    {

    }

    public void SetBGM(bool isDaytime)
    {
        if (isDaytime)
        {
            audioSource.Play();
            // bgm 종료 체크 시작
            StartCoroutine("isBGMPlaying");
        }
        else
        {
            
            StopCoroutine("isBGMPlaying");
            audioSource.Stop();
        }
    }

    IEnumerator isBGMPlaying()
    {

        while (audioSource.isPlaying)
        {
            Debug.Log("bgm playing >> "+ audioSource.isPlaying);
            yield return null;
        }

        //낮밤 변경
        GameObject.Find("GameManager").GetComponent<DayController>().changeDay();
        SetBGM(false);
    }
}
