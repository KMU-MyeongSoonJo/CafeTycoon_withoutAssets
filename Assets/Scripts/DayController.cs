using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
낮과 밤을 컨트롤하는 스크립트

@author 진민준

@date 2019-12-03

@version 1.0.4

@details
최초 작성일: 191123

Recently modified list
- 밤->낮 일 때 다시 모든 직원이 걷게 만들어 줍니다.

*/

public class DayController : MonoBehaviour
{
    // 낮:
    //  직원(하나) 계산대에 고정, 손님 주기적으로 입장
    //  오브젝트 컨트롤 불가
    //  (임시)space로 밤으로 넘기기 가능
    //      손님 추가 입장 x, 모두 퇴실하면 밤 시작?
    // 밤:
    //  오브젝트 편집 가능
    //  직원 서성거리기, 손님 입장 불가
    //  (임시)space로 낮으로 넘기기 가능
    //      직원(하나) 카운터로 이동

    public bool isDaytime { set; get; }
    BGMController bgm;

    private void Awake()
    {
        // 시작은 저녁.
        isDaytime = false;
        // 배경 검정으로 설정
        GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = new Color(0, 0, 0);

        GameObject.Find("Main Camera").GetComponent<BGMController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 낮밤 변경
            changeDay();
        }
    }
    
    // 낮밤 셋팅
    public void changeDay()
    {
        // 낮밤 변경
        isDaytime = !isDaytime;

        GameObject.Find("FigureCanvas").GetComponent<FigureUIController>().setDayNight(isDaytime);
        // 밤이라면 배경을 검게
        if (!isDaytime)
        {
            GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = new Color(0, 0, 0);
            GameObject.Find("Main Camera").GetComponent<BGMController>().SetBGM(false);

            // 모든 직원의 state를 초기화
            // 모든 층의 직원들 조회 및 state 초기화
            // 카운터에 서 있던 직원들 걷기 애니메이션으로 전환
            for (int i = 0; i < 3; i++)
            {
                CharacterController[] employees = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[i].GetComponentsInChildren<CharacterController>();

                foreach (var v in employees)
                {
                    if (v.isEmployee)
                    {
                        v.animator.SetBool("isStand", false);
                        // state 초기화
                        v.resetState();
                    }
                }
            }
        }
        else
        {
            // 낮이라면 기존 색으로
            GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = new Color(22 / 255f, 48 / 255f, 84 / 255f);
            GameObject.Find("Main Camera").GetComponent<BGMController>().SetBGM(true);
        }

        Debug.Log(isDaytime ? "낮이되었습니다" : "밤이되었습니다");

    }

}
