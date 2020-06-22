using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/************************************
* 
* 버전: ver 1.6.7
* 작성자: 진민준     최초 작성날짜: 190921      최근 수정날짜: 191126
* 설명:
*   하나의 층을 관리하는 스크립트    
*       가용 필드 unlock
*       이동 경로 탐색
* 
************************************/

/************ 최근 수정 내역 *************
* 
* 벽 오브젝트 rowPos 미세 수정(+0.5)
*   이제 벽 오브젝트가 캐릭터와 겹치지 않습니다.
*  
************************************/

/**

@brief 
    하나의 층을 관리하는 스크립트
      - 가용 필드 unlock
      - 이동 경로 탐색
      .
    
    
@author 진민준

@date 2019-11-26

@version 1.6.7

@details
최초 작성일: 190921

Recently modified list
- 벽 오브젝트 rowPos 미세 수정(+0.5)
    - 이제 벽 오브젝트가 캐릭터와 겹치지 않습니다.
    .
.
    
*/

public class FieldManager : MonoBehaviour
{
    enum FieldState {
        LOCK = -1, PASSABLE, UNPASSABLE, PASSED, TARGET,
    };

    /**
    state of each field's arrangeablePos
    - -1: lock
    - 0: passable (usable)
    - 1: unpassable (unusable)
    - 2: unpassable (passfinding에 사용한 후 0으로 되돌린다)
    - 3: enterance (in)
    - 4: sitable (nobody sit on this chair)
    - 5: unsitable (somebody sit on this chair)
    - 6: table */
    public GameObject[,] field;

    int[,] fieldState;

    [SerializeField]
    Sprite ruinChairSprite;
    [SerializeField]
    Sprite ruinTableSprite;

    public GameObject[,] leftWall; // SetActive(false): lock
    public GameObject[,] rightWall; // SetActive(true): unlock
                                

    int[,] fieldInt;

    public int chairCount; // 배치된 의자 수만큼 손님 입장 가능
    public int guestCount; // 그 층의 손님 수

    public GameObject floorArranger; // Prefab
    public GameObject LWArranger; // Left Wall
    public GameObject RWArranger; // Right Wall
    //public Sprite basicTileSprite; // 기본 타입 스프라이트
    public GameObject basicTile; // 기본 타입 바닥 오브젝트

    //[SerializeField]
    //GameObject enterance; // 입구 오브젝트


    const int START_ROW = 8, START_COL = 8; // 시작 사이즈 (단위: 타일 갯수)
    public const int MAX_ROW = 16, MAX_COL = 16; // 최대 사이즈
    const int UNLOCK_RANGE = 4; //필드 업그레이드시 어느만큼 unlock 해줄것인지 
    
    public int CUR_ROW { get; set; }
    public int CUR_COL { get; set; } // 현재 Unlock 된 필드 행렬 사이즈
    

    public int thisFloor; // 현재 층
    public int curYPos; // 현재 층이 위치할 Y 좌표
    int curUpgrade; // 업그레이드 단계

    float time;


    void Awake()
    {
        time = 0f;

        // 시작 맵 셋팅
        CUR_ROW = START_ROW; CUR_COL = START_COL;
        curUpgrade = 1; // 업그레이드 단계

        chairCount = 0; // 의자 수
        guestCount = 0; // 손님 수

        // 각 필드(arrangeablePos)에 상태 초기값 부여
        field = new GameObject[MAX_ROW, MAX_COL];
        leftWall = new GameObject[MAX_ROW, 2];
        rightWall = new GameObject[2, MAX_COL];

        for (int x = 0; x < MAX_ROW; x++)
        {
            for (int y = 0; y < MAX_COL; y++)
            {
                // LeftWall
                if (y == 0){
                    leftWall[x, 0] = Instantiate(LWArranger, new Vector3(-x * (float)0.64 - 0.32f + 0.02f*x + 0.01f, -x * (float)0.32 + gameObject.transform.position.y + 0.48f, 20), transform.rotation);
                    leftWall[x, 0].transform.parent = gameObject.transform;
                    leftWall[x, 0].GetComponent<ArrangeablePosState>().setState(19.5f - x * 0.5f, 0, true);
                    leftWall[x, 0].SetActive(false);
                    //leftWall[x, 0].GetComponent<ArrangeablePosState>().setState(19 - x - y * (float)0.5, -1);

                    leftWall[x, 1] = Instantiate(LWArranger, new Vector3(-x * (float)0.64 - 0.32f + 0.02f*x + 0.01f, (-x + 2) * (float)0.32 + gameObject.transform.position.y + 0.48f - 0.01f, 20), transform.rotation);
                    leftWall[x, 1].transform.parent = gameObject.transform;
                    leftWall[x, 1].GetComponent<ArrangeablePosState>().setState(19.5f - x * 0.5f, 0, true);
                    leftWall[x, 1].SetActive(false);
                    //leftWall[x, 1].GetComponent<ArrangeablePosState>().setState(19 - x - y * (float)0.5, -1);
                }
                
                // RightWall (x가 0일 때 한 차례만 작동)
                if(x == 0) {
                    rightWall[0, y] = Instantiate(RWArranger, new Vector3(y * (float)0.64 + 0.32f - 0.02f*y - 0.01f, -y * (float)0.32 + gameObject.transform.position.y + 0.48f, 20), transform.rotation);
                    rightWall[0, y].transform.parent = gameObject.transform;
                    rightWall[0, y].GetComponent<ArrangeablePosState>().setState(19.5f - y * 0.5f, 0, false);
                    rightWall[0, y].SetActive(false);
                    //rightWall[0, y].GetComponent<ArrangeablePosState>().setState(19 - x - y * (float)0.5, -1);

                    rightWall[1, y] = Instantiate(RWArranger, new Vector3(y * (float)0.64+0.32f - 0.02f*y - 0.01f, (-y + 2) * (float)0.32 + gameObject.transform.position.y + 0.48f-0.01f, 20), transform.rotation);
                    rightWall[1, y].transform.parent = gameObject.transform;
                    rightWall[1, y].GetComponent<ArrangeablePosState>().setState(19.5f - y * 0.5f, 0, false);
                    rightWall[1, y].SetActive(false);
                    //rightWall[1, y].GetComponent<ArrangeablePosState>().setState(19 - x - y * (float)0.5, -1);
                }
                
                // Floor
                field[x, y] = Instantiate(floorArranger, new Vector3(-x * (float)0.64 + y * (float)0.64 + x *0.02f - y*0.02f, (-x * (float)0.32 + 0 - y * (float)0.32) + gameObject.transform.position.y , 20), transform.rotation);
                field[x, y].transform.parent = gameObject.transform;
                field[x, y].GetComponent<ArrangeablePosState>().setState(19 - x * (float)0.5 - y * (float)0.5, -1);
                field[x, y].GetComponent<ArrangeablePosState>().setPos(x, y);
                // defaultLayer
                // locked Position
                field[x, y].gameObject.layer = 0;
            }
        }

        // 최소 영역 활성화
        for (int x = 0; x < CUR_ROW; x++)
        {
            for (int y = 0; y < CUR_COL; y++)
            {
                if (y == 0)
                {
                    // LeftWall
                    leftWall[x, 0].SetActive(true);
                    leftWall[x, 1].SetActive(true);
                }

                if (x == 0)
                {
                    // RightWall
                    rightWall[0, y].SetActive(true);
                    rightWall[1, y].SetActive(true);
                }

                // Floor
                field[x, y].GetComponent<ArrangeablePosState>().setState(0); // 활성화
                //field[x, y].GetComponent<SpriteRenderer>().sprite = basicTileSprite;
                field[x, y].GetComponent<SpriteRenderer>().sprite = basicTile.GetComponent<SpriteRenderer>().sprite;

                field[x, y].gameObject.layer = 8; // ArrangeablePos
            }
        }

        // 기본 오브젝트 배치?
        // 입구, 계산대, 등
        // 입구 배치
        field[0, 4].GetComponent<ArrangeablePosState>().setState(3);
        field[0, 4].GetComponent<SpriteRenderer>().color = new Color(200, 0, 0);


        // 기본 책상 배치
        // x, y, 는 배치할 위치
        //  [process]
        //  0.기본 오브젝트 Sprite 별도 저장
        //  1.field[x, y] 의 좌표에 오브젝트 instantiate 및 좌표 설정(z축 rowPos로 밀어넣기)
        //    1-1.해당 오브젝트 프리팹을 그 층의 자식으로 설정
        //  2.field[x, y]의 State와 PutHere를 설정
        //    2-1.State: table(6), chair(4), other(1)
        //  3.해당 오브젝트 프리팹의 putWhere를 field[x, y]로 설정

        { // 책상 및 의자
          // 책상 선배치 후 의자 후배치 할 것
            setBaseObject(5, 2, (int)ItemController._itemType.Table);
            setBaseObject(5, 1, (int)ItemController._itemType.Chair);
            setBaseObject(4, 2, (int)ItemController._itemType.Chair);

            setBaseObject(5, 5, (int)ItemController._itemType.Table);
            setBaseObject(5, 4, (int)ItemController._itemType.Chair);
            setBaseObject(4, 5, (int)ItemController._itemType.Chair);

            // 계산대
            setBaseObject(1, 1, (int)ItemController._itemType.Counter);
        }
    }

    private void Update()
    {
        if (thisFloor == 1)
        {
            // 3~4초마다 손님 입장
            time += Time.deltaTime;
            if (time > 4f)
            {
                time = Random.Range(0, 1f);
                Debug.Log("손님 입장");
                GameObject.Find("GameManager").GetComponent<CharacterManager>().CharacterEmployer(1);
            }
        }
    }

    void setBaseObject(int x, int y, int select)
    {
        GameObject tmpObj = Instantiate(GameObject.Find("MenuCanvas").GetComponent<MenuUIController>().ObjectsPrefab[select], new Vector3(field[x, y].transform.position.x, field[x, y].transform.position.y, field[x, y].GetComponent<ArrangeablePosState>().getRowPos()), Quaternion.identity, this.transform);
        field[x, y].GetComponent<ArrangeablePosState>().setState(6);
        field[x, y].GetComponent<ArrangeablePosState>().putHere = tmpObj;
        tmpObj.GetComponent<FurnitureManager>().putWhere = field[x, y];
        tmpObj.GetComponent<FurnitureManager>().newPos = field[x, y].GetComponent<ArrangeablePosState>();

        // 의자라면, 정보 갱신 및 책상과의 관계 설정
        if ((int)ItemController._itemType.Chair == select)
        {
            field[x, y].GetComponent<ArrangeablePosState>().setState(4); // sitable

            // 의자를 놓은 좌표 주변에 책상이 있는지 확인
            // 좌하단에 책상
            if (field[x + 1, y].GetComponent<ArrangeablePosState>().getState() == 6)
            {
                // 그 책상을 바라봄
                tmpObj.GetComponent<ChairSurporter>().setTable(field[x + 1, y].GetComponent<ArrangeablePosState>().putHere);

                tmpObj.transform.localScale = new Vector3(-1, 1, 1);
            }
            // 우하단에 책상
            else if (field[x, y + 1].GetComponent<ArrangeablePosState>().getState() == 6)
            {
                // 그 책상을 바라봄
                tmpObj.GetComponent<ChairSurporter>().setTable(field[x, y + 1].GetComponent<ArrangeablePosState>().putHere);

                tmpObj.transform.localScale = new Vector3(1, 1, 1);
            }
            // 책상이 없으면
            else
            {
                tmpObj.GetComponent<ChairSurporter>().setTable(null);
            }

            // 해당 좌표를 sitable(pathfind의 목적지) 로 설정
            //tmpObj.GetComponent<FurnitureManager>().putWhere.GetComponent<ArrangeablePosState>().setState(4);

            // 해당 층에 의자 개수 1 추가
            chairCount++;

        }
    }

    public void instantiater(int thisFloor)
    {
        this.thisFloor = thisFloor;
        this.curYPos = (thisFloor-1) * 15;
    }

    // Main Camera의 위치를 옮기기 위한 getter
    int getCurYPos()
    {
        return curYPos;
    }

    int getCurUpgrade()
    {
        return curUpgrade;
    }


    // 주변 랜덤이동
    // 1_ 주변 탐색
    // 인자로 캐릭터의 현재 row, column을 얻는다
    public Vector3 CheckAround(GameObject character, int r, int c)
    {
        int way = -1;

        // 이동 방향이 결정되어야 escape 되는 infinity loop
        bool escapeFlag = false;
        while (!escapeFlag)
        {
            way = Random.Range(0, 9); // 0~8
            switch (way)
            // 0: up
            // 1: down
            // 2: left
            // 3: right
            // 4~8: stay
            {
                // 해당 경로로 진행할 수 있다면 루프 탈출(이동경로 재탐색 진행 X)
                case 0: // up
                    if (!(r - 1 < 0) && field[r - 1, c].GetComponent<ArrangeablePosState>().getState() == 0)
                    {
                        //escapeFlag = true;
                        Vector3 tmp = field[r - 1, c].transform.position;
                        tmp.z = field[r - 1, c].GetComponent<ArrangeablePosState>().getRowPos();
                        character.GetComponent<CharacterController>().setCurPos(r - 1, c);
                        return tmp;
                    }
                    break;
                case 1: // down
                    if (!(r + 1 >= MAX_ROW) && field[r + 1, c].GetComponent<ArrangeablePosState>().getState() == 0)
                    {
                        Vector3 tmp = field[r + 1, c].transform.position;
                        tmp.z = field[r + 1, c].GetComponent<ArrangeablePosState>().getRowPos();
                        character.GetComponent<CharacterController>().setCurPos(r + 1, c);
                        return tmp;
                    }
                    break;
                case 2: // left
                    if (!(c - 1 < 0) && field[r, c-1].GetComponent<ArrangeablePosState>().getState() == 0)
                    {
                        Vector3 tmp = field[r, c-1].transform.position;
                        tmp.z = field[r, c-1].GetComponent<ArrangeablePosState>().getRowPos();
                        character.GetComponent<CharacterController>().setCurPos(r, c - 1);
                        return tmp;
                    }
                    break;
                case 3: // right
                    if (!(c + 1 >= MAX_COL) && field[r, c+1].GetComponent<ArrangeablePosState>().getState() == 0)
                    {
                        Vector3 tmp = field[r, c+1].transform.position;
                        tmp.z = field[r, c+1].GetComponent<ArrangeablePosState>().getRowPos();
                        character.GetComponent<CharacterController>().setCurPos(r, c+1);
                        return tmp;
                    }
                    break;
                default: //4~8
                    escapeFlag = true;
                    break;
            }
        }
        return Vector3.zero;
    }

    // 카운터의 좌표를 반환
    // 직원/손님 여부에 따라 카운터의 안쪽/바깥 좌표를 달리 반환
    public GameObject destPosOfCounter(out int dest_row, out int dest_col, out int scaleX,  bool isGuest)
    {
        // 카운터 탐색
        for(int x = 0; x < CUR_ROW; x++)
        {
            for (int y = 0; y < CUR_COL; y++)
            {
                if(field[x, y].GetComponent<ArrangeablePosState>().putHere!=null && field[x, y].GetComponent<ArrangeablePosState>().putHere.CompareTag("Counter"))
                {
                    //카운터의 방향을 확인해서 상하단/좌우 어디로 도착하게 할지 결정
                    // scale.x == 1
                    // 직원: 좌상단 , 손님: 우하단
                    // scale.x == -1
                    // 직원: 우상단 , 손님: 좌하단
                    // 좌표 및 도착지점 반환
                    if(field[x, y].GetComponent<ArrangeablePosState>().putHere.transform.localScale.x == 1)
                    {
                        if (isGuest) { dest_row = x; dest_col = y + 1; scaleX = 1; return field[x, y].gameObject; }
                        else { dest_row = x; dest_col = y - 1; scaleX = -1; return field[x, y].gameObject; }
                    }
                    else if (field[x, y].GetComponent<ArrangeablePosState>().putHere.transform.localScale.x == -1)
                    {
                        if (isGuest) { dest_row = x + 1; dest_col = y; scaleX = -1; return field[x, y].gameObject; }
                        else { dest_row = x - 1; dest_col = y; scaleX = 1; return field[x, y].gameObject; }
                    }

                }
            }
        }


        // 이 구문에 닿는 경우 버그
        dest_row = -1; dest_col = -1; scaleX = -1;
        return null;
    }


    // 랜덤한 의자(앉을 수 있는)의 좌표를 반환
    // myTable이 있는 의자에 한해서 탐색
    // 그 의자의 Pos(ArrangeablePos) GameObject를 반환
    public GameObject destPosOfChair(out int dest_row, out int dest_col)
    {
        List<ArrangeablePosState> chairsList = new List<ArrangeablePosState>();
        for(int x = 0; x < CUR_ROW; x++)
        {
            for(int y = 0; y < CUR_COL; y++)
            {
                // 앉을 수 있는 의자라면 List에 저장.
                //if(field[x, y].GetComponent<ArrangeablePosState>().getState() == 4)

                //무언가 놓여져있는 타일이고, 그게 의자라면
                if (field[x, y].GetComponent<ArrangeablePosState>().putHere!=null && field[x, y].GetComponent<ArrangeablePosState>().putHere.CompareTag("Chair"))
                {// 테이블이 있는 의자라면
                    if (field[x, y].GetComponent<ArrangeablePosState>().putHere.GetComponent<ChairSurporter>().getTable() != null)
                        chairsList.Add(field[x, y].GetComponent<ArrangeablePosState>());
                }
            }
        }

        // 의자가 없는 경우 
        if (chairsList.Count == 0)
        {
            dest_row = -1; dest_col = -1;
            return null;
        }
        
        // 목록 중 하나의 인덱스 셋
        int idx = Random.Range(0, chairsList.Count);

        for (int x = 0; x < CUR_ROW; x++)
        {
            for (int y = 0; y < CUR_ROW; y++)
            {
                //if (field[x, y].GetComponent<ArrangeablePosState>().getState() == 4)
                if (field[x, y].GetComponent<ArrangeablePosState>() == chairsList[idx])
                {
                    dest_row = x; dest_col = y;

                    //그 좌표를 남이 앉을 수 없는 자리로 설정(state = 5)
                    //chairsList[idx].setState(5);

                    return chairsList[idx].gameObject;
                }
            }
        }
        //이 구문에 닿는 경우 : 버그
        dest_row = -1; dest_col = -1;
        return null;
    }
   
    // 출구의 좌표를 반환
    public void destPosOfExitDoor(out int dest_row, out int dest_col)
    {
        for(int x = 0; x < CUR_ROW; x++)
        {
            for(int y = 0; y < CUR_COL; y++)
            {
                // 출구를 찾아서 좌표(idx) 반환
                if(field[x, y].GetComponent<ArrangeablePosState>().getState() == 3)
                {
                    dest_row = x; dest_col = y;
                    return;
                }
            }
        }

        // 이 구문까지 닿는 경우 = 버그
        dest_row = -1; dest_col = -1;
    }

    public int getFieldUpgrade()
    {
        return curUpgrade;
    }

    // lock 되어있는 타일들을 unlock 하는 함수
    public void fieldUpgrade(int max)
    {
        if(!(curUpgrade < max))
        {
            Debug.Log("더이상 업그레이드 할 수 없습니다");
            return;
        }

        curUpgrade = curUpgrade + 1;
        CUR_ROW = CUR_ROW + 4;
        CUR_COL = CUR_COL + 4;

        
        // Ungrade 영역 활성화
        for (int x = 0; x < CUR_ROW; x++)
        {
            for (int y = 0; y < CUR_COL; y++)
            {
                if (y == 0)
                {
                    // LeftWall
                    if (!leftWall[x, 0].activeSelf)
                        leftWall[x, 0].SetActive(true);
                    if (!leftWall[x, 1].activeSelf)
                        leftWall[x, 1].SetActive(true);
                }

                if (x == 0)
                {
                    // RightWall
                    if (!rightWall[0, y].activeSelf)
                        rightWall[0, y].SetActive(true);
                    if (!rightWall[1, y].activeSelf)
                        rightWall[1, y].SetActive(true);
                }
                
                // Floor
                // locked pos 라면 활성화.
                if (field[x, y].GetComponent<ArrangeablePosState>().getState() == -1)
                {
                    field[x, y].GetComponent<ArrangeablePosState>().setState(0); // 활성화
                    //field[x, y].GetComponent<SpriteRenderer>().sprite = basicTileSprite;
                    field[x, y].GetComponent<SpriteRenderer>().sprite = basicTile.GetComponent<SpriteRenderer>().sprite;
                    field[x, y].gameObject.layer = 8; // ArrangeablePos
                }
            }
        }

        //// 신규 입구 설정
        //field[CUR_ROW - 4, CUR_COL - 1].GetComponent<ArrangeablePosState>().setState(3);
        //field[CUR_ROW - 4, CUR_COL - 1].GetComponent<SpriteRenderer>().color = new Color(200, 0, 0);
    }
    
    public void rendererController(bool On)
    {
        // true: renderer.enable = true
        // false: renderer.enable = false
        
        foreach(var r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = On;
        }
    }
}
