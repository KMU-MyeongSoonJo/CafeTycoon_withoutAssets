using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
캐릭터(직원, 손님)의 움직임을 컨트롤하는 스크립트

@author 진민준

@date 2019-12-03

@version 1.9.4

@details
최초 작성일: 190921

Recently modified list
- 목표 없이 서성이는 모습 구현
- 이제 직원이 제자리에 서고, 다시 걷습니다.

*/

public class CharacterController : MonoBehaviour
{
    // 게스트 여부
    // true: 목적지를 갖고 움직인다(손님)
    // false:가게 내를 서성거린다(직원)
    public bool isEmployee; 

    [SerializeField]
    int state;  // 상태
               

    public bool dayChangeFlag; // 날이 바뀔 떄 state를 초기화해주기위한 플래그
    int tmpScaleX = 1;// 카운터 도착 후 방향을 맞추기 위한 변수


    [HideInInspector]
    public Animator animator;

    //FieldManager의 CheckAround() 호출
    FieldManager FM;
    // PathFinder의 A* 길찾기 startPathFinding() 호출
    PathFinder PF;

    // 접시(음식, 컵 등)를 놓는 자리
    GameObject Plate;
    //놓을 음식
    [SerializeField]
    Sprite plate;
    // 착석 후 대기 시간
    float waitTime;

    // 이동 경로
    //List<PathFinder.WayPoint> route = new List<PathFinder.WayPoint>();
    [SerializeField]
    int dest_row, dest_col; // 도착 위치 좌표
    GameObject destPos; // 도착 위치(Tile) 게임오브젝트
    int astarIdx;
    int[,] route;

    float timer;
    [SerializeField]
    bool isCoroutineOn; // 코루틴(이동)이 작동중인지

    int cur_row, cur_col; // 현재 위치 ( FieldMananger.field[cur_row, cur_col] )
    

    Vector3 dest; // 이동 도착지점
   // GameObject destPos; // 이동 도착지점 2
    // 생성자의 기능을 한다
    private void Awake()
    {
        animator = GetComponent<Animator>();

        route = null;
        isCoroutineOn = false;

        state = 0;
        dayChangeFlag = false;
        //astar 루트 경로를 순차로 읽기 위한 idx
        astarIdx = 0;
        

        dest_row = -1;
        dest_col = -1;

        // 음식 보이지 않게.
        if (plate != null)
        {
            Plate = transform.Find("Plate").gameObject;
            Plate.GetComponent<Renderer>().enabled = false;

            Plate.GetComponent<SpriteRenderer>().sprite = plate;
        }
    }

    private void Start()
    {
        // 속한 Floor의 콤포넌트들 가져오기
        FM = GetComponentInParent<FieldManager>();
        PF = GetComponentInParent<PathFinder>();

        waitTime = Random.Range(5, 10);


        // 캐릭터 버튼 클릭시 배치될 위치
        // 자신의 현재 위치 저장
        for (int x = 0; x < FM.CUR_ROW; x++)
        {
            for (int y = 0; y < FM.CUR_COL; y++)
            {
                if (FM.field[x, y].GetComponent<ArrangeablePosState>().getState() == 3)
                {
                    cur_row = x;
                    cur_col = y;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    // 직원이면서 space를 눌러 밤이 됐다면
        //    if (isEmployee && !GameObject.Find("GameManager").GetComponent<DayController>().isDaytime)
        //        resetState();
        //}

        // 손님은 의자로 가서 앉는다
        // A* 작동
        if (!isEmployee)
        {
            
            // 캐릭터의 위치 , 
            // 도착지점의 위치 ( field[x, y]의 x, y 인덱스? )
            // 현재 위치와 도착 위치를 전달해 이동할 경로를 반환

            // 손님 카페 입장
            switch (state)
            // 입장 - 카운터 - 의자 - 착석 - 퇴장
            // 0: 경로 탐색(카운터)
            // 1: 출발(coroutine)
            // 2: 도착(주문)
            // 3: 경로 탐색(의자)    
            // 4: 출발 (corountine)
            // 5: 도착 (착석)
            // 6: 식사
            // 7: 경로 탐색(출구)
            // 8: 퇴장 (coroutine)
            {
                case 0:
                    destPos = FM.destPosOfCounter(out dest_row, out dest_col, out tmpScaleX, true);

                    route = PF.startPathFinding(cur_row, cur_col, dest_row, dest_col);
                    if (route == null)
                    {
                        Debug.Log("카운터 경로 존재 X");
                        return;
                    }

                    // 경로가 있다면
                    // idx 값 셋팅 및 출발(state = 1)
                    astarIdx = (route.Length / 2) - 1;
                    state = 1;
                    break;

                //카운터로 이동중
                case 1:
                    if (!isCoroutineOn)
                    {
                        //경로가 남았다면
                        if (astarIdx >= 0)
                        {
                            // 이동 경로 확인한 후 출발신호
                            dest = FM.field[route[0, astarIdx], route[1, astarIdx]].transform.position;
                            dest.z = FM.field[route[0, astarIdx], route[1, astarIdx]].GetComponent<ArrangeablePosState>().getRowPos();
                        }
                        //경로의 끝에 도달했다면
                        else
                        {
                            animator.SetBool("Stand", true);

                            gameObject.transform.localScale = new Vector3(tmpScaleX, 1, 1);
                            state = 2;
                            break;
                        }

                        // 이동 시작
                        Debug.Log("astar start");
                        isCoroutineOn = true;
                        StartCoroutine("moving_astar");
                    }
                    break;
                case 2:

                    // 도착. n초간 대기
                    timer += Time.deltaTime;
                    if (timer >= 2f)
                    {
                        state = 3; // n초간 대기 후 의자 탐색
                    }
                    break;
                // 의자로 가는 경로 탐색
                case 3:
                    destPos = FM.destPosOfChair(out dest_row, out dest_col);

                    // 경로를 2차원배열로 반환
                    route = PF.startPathFinding(cur_row, cur_col, dest_row, dest_col);
                    if (route == null)
                    {
                        Debug.Log("의자 경로 존재 X");
                        return;
                    }
                    // 밤이 되었다면 바로 퇴장
                    if (!GameObject.Find("GameManager").GetComponent<DayController>().isDaytime)
                    {
                        FM.destPosOfExitDoor(out dest_row, out dest_col);
                        animator.SetBool("Stand", false);
                        state = 6; break;
                    }

                    // 경로가 있다면
                    // idx 값 셋팅 및 출발(state = 1)
                    astarIdx = (route.Length / 2) - 1;
                    state = 4;

                    // 걸어가기
                    animator.SetBool("Stand", false);

                    // 음식보이게
                    Plate.GetComponent<Renderer>().enabled = true;
                    break;

                // 의자로 이동
                case 4:
                    // 이동 동작(coroutine)이 실행중이 아니라면
                    if (!isCoroutineOn)
                    {
                        //경로가 남았다면
                        if (astarIdx >= 0)
                        {
                            // 이동 경로 확인한 후 출발신호
                            dest = FM.field[route[0, astarIdx], route[1, astarIdx]].transform.position;
                            dest.z = FM.field[route[0, astarIdx], route[1, astarIdx]].GetComponent<ArrangeablePosState>().getRowPos()-0.1f;

                            //state = 1;
                        }
                        //경로의 끝에 도달했다면 착석

                        else
                        {
                            animator.SetBool("Sit", true);

                            // 타이머 값 설정
                            timer = 0f;
                            state = 5;
                            Transform tmpt = transform.Find("CharacterControlCanvas");

                            // 앉는 방향 설정(바닥 타일 따라서)
                            //transform.localScale = FM.field[cur_row, cur_col].GetComponent<ArrangeablePosState>().transform.localScale;
                            // 착석한 의자의 테이블이 플레이어 우측이라면 반전
                            if (transform.position.x < destPos.GetComponent<ArrangeablePosState>().putHere.GetComponent<ChairSurporter>().getTable().transform.position.x)
                            {
                                transform.localScale = new Vector3(-1, 1, 1);
                                //GameObject tmp = transform.Find("CharacterControlCanvas").gameObject;
                                if (tmpt != null)
                                    tmpt.localScale = new Vector3(-1, 1, 1);

                            }
                            else
                            {
                                transform.localScale = new Vector3(1, 1, 1);
                                if (tmpt != null)
                                    tmpt.localScale = new Vector3(1, 1, 1);
                            }

                            //Transform tmpt = transform.Find("CharacterControlCanvas");
                            //if (tmpt != null)
                            //{
                            //    tmpt.localScale = new Vector3(1, 1, 1);
                            //}

                            break;
                        }

                        // 이동 시작
                        isCoroutineOn = true;
                        StartCoroutine("moving_astar");
                    }
                    break;

                // Sit and Eat
                case 5:
                    // 도착. n초간 대기
                   timer += Time.deltaTime;
                    if (timer >= waitTime)
                    {
                        animator.SetBool("Sit", false);

                        // 문 좌표 리턴
                        FM.destPosOfExitDoor(out dest_row, out dest_col);

                        // 음식안보이게
                        Plate.GetComponent<Renderer>().enabled = false;
                        state = 6; // n초간 대기 후 퇴장

                    }
                    break;

                // 퇴장 경로 탐색
                case 6:
                    

                    // 문으로 향하기 위한 경로 탐색
                    route = PF.startPathFinding(cur_row, cur_col, dest_row, dest_col);

                    if (route == null)
                    {
                        Debug.Log("퇴장 경로 존재 X");
                        return;
                    }

                    // 경로가 있다면
                    // idx 값 셋팅 및 출발(state = 1)
                    astarIdx = (route.Length / 2) - 1;
                    state = 7;
                    break;
                    
                // 출구로 이동
                case 7:

                    if (!isCoroutineOn)
                    {
                        //경로가 남았다면
                        if (astarIdx >= 0)
                        {
                            // 이동 경로 확인한 후 출발신호
                            dest = FM.field[route[0, astarIdx], route[1, astarIdx]].transform.position;
                            dest.z = FM.field[route[0, astarIdx], route[1, astarIdx]].GetComponent<ArrangeablePosState>().getRowPos();
                        }
                        //경로의 끝에 도달했다면
                        else
                        {
                            state = 8;
                            break;
                        }

                        // 이동 시작
                        Debug.Log("astar start");
                        isCoroutineOn = true;
                        StartCoroutine("moving_astar");
                    }
                    break;

                // 퇴장 ( Destroy )
                case 8:
                    int curFloorIdx = GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx;
                    GameObject curFloor = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[curFloorIdx];
                    curFloor.GetComponent<FieldManager>().guestCount--;

                    Transform tmp = gameObject.transform.Find("CharacterControlCanvas");
                    if (tmp != null)
                    {
                        tmp.SetParent(null);
                        tmp.transform.position = GameObject.Find("OutviewBase").transform.position;
                    }
                    Destroy(gameObject);
                    break;
            }
        }

        // 직원은 가게를 돌아다닌다
        else
        {
            // 낮에는 카운터로 이동
            if (GameObject.Find("GameManager").GetComponent<DayController>().isDaytime)
            {
                switch (state)
                {
                    case 0:
                        destPos = FM.destPosOfCounter(out dest_row, out dest_col, out tmpScaleX,  false);
                        
                        route = PF.startPathFinding(cur_row, cur_col, dest_row, dest_col);
                        if (route == null)
                        {
                            Debug.Log("카운터 경로 존재 X");
                            return;
                        }

                        // 경로가 있다면
                        // idx 값 셋팅 및 출발(state = 1)
                        astarIdx = (route.Length / 2) - 1;
                        state = 1;
                        break;

                    //카운터로 이동중
                    case 1:
                        if (!isCoroutineOn)
                        {
                            //경로가 남았다면
                            if (astarIdx >= 0)
                            {
                                // 이동 경로 확인한 후 출발신호
                                dest = FM.field[route[0, astarIdx], route[1, astarIdx]].transform.position;
                                dest.z = FM.field[route[0, astarIdx], route[1, astarIdx]].GetComponent<ArrangeablePosState>().getRowPos();
                            }
                            //경로의 끝에 도달했다면
                            else
                            {
                                transform.localScale = new Vector3(tmpScaleX, 1, 1);
                                animator.SetBool("isStand", true);
                                state = 2;
                                break;
                            }

                            // 이동 시작
                            Debug.Log("astar start");
                            isCoroutineOn = true;
                            StartCoroutine("moving_astar");
                        }
                        break;

                    case 2:
                        // 도착, 대기
                        //timer += Time.deltaTime;
                        //if (timer >= 2f)
                        //{
                        //    state = 3;
                        //}
                        break;
                }
            }
            // 밤에는 돌아다니기
            else
            {

                switch (state)
                // 0: Stop
                // 1: Walk
                // 2: Sit
                // 3: wait order
                {
                    case 0:
                        //주변 확인 후 출발신호
                        moveSignal();
                        break;

                    case 1:
                        // 이동
                        moving();
                        break;

                    case 2:
                        break;
                    //default:
                    //    state = 0;
                    //    break;
                    //default:
                    //    state = 0;
                    //    break;
                }
            }
        }
    }

    public void setStateToZero()
    {
        // 모든 이동 루틴을 종료하고 자유 이동으로 전환한다
        StopAllCoroutines();
        state = 0;
    }

    void moveSignal()
    {
        //FieldManager에 현 위치 정보 전달
        //FieldManager가 이동할 지점의 좌표 정보를 반환
        dest = FM.CheckAround(gameObject, cur_row, cur_col);

        //해당 위치로 이동을 시작
        state = 1; 
    }

    void moving()
    {
        StartCoroutine("moving_c");
        state = 3;
    }
    
    // 서성이기 동작
    IEnumerator moving_c()
    {
        // 제자리 대기 신호
        if (dest == Vector3.zero)
        {
            float t = 0;
            while (true)
            {
                t += Time.deltaTime;
                if (t > 1.0f)
                {
                    state = 0;
                    StopCoroutine("moving_c");
                }
                yield return null;
            }
        }

        // 이동 신호
        else
        {
            // 이동할 거리
            Vector3 distance = dest - transform.position;
            Vector3 moved_distance = Vector3.zero;
            Vector3 tmp;
            while (true)
            {
                tmp = distance * Time.deltaTime;
                //transform.position += tmp;
                transform.Translate(tmp);
                moved_distance += tmp;
                
                // 도착
                if (moved_distance.sqrMagnitude > distance.sqrMagnitude)
                {
                    state = 0;
                    StopCoroutine("moving_c");

                    break;
                }

                yield return null;
            }
        }
    }

    // A* 동작
    IEnumerator moving_astar()
    {   
        // 이동할 거리(및 방향?)
        Vector3 distance = dest - transform.position;
        Vector3 moved_distance = Vector3.zero;
        Vector3 tmp;

        // 위치 갱신(anim flip)
        setCurPos(route[0, astarIdx], route[1, astarIdx]);

        while (true)
        {
            
            tmp = distance * Time.deltaTime;
            if (isEmployee) tmp *= 2; // 직원은 2배속으로 이동(카운터로)
            transform.Translate(tmp);
            moved_distance += tmp;

            // 다음 타일에 도착
            if (moved_distance.sqrMagnitude > distance.sqrMagnitude)
            {
                ////현재 위치 갱신
                //cur_row = route[0, astarIdx];
                //cur_col = route[1, astarIdx];

                // 다음 루트 가리킴
                astarIdx--;

                //state = 0;
                StopCoroutine("movind_astar");
                
                break;
            }

            yield return null;
        }
        // coroutine 호출 종료
        isCoroutineOn = false;
    }

    // state 초기화
    public void resetState()
    {
        state = 0;
    }

    public void setCurPos(int r, int c)
    {
        if (cur_col > c || cur_row < r)
        {
            //GetComponent<SpriteRenderer>().flipX = false;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            //GetComponent<SpriteRenderer>().flipX = true;
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (transform.Find("CharacterControlCanvas") != null)
        {
            transform.Find("CharacterControlCanvas").transform.localScale = new Vector3(transform.localScale.x, 1, 1);
        }

        cur_row = r;
        cur_col = c;

    
    }



}
