using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief 각 Floor Prefab을 층별로 생성해주는 스크립트

@author 진민준

@date 2019-11-24

@version 1.2.5

@details
최초 작성일: 190922

Recently modified list
- 대기모드 진입시간 4초 -> 5초로 변경

*/

public class FieldConstructor : MonoBehaviour
{
    Camera mainCamera;

    [SerializeField]
    GameObject menuTap;

    // 일정 시간 유저의 입력이 없으면 상하단 ui바 를 숨긴다
    [SerializeField]
    GameObject upHideBar;
    [SerializeField]
    GameObject downHideBar;
    public const float HIDE_DISTANCE = 2f;
    public const float WAIT_TIME = 4.0f; // 대기모드로 들어가기까지 걸리는 시간
    Vector3 mousePos;
    bool waitFlag;
    float waitTime;

    bool hideCoroutineIsOn;

    public GameObject FloorPrefab;
    public const int MAX_HEIGHT = 3;
    public const int MAX_UPGRADE = 3;
    
    public const float elevateSpeed = 50.0f;
    bool elevateCoroutineIsOn;

    // 현재 MainCamera에 비춰지고있는 Floor의 Index.
    // 추후 데이터를 통합 관리하는 스크립트로 이동될 예정
    public int curActivateFloorIdx { set; get; }

    public GameObject[] Floor = new GameObject[MAX_HEIGHT];

    private void Awake()
    {
        waitFlag = false;
        waitTime = 0f;
        hideCoroutineIsOn = false;

        elevateCoroutineIsOn = false;

        curActivateFloorIdx = 0;

        // y축 변경해가며 층 배치
        // 층을 배치할 EmptyObject를 생성
        for (int i = 0; i < MAX_HEIGHT; i++)
        {
            Floor[i] = Instantiate(FloorPrefab, new Vector3(0, i*15 -3 + 4.43f, 0), transform.rotation);
            Floor[i].GetComponent<FieldManager>().instantiater(i+1);
            Floor[i].GetComponent<FieldManager>().rendererController(false); // 화면 출력 x
        }
        
        Floor[0].GetComponent<FieldManager>().rendererController(true); // 화면 출력 o
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        // 카메라위치셋팅
    }

    private void Update()
    {

        // 플레이어에게 아무 입력이 없다면
        if (mousePos == Input.mousePosition && Input.anyKey == false)
        {
            // 현재 홈화면(메뉴탭이 꺼진 상황)이라면
            if (!menuTap.gameObject.activeSelf)
            {
                waitTime += Time.deltaTime;
                if (waitTime > WAIT_TIME && waitFlag == false)
                {
                    //Debug.Log("hide ON");
                    waitFlag = true;
                    //Debug.Log("hide ON");

                    StartCoroutine("hideOnAnim");
                }
                //waitFlag = true;
            }
        }

        else
        {
            waitTime = 0f;
            if (waitFlag)
            {
                waitFlag = false;
                StartCoroutine("hideOffAnim");
            }
            mousePos = Input.mousePosition;
        }
    }

    public void elevator(bool updown)
    {
        if (!elevateCoroutineIsOn)
        {
            if (updown) // UP
            {
                if (curActivateFloorIdx < MAX_HEIGHT - 1)
                {
                    //curActivateFloorIdx += 1;
                    //mainCamera.transform.Translate(new Vector3(0, 15, 0));
                    StartCoroutine("elevatorUpAnim");
                }
                else
                {
                    Debug.Log("더 올라갈 수 없습니다");
                }
            }

            else // DOWN
            {
                if (curActivateFloorIdx > 0)
                {
                    //curActivateFloorIdx -= 1;
                    //mainCamera.transform.Translate(new Vector3(0, -15, 0));
                    StartCoroutine("elevatorDownAnim");
                }
                else
                {
                    Debug.Log("더 내려갈 수 없습니다");
                }
            }
        }
    }

    // 190928 Elevator Animation Function
    IEnumerator elevatorUpAnim()
    {
        curActivateFloorIdx += 1;
        Floor[curActivateFloorIdx].GetComponent<FieldManager>().rendererController(true); // 화면 출력 o
        elevateCoroutineIsOn = true;
        float addMoveYPos = 0;
        float tmpYPos = mainCamera.transform.position.y % 15;
        while (addMoveYPos < 15)
        {
            // 직전까지 움직인 누적 YPos
            // 목표를 지나치지 않게 하기 위함
            float tmpSum = addMoveYPos;
            addMoveYPos += elevateSpeed * Time.deltaTime;
            /*
            if (addMoveYPos > 15)
            {
                //mainCamera.transform.Translate(new Vector3(0, 15 - tmpSum, 0));
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, tmpYPos + 15 * curActivateFloorIdx, mainCamera.transform.position.z);
                Debug.Log("정확한 위치에 도착");
            }
            */
            if (addMoveYPos < 15)
            {
                mainCamera.transform.Translate(new Vector3(0, elevateSpeed * Time.deltaTime, 0));
            }

            yield return null;
        }
        elevateCoroutineIsOn = false;
        Floor[curActivateFloorIdx-1].GetComponent<FieldManager>().rendererController(false); // 화면 출력 o
    }

    IEnumerator elevatorDownAnim()
    {
        curActivateFloorIdx -= 1;
        Floor[curActivateFloorIdx].GetComponent<FieldManager>().rendererController(true); // 화면 출력 o
        elevateCoroutineIsOn = true;
        float addMoveYPos = 0;
        float tmpYPos = mainCamera.transform.position.y % 15;
        while (addMoveYPos > -15)
        {
            float tmpSum = addMoveYPos;
            addMoveYPos -= elevateSpeed * Time.deltaTime;
            /*
            if (addMoveYPos < -15)
            {
                //mainCamera.transform.Translate(new Vector3(0, 15 + tmpSum, 0));
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, tmpYPos + 15 * curActivateFloorIdx, mainCamera.transform.position.z);
            }
            */
            if (addMoveYPos>-15)
            {
                mainCamera.transform.Translate(new Vector3(0, -elevateSpeed * Time.deltaTime, 0));
            }

            yield return null;
        }
        elevateCoroutineIsOn = false;
        Floor[curActivateFloorIdx+1].GetComponent<FieldManager>().rendererController(false); // 화면 출력 o
    }

    IEnumerator hideOnAnim()
    {
        float addMovePos = 0f;

        while (addMovePos < HIDE_DISTANCE)
        {
            float dist = HIDE_DISTANCE * (Time.deltaTime * 2);
            upHideBar.transform.Translate(new Vector3(0, dist, 0));
            downHideBar.transform.Translate(new Vector3(0, -dist, 0));
            addMovePos += dist;
            yield return null;
        }


    }
    IEnumerator hideOffAnim()
    {
        float addMovePos = 0f;

        while (addMovePos < HIDE_DISTANCE)
        {
            float dist = HIDE_DISTANCE * (Time.deltaTime * 2);
            upHideBar.transform.Translate(new Vector3(0, -dist, 0));
            downHideBar.transform.Translate(new Vector3(0, dist, 0));
            addMovePos += dist;
            yield return null;
        }

        // Hide off시 ui를 좌표상에 고정. 점점 오차 편차가 벌어지는 버그 방지 대책
        //downHideBar.transform.position = new Vector3(0,0,0);
        upHideBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);
        downHideBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);


    }

    public void fieldUpgrade()
    {
        Floor[curActivateFloorIdx].GetComponent<FieldManager>().fieldUpgrade(MAX_UPGRADE);
    }
}
