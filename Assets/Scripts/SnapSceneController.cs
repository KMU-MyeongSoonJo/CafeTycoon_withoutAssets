using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**

@brief
SnapScene 재생 스크립트       \n
로고의 Fade In-Out  \n
로고 재생 후 로딩(씬 전환) \n
    
@author 진민준

@date 2019-10-14

@version 1.0.2

@details
최초 작성일: 190925

Recently modified list
- 유니티 로고 삭제

*/

public class SnapSceneController : MonoBehaviour
{
    public Image TeamLogo; // fading할 이미지
    Color tempColor; // fading에 사용될 임시 색상 변수

    float time;
    float fadeTime; // fade in/out 소요시간
    byte progress;  // 1 2 TeamLogo fadein/out
                    // 3 done (wait nextScene is loaded)

    AsyncOperation asyncOper;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;

        time = 0.0f;
        fadeTime = 2.0f;
        progress = 1;

        // 이미지의 투명도를 0.0f로 변경
        tempColor = TeamLogo.color;
        tempColor.a = 0.0f;
        TeamLogo.color = tempColor;
    }

    // Start is called before the first frame update
    void Start()
    {
        asyncOper = SceneManager.LoadSceneAsync(1);
        asyncOper.allowSceneActivation = false;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        

        switch (progress) {
            case 1:
                tempColor = TeamLogo.color;

                tempColor.a = Mathf.Lerp(0f, 1f, time / fadeTime);
                TeamLogo.color = tempColor;
                if (!(tempColor.a < 1.0f))
                {
                    time = 0;
                    progress = 2;
                }
                break;

            case 2:
                tempColor.a = Mathf.Lerp(1f, 0f, time / fadeTime);
                TeamLogo.color = tempColor;

                if (!(tempColor.a > 0.0f))
                {
                    time = 0;
                    progress = 3;
                }
                break;

            case 3:
                //SceneManager.LoadScene(1);
                Debug.Log("ready");
                asyncOper.allowSceneActivation = true;
                break;

        }
        if (Input.anyKeyDown)
        {
            Debug.Log("player want skip");
        }
    }

    

}
