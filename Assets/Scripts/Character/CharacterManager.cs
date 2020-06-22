using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
캐릭터 클릭 및 status 출력 등을 총괄하는 스크립트

@author 진민준

@date 2019-11-27

@version 1.2.1

@details
최초 작성일: 191113

Recently modified list
- 불필요하게 남아있던 target(편집할 캐릭터) 정보를 보다 명확히 개선
    - 오브젝트 클릭시, AI 레이어가 함께 포함되었다면 선택 x
    .
- 편집 탭의 버튼 뒤에 캐릭터가 있으면 버튼이 아닌 캐릭터가 선택되는 버그 수정
    - ray에서 ui가 포함되면 ray를 끊는다
    .
- 캐릭터 트래킹 정상 작동

*/

public class CharacterManager : MonoBehaviour
{
    // 직원 및 손님들 목록
    [SerializeField]
    GameObject[] CharactersPrefab;

    // 선택된 캐릭터를 트래킹한다
    GameObject mainCamera;

    GameObject target;

    Canvas characterControlTap; // 캐릭터 편집 탭 (트래킹, 상호작용 등)
    bool isActivate; // 편집이 진행 중임을 알리는 플래그(트래킹 등). 편집 버튼 클릭과 동시에 true

    //bool isTapOn;
    // 버튼 클릭과 함께 isTrackingOn이 꺼지는 버그를 막기 위한 대책
    // 버튼 클릭과 함께 Flag는  true가 된다
    // 마우스 입력마다 Flag가 true라면 false로, false라면 트래킹 종료
    bool isTrackingOn;
    bool trackingFlag; 

    bool isTapOn;
    

    private void Awake()
    {
        characterControlTap = GameObject.Find("CharacterControlCanvas").GetComponent<Canvas>();
        mainCamera = GameObject.Find("Main Camera");
        isActivate = false;
        isTrackingOn = false;
        trackingFlag = false;
        isTapOn = false;
        target = null;
    }

    private void Start()
    {
        CharacterEmployer(0);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("target >> " + target);
        //Debug.Log("tmp >> " + tmp);

        if (Input.GetMouseButtonUp(0))
        {
            if (trackingFlag) trackingFlag = false;
            else isTrackingOn = false;


            // 캐릭터 편집 탭 버튼 클릭과 동시에 ray가 발사되어
            // 편집 탭이 꺼지는 버그를 막기 위한 플래그 설정
            if (isActivate) {
                isActivate = false;
            }
            // 정상 진행
            else
            {
                GameObject tmp = GetClickedCharacter(); // 커서가 가리키는 지점에 위치한 캐릭터를 target으로 지정

    
                if (tmp != null)
                {
                    // ai layer
                    if (tmp.layer == 12)
                    {
                        if (tmp == target)
                        {
                            CallCharacterController(false);
                        }
                        else
                        {
                            target = tmp;
                            CallCharacterController(true);
                        }
                    }
                }
                else
                {
                    CallCharacterController(false);
                }



            }
        }

        // 트래킹 중 카메라를 이동(상하좌우)한다면 자동트래킹 종료
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)|| Input.GetKeyDown(KeyCode.A)|| Input.GetKeyDown(KeyCode.D))
        {

            //    // 트래킹 종료
            if (target != null)
            {
                CallCharacterController(false);
                CharacterControl_Tracking(false);
            }
            
        }

        // 카메라 트래킹 진행중
        if (isTrackingOn)
        {
            if (target != null)
            {
                mainCamera.transform.position = target.transform.position;
                mainCamera.transform.Translate(new Vector3(0, 0, -20));
            }

            // 진행 중 null이 되었다면
            // >> 퇴장(Destroy)한 경우
            // >> ?
            else
            {
                mainCamera.transform.Translate(new Vector3(0, 0, -10 - mainCamera.transform.position.z));
                isTrackingOn = false;
            }
        }
    }

    // 버튼을 눌러 캐릭터를 생성하는 오브젝트
    public void CharacterEmployer(int idx)
    {
        

        // 현재 층을 받아온 후
        // FieldManager에서 그 층의 chairCount를 사용해 손님 입장
        int curFloorIdx = GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx;
        GameObject curFloor = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[curFloorIdx];
        FieldManager fieldManager = curFloor.GetComponent<FieldManager>();

        // 손님의 경우
        if (idx == 1)
        {
            //밤에는 손님 입장 x
            if (!GameObject.Find("GameManager").GetComponent<DayController>().isDaytime) return;

            Debug.Log("GUEST");
            // 가용한 의자 수만큼 손님 입장 가능
            // 의자가 5개, 손님이 5명 있더라도 2명이 퇴장하는 중이라면
            // 7명까지 입장 가능
            int openChairCount = 0;
            for (int x = 0; x < fieldManager.CUR_ROW; x++)
            {
                for (int y = 0; y < fieldManager.CUR_COL; y++)
                {// 의자이고, 
                    if (fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState() == 4 && fieldManager.field[x, y].GetComponent<ArrangeablePosState>().putHere.GetComponent<ChairSurporter>().getTable() != null)
                    {// 테이블이 있는 의자라면
                     //if (fieldManager.field[x, y].GetComponent<ArrangeablePosState>().putHere.GetComponent<ChairSurporter>().myTable != null)
                        openChairCount++;
                    }
                }
            }

            // 손님이 앉을 자리가 존재하지 않으면 중지
            //if (fieldManager.guestCount < fieldManager.chairCount) 
            if (openChairCount <= 0) { return; }

            // 생성
            // 손님의 경우 랜덤하게 생성(색으로 구분)
            fieldManager.guestCount++; // 손님 수 +1
            idx = Random.Range(1, 8);
        }

        // 해당 필드 전체 탐색. enterance(startPos) 를 찾기 위함
        GameObject curField = null;

        for (int x = 0; x < fieldManager.CUR_ROW; x++)
        {
            for (int y = 0; y < fieldManager.CUR_COL; y++)
            {
                // 현재 층에서 enterance 탐색
                if (fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState() == 3)
                {
                    curField = fieldManager.field[x, y];
                    break;
                }
            }
        }

        //GameObject curField = curFloor.GetComponent<FieldManager>().field[7, 7]; // ArrangeablePos
        Vector3 startPos = curField.transform.position;

        // 캐릭터 z축 깊이 부여
        startPos.z = curField.GetComponent<ArrangeablePosState>().getRowPos();
        
        //target = Instantiate(CharactersPrefab[idx], startPos, transform.rotation);
        GameObject tmp = Instantiate(CharactersPrefab[idx], startPos, transform.rotation);

        // 해당 필드의 자식으로 설정
        //target.transform.parent = curFloor.transform;
        tmp.transform.parent = curFloor.transform;

    }

    GameObject GetClickedCharacter()
    {
        int layerMask = 1;
        GameObject target = null;
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // AI, Untouchable layer에 대해서만 raycast
        //RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f, layerMask = 1 << 12 | 1 << 14);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);

        //  AI 레이어에서 무언가 캐릭터가 Click 되었다면
        // UntouchableLayer에 닿았다면 ray 차단
        // 해당 오브젝트 반환
        //if (hit.collider != null && hit.collider.gameObject.layer != 14)
        if (hit.collider != null && hit.collider.gameObject.layer == 12)
        {
            Debug.Log("트래킹 종료");
            isTrackingOn = false;
            target = hit.transform.parent.gameObject;
            return target;
        }
        
            
        //else if(hit.collider.gameObject.layer == 5)
        //{
        //    Debug.Log("bu");

        //    return null;
        //}
        else 
        {
            //if (hit.collider != null && hit.collider.gameObject.layer != 5)
            //    isTrackingOn = false;


            Debug.Log("return no character");
            //CallCharacterController(false);
            return null;
        }
        
    }

    // 캐릭터 편집 탭 호출
    // true: 시작 , false: 종료
    public void CallCharacterController(bool controlParameter)
    {
        // 탭 키기 명령
        if (controlParameter)
        {
            //캐릭터 머리 위로 호출
            characterControlTap.transform.position = target.transform.position;
            //characterControlTap.transform.parent = target.transform;
            //target.transform.Find("Clicker").localScale = new Vector3(2, 2, 1);
            characterControlTap.transform.SetParent(target.transform);
        }

        //탭 끄기 명령
        else if(!controlParameter)
        {
            //탭  종료
            if (target != null)
            {
                //부모관계 해제
                if (characterControlTap.transform.parent != null)
                    characterControlTap.transform.SetParent(characterControlTap.transform.parent.parent);
                // 위치 이동
                characterControlTap.transform.position = GameObject.Find("OutviewBase").GetComponent<Transform>().position;

                if (!isTrackingOn)
                {
                    target.transform.Find("Clicker").localScale = new Vector3(1, 1, 1);
                    target = null; // 트래킹 중이 아닐 때만 타겟 해제
                }
            }
            //isTapOn = false;
        }
        isTapOn = controlParameter;
    }


    // 캐릭터 편집 탭의 호출 여부를 반환
    bool isCharacterControllerCalled()
    {
        //호출되지 않았다면 false 반환
        if (characterControlTap.transform.position == GameObject.Find("OutviewBase").GetComponent<Transform>().position)
            return false;
        else return false;
    }

    // 카메라 트래킹
    public void CharacterControl_Tracking(bool trackingOn)
    {        
        // 트래킹 종료
        if (!trackingOn)
        {
            //target = null;
            //카메라 z축 복구
            mainCamera.transform.Translate(new Vector3(0, 0, -10 - mainCamera.transform.position.z));

            // 트래킹 종료
            isTrackingOn = false;
        }

        // 트래킹 시작
        else
        {
            isActivate = true;

            // 트래킹 시작
            isTrackingOn = true;
            trackingFlag = true;
        }
        CallCharacterController(false);
    }
    // 상호작용
    public void CharacterControl_Talk()
    {
        isActivate = true;

        Debug.Log("말걸기");

        CallCharacterController(false);
    }
}
