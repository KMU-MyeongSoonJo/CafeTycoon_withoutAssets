using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**

@brief
    가구의 배치와 관련된 모든 기능들이 포함된 스크립트\n
    마우스 클릭, 오브젝트 선택 배치 회전 삭제, 정상/비정상 배치 처리, 오브젝트 하이라이팅 등이 포함된다

@author 진민준

@date 2020-04-05

@version 2.3.5

@details
최초 작성일: 190914

Recently modified list
- 타일 설치시 해당 칸(ArrangeablePosState)에 타일의 시리얼 정보를 저장합니다.

*/

public class FloorArranger : MonoBehaviour
{
    // Screen Shader가 적용된 object highlighting Material
    Material blueHighlightMaterial; // Correct Arrange : blue light
    Material redHighlightMaterial; // Incorrect Arrange : red light
    Material defaultMaterial; // defaultMaterial. 아무 오브젝트로부터 추출

    [SerializeField]
    GameObject target; // 배치할 오브젝트
    GameObject target2; // 배치될 위치 (타일, ArrangeablePos)
    public GameObject tmp; // 배치 중 바탁 타일의 하이라이팅을 지우기 위한 임시 변수
    Canvas objectControlTap; // 오브젝트 편집 탭
    Image objectControlTap_Spin; // 편집 탭의 회전 버튼. 사진 색 변경 및 raycastTarget을 비활성화.

    Vector3 originPos; // 배치중인 오브젝트의 기존 위치
    GameObject originTile; // 배치중 오브젝트의 기존 타일

    // 책상 위치를 옮기는 경우
    // 재배치 시작과 함께 기존 의자정보 저장
    // 위치가 변경된 경우 기존 의자들에게서 책상 정보 삭제
    GameObject tmpChairLeft; // 좌상단
    GameObject tmpChairRight; // 우상단

    bool isControl; // 오브젝트 편집 중 ? true : false
    bool isRotate; // 오브젝트 편집 -> 회전 중
    public bool isMove; // 배치 중 ? true : false
    [SerializeField]
    byte arrangeFlag;   // 0: 정상배치
                        // 1: 비정상 배치. isByButton ? go to originPos : Destroy
                        // 2: 타일 및 벽지 배치

    bool isByButton;// 버튼 클릭을 통해 배치되는 오브젝트를 판단하기위한 플래그
   // bool isWrongMove; // 비정상 배치를 체크하는 플래그
                      
         
    private void Awake()
    {
        arrangeFlag = 99; // null value
        isMove = false; // 오브젝트 재배치를 컨트롤 하기 위한 플래그
        isByButton = false; 
        //arrangeFlag = 0; // 비정상 배치를 판단하기 위한 플래그
        
        isControl = false; // 오브젝트 편집탭 호출 여부
        isRotate = false; // 오브젝트 편집 -> 회전 여부
        objectControlTap = GameObject.Find("ObjectControlCanvas").GetComponent<Canvas>();
        objectControlTap_Spin = GameObject.Find("Spin").GetComponent<Image>();

        /************* 리소스 로딩 *************/
        blueHighlightMaterial = Resources.Load("Materials/CorrectArrangeShader") as Material;
        redHighlightMaterial = Resources.Load("Materials/IncorrectArrangeShader") as Material;
        defaultMaterial = Resources.Load("Materials/DefaultShader") as Material;
    }

    // Update is called once per frame
    void Update()
    {
        // 타일 및 벽지 배치
        if(arrangeFlag == 2)
        {
            if (Input.GetMouseButton(0))
            {
                objectHighlightEraser(tmp);

                // 타일 교체 및 시리얼 번호 할당
                if (tmp != null)
                {
                    tmp.GetComponent<SpriteRenderer>().sprite = target.GetComponent<TileManager>().getTile();
                    tmp.GetComponent<ArrangeablePosState>().setTile(target.GetComponent<SerialNumbManager>().getSerialNumb());
                }
            }
        }

        
        // 오브젝트 직접 클릭을 통한 재배치
        if (arrangeFlag != 2 && Input.GetMouseButtonUp(0))
        {
            // 편집 중이었다면 -> 편집 취소
            // 편집 + 배치중이었다면 -> 배치 취소
            if (isMove)
            {
                // 배치 정상/비정상 여부 판단
                switch (arrangeFlag)
                {
                    /**** 정상 종료 ****/
                    case 0:
                        arrangeFlag = 99; // null

                        // 해당 오브젝트에 SerialNumb 부여

                        // 해당 필드의 자식으로 설정
                        target.transform.parent = getCurFloor().transform;
                        
                        isByButton = false;

                        // 오브젝트와 바닥 타일(tmp)의 하이라이팅 제거
                        objectHighlightEraser(target);
                        objectHighlightEraser(tmp);

                        isMove = false;

                        //새 위치에 배치되었음을 알림
                        // 인자로 자신이 위치한 타일(Pos)을 전달
                        // setState()가 이루어짐. 특수한 배치의 경우(책상, 의자 등) 이후 재설정 과정 발생
                        target.GetComponent<FurnitureManager>().moveEnd(tmp);

                        if (target.CompareTag("Chair"))
                        {
                            chairCheck(target, tmp);
                        }
                        else if (target.CompareTag("Table"))
                        {
                            tableCheck(target, tmp);
                        }
                        // 기존 타일에서 오브젝트 정보 삭제
                        if (originTile != null)
                        {
                            originTile.GetComponent<ArrangeablePosState>().putHere = null;
                            originTile = null;
                        }

                        // 해당 arrangeablePos에 target정보 전달
                        if (tmp.GetComponent<ArrangeablePosState>() != null)
                            tmp.GetComponent<ArrangeablePosState>().putHere = target;
                        // 해당 target에 arrangeablePos 정보 전달
                        target.GetComponent<FurnitureManager>().putWhere = tmp;

                        CallObjectController(false);
                        target = null;

                        break;

                    /**** 비정상 종료 ****/
                    case 1:
                        // 배치 취소
                        cancelReplace();

                        break;

                }
            }
            // 편집 탭 종료
            // 회전 중이었다면 편집을 종료하지 않는다.
            else if (isControl)
            {
                if (!isRotate)
                {
                    CallObjectController(false);
                    objectHighlightEraser(target);
                    objectHighlightEraser(tmp);
                    target = null;
                }
                isRotate = false;

            }
            else // 클릭 로직 수행. 배치를 시작할 수 있는지 확인
            {
                GameObject tmp = GetClickedObject(); // 커서가 가리키는 지점에 위치한 오브젝트를 target으로 지정
                if (tmp == null) return;
                if (target != tmp) // target에 대해 올바른 클릭이라면 재배치 시작
                {
                    target = tmp;

                    // 자신 및 child 오브젝트들 까지 하이라이트 적용되는 함수 호출
                    objectHighlighter(target, true);

                    // target의 기존 위치 저장
                    originPos = target.transform.position;
                    //beforeTarget = tmp;

                    CallObjectController(true);
                }
            }
        }
        // 우클릭 입력
        else if (Input.GetMouseButtonUp(1))
        {
            cancelReplace();
        }
        
        // 오브젝트 이동 중
        if (isMove)
        {

            // 재배치 진행 중
            MoveFurniture(target);
        }
    }

    // 배치 로직
    void MoveFurniture(GameObject target)
    {
        int layerMask = 1;
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit;

        // 타일이라면, ArrangeablePos 레이어에 대해서(오브젝트 제외)만 배치?
        if (target.CompareTag("Tile"))
        {
            // Layer : Tile_Floor
            //hit = Physics2D.Raycast(pos, Vector2.zero, 0f, layerMask = (1 << 8) | (1 << 10) | (1 << 11));
            hit = Physics2D.Raycast(pos, Vector2.zero, 0f, layerMask = (1 << 8));
        }

        // 레이어가 Furniture_onTop 이라면
        // 테이블 위에만 놓을 수 있는 오브젝트라면
        // StackPos 레이어(15)에 대해서만 배치
        else if(target.layer == 14)
        {
            hit = Physics2D.Raycast(pos, Vector2.zero, 0f, layerMask = (1 << 15));
        }
        // 오브젝트라면 스택포인트(테이블 위)를 제외한 모든 레이어에 대해 배치?
        else
        {
            // Layer: ArrangeablePos , Furniture(stack)에 대해서만 활성화
            //hit = Physics2D.Raycast(pos, Vector2.zero, 0f, layerMask = (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11));
            hit = Physics2D.Raycast(pos, Vector2.zero, 0f, layerMask = (1 << 8) | (1<<9) | (1<<10));
        }


        // hit : 배치 될 위치의 오브젝트 정보
        // 정상 배치 가능한 경우
        //   마우스가 가리키는 오브젝트가 null이 아니고, 그 오브젝트가 자기 자신이 아닐 경우
        if (hit.collider != null  && hit.collider.gameObject != target.gameObject)
        {
            // 바닥에 놓는 경우
            if (hit.collider.CompareTag("Arrangeable_Ground"))
            {
                bool tmpBool = false;

                // target이 의자라면
                // field에서 idx가 x+1 혹은 y+1이 Table이어야 배치 가능
                if (target.CompareTag("Chair"))
                {
                    // 놓으려는 타일의 좌표
                    int x = hit.collider.GetComponent<ArrangeablePosState>().x;
                    int y = hit.collider.GetComponent<ArrangeablePosState>().y;

                    // 그 위치 주변에 책상이 있는지 확인
                    //int curFloorIdx = GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx;
                    //FieldManager curField = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[curFloorIdx].GetComponent<FieldManager>();
                    FieldManager curField = getCurFloor().GetComponent<FieldManager>();

                    if (curField.field[x, y].GetComponent<ArrangeablePosState>().getState() == 0)
                    {
                        if (curField.field[x + 1, y].GetComponent<ArrangeablePosState>().getState() == 6)
                        {
                            tmpBool = true;
                        }
                        else if (curField.field[x, y + 1].GetComponent<ArrangeablePosState>().getState() == 6)
                        {
                            tmpBool = true;
                        }
                    }
                }

                // 지금 선택된 오브젝트가 바닥에 놓을 수 있는 오브젝트인지 확인
                // 그 자리가 배치 가능한 자리인지 확인
                // 타일이고 그 자리가 입구가 아니라면
                else if (target.layer == 11 && !(hit.collider.GetComponent<ArrangeablePosState>().getState() == 3))
                { // 입구가 아닌 경우에 대해 배치 가능
                    tmpBool = true;
                }
                // 오브젝트라면 배치 가능한(비어있는) 바닥에 대해서 배치 가능
                else if (target.layer == 9 && hit.collider.GetComponent<ArrangeablePosState>().getState() == 0)
                {
                    tmpBool = true;
                }


                // 모두 true라면 배치 가능(tmpBool = true)

                //bool tmpBool = target.layer == 9 ? true : false;
                

                // 하이라이팅
                {
                    objectHighlighter(target, tmpBool);

                    // 마우스가 직전에 어느 타일도 가리켰던 적이 없다면 (배치의 시작 시)
                    if (tmp == null)
                    {
                        tmp = hit.collider.gameObject;
                        objectHighlighter(tmp, tmpBool);
                    }
                    // 마우스가 배치할 필드를 가리키는 작업이 진행중이라면
                    else
                    {
                        // 배치할 타일이 변경된 경우?
                        if (tmp != hit.collider.gameObject)
                        {
                            // 직전 가리키던 타일의 하이라이트 제거
                            objectHighlightEraser(tmp);

                            // 지금 가리키는 타일 저장 및 하이라이팅
                            tmp = hit.collider.gameObject;
                            objectHighlighter(tmp, tmpBool);
                        }
                    }
                }

                target2 = hit.collider.gameObject;
                target.transform.parent = null;
                target.transform.position = new Vector3(target2.transform.position.x, target2.transform.position.y, target2.GetComponent<ArrangeablePosState>().getRowPos());


                // 정상 배치가 완료됨을 알려줌
                // 오브젝트였다면 플래그값 0
                // 타일이었다면 플래그값 2
                if (tmpBool)
                    arrangeFlag = (target.CompareTag("Tile")) ? (byte)2 : (byte)0;
                //arrangeFlag = (target.layer == 9) ? (byte)0 : (byte)2; 
                else
                    arrangeFlag = 1;
                
            }

            // 벽에 배치하는 경우
            else if (hit.collider.CompareTag("Arrangeable_Wall"))
            {

                bool tmpBool = false;

                // 그 오브젝트가 벽에 걸 수 있는 오브젝트인지 확인
                // 그 자리가 배치 가능한 자리인지 확인

                // 타일이라면
                if (target.layer == 13)
                {
                    tmpBool = true;
                }// 벽걸이형 오브젝트라면 비어있는 경우에 한해 배치 가능
                else if (target.layer == 10 && hit.collider.GetComponent<ArrangeablePosState>().getState() == 0)
                {
                    tmpBool = true;
                }


                // 하이라이팅
                {
                    objectHighlighter(target, tmpBool);

                    // 마우스가 직전에 어느 타일도 가리켰던 적이 없다면 (배치의 시작 시)
                    if (tmp == null)
                    {
                        tmp = hit.collider.gameObject;
                        objectHighlighter(tmp, tmpBool);
                    }
                    // 마우스가 배치할 필드를 가리키는 작업이 진행중이라면
                    else
                    {
                        // 배치할 타일이 변경된 경우?
                        if (tmp != hit.collider.gameObject)
                        {
                            // 직전 가리키던 타일의 하이라이트 제거
                            objectHighlightEraser(tmp);

                            // 지금 가리키는 타일 저장 및 하이라이팅
                            tmp = hit.collider.gameObject;
                            objectHighlighter(tmp, tmpBool);
                        }
                    }
                }

                target2 = hit.collider.gameObject;
                target.transform.parent = null;
                target.transform.position = new Vector3(target2.transform.position.x, target2.transform.position.y, target2.GetComponent<ArrangeablePosState>().getRowPos());

                //벽 방향 따라 좌/우 자동반전
                //true: left  ,  false: right
                // 벽 오브젝트에 대해서만 자동반전 기능 작동
                if (target.layer == 10)
                {
                    if (hit.collider.GetComponent<ArrangeablePosState>().getIfWall())
                    {
                        //target.GetComponent<SpriteRenderer>().flipX = false;
                        target.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    }
                    else
                    {
                        //target.GetComponent<SpriteRenderer>().flipX = true;
                        target.GetComponent<Transform>().localScale = new Vector3(-1, 1, 1);
                    }
                }
                // 정상 배치 여부 플래그 관리
                // 오브젝트였다면 플래그값 0
                // 타일이었다면 플래그값 2
                if (tmpBool)
                    //arrangeFlag = (target.layer == 10) ? (byte)0 : (byte)2;
                    arrangeFlag = target.CompareTag("Tile") ? (byte)2 : (byte)0;
                else
                    arrangeFlag = 1;
            }


            // 오브젝트 위에 쌓는 경우
            // 자신의 위에 Stack하지 못하게 한다
            //else if (hit.collider.CompareTag("Arrangeable_Stack") && hit.collider.gameObject.transform.parent != target.transform)
            else if (hit.collider.gameObject.layer == 15 && hit.collider.gameObject.transform.parent != target.transform)
            {
                // 오브젝트 위에 타일을 올리지 못하게 한다.
                if (target.CompareTag("Tile")) return;

                // 오브젝트 위에 놓을 수 있는 오브젝트인지 확인
                bool tmpBool = target.layer == 14 ? true : false;

                // 하이라이팅
                {
                    objectHighlighter(target, tmpBool);

                    // 마우스가 직전에 어느 타일도 가리켰던 적이 없다면 (배치의 시작 시)
                    if (tmp == null)
                    {
                        tmp = hit.collider.gameObject;
                        objectHighlighter(tmp, tmpBool);
                    }
                    // 마우스가 배치할 필드를 가리키는 작업이 진행중이라면
                    else
                    {
                        // 배치할 타일이 변경된 경우?
                        if (tmp != hit.collider.gameObject)
                        {
                            // 직전 가리키던 타일의 하이라이트 제거
                            objectHighlightEraser(tmp);

                            // 지금 가리키는 타일 저장 및 하이라이팅
                            tmp = hit.collider.gameObject;
                            objectHighlighter(tmp, tmpBool);
                        }
                    }
                }
                target2 = hit.collider.gameObject;
                target.transform.parent = target2.transform.parent; 
                target.transform.position = new Vector3(target2.transform.position.x, target2.transform.position.y, target2.transform.parent.transform.position.z-0.0001f);

                if (tmpBool)
                    arrangeFlag = 0; // 정상 배치가 완료됨을 알려줌
                else
                    arrangeFlag = 1;
            }
        }
        // 정상 배치 불가능한 경우
        else
        {

            // 선택된 오브젝트가 커서에 위치하게 하는 코드
            target.transform.position = pos;
            arrangeFlag = 1; // 비정상 배치

            // incorrect highlight
            objectHighlighter(target, false);
        }
    }

    // (재)배치를 시작할 오브젝트를 선택하는 함수
        // Furniture의 arrangePoint에 대해서만
        // Furnitre tag 에 대해서만 gameObject return
    GameObject GetClickedObject()
    {
        //낮에는 오브젝트 선택 불가
        if (GameObject.Find("GameManager").GetComponent<DayController>().isDaytime) return null;

        int layerMask = 1;
        GameObject target = null;
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Furniture layer에 대해서만 raycast
        //RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f, layerMask = 1 << 9 | 1 << 14);

        //Furniture layer 및 AI 레이어까지 raycast. ai가 포함되었다면 편집 탭 호출 x
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f, layerMask = 1 << 9 | 1 << 12 | 1 << 14);
        
        // Furniture 레이어에서 무언가 오브젝트가 Click 되었다면
        // + Untouchable 오브젝트가 아니라면(Untouchable Layer)
        // 해당 오브젝트 반환
        //if (hit.collider != null && !hit.collider.CompareTag("Untouchable"))
        

        if (hit.collider != null)
        {
            // ai라면
            if(hit.collider.gameObject.layer == 12)
            {
                return null;
            }

            // untouchable이 아니라면
            else if (hit.collider.gameObject.layer != 14)
            {
                Debug.Log("return some object");
                target = hit.collider.gameObject;
                return target.transform.parent.gameObject;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    // 버튼UI에서 클릭시 오브젝트를 생성하고 배치하기 위한 셋팅 메소드
    public void ButtonClicker(GameObject t = null)
    {
        target = t; 

        isByButton = true;
        arrangeFlag = 2; // 비정상 배치시 Destroy 시키기위한 플래그
        // 가구 배치시
        objectHighlighter(target, true);
        isMove = true;
    }


    // Object highlighter
    // 자식 관계까지 모두 하이라이팅하는 함수
    // GameObject   : 쉐이더를 적용할 오브젝트
    // correct      : 옳은 배치 = blue , 잘못된 배치 = false
    public void objectHighlighter(GameObject target, bool correct)
    {
        if (target == null) return;
        // stackPoint의 경우 SpriteRenderer가 없으므로 target을 parent로 바꾸어줌
        if (target.GetComponent<SpriteRenderer>() == null)
        {
            target = target.transform.parent.gameObject;
        }

        // 하이라이트 적용
        target.GetComponent<SpriteRenderer>().material = (correct == true) ? blueHighlightMaterial : redHighlightMaterial;
        GameObject tmp = target;
        while (tmp.transform.childCount > 2)
        {
            tmp = tmp.transform.GetChild(2).gameObject; // index
            tmp.GetComponent<SpriteRenderer>().material= (correct == true) ? blueHighlightMaterial : redHighlightMaterial;
        }
    }

    // highlighting erase function
    public void objectHighlightEraser(GameObject target = null)
    {
        if (target == null) return;

        // stackPoint의 경우 SpriteRenderer가 없으므로 target을 parent로 바꾸어줌
        //if (target.GetComponent<SpriteRenderer>() == null)
        if(target.CompareTag("Arrangeable_Stack"))
        {
            target = target.transform.parent.gameObject;
        }

        target.GetComponent<SpriteRenderer>().material = defaultMaterial;
        GameObject tmp = target;
        while (tmp.transform.childCount > 2)
        {
            tmp = tmp.transform.GetChild(2).gameObject; // index
            
            tmp.GetComponent<SpriteRenderer>().material = defaultMaterial;
        }
    }

    // 오브젝트 편집 탭 호출
    public void CallObjectController(bool isControlParameter)
    {
        //편집 시작
        if (isControlParameter)
        {
            // 벽에 거는 오브젝트 혹은 의자였다면 회전 기능을 비활성화
            if (target.layer == 10 || target.CompareTag("Chair"))
            {
                // 회전 기능 비활성화
                objectControlTap_Spin.raycastTarget = false;
                //objectControlTap_Spin.color = Color.black;
                objectControlTap_Spin.color = new Color(.3f, .3f, .3f); 
                //objectControlTap_Spin.color = Color.red;
                Debug.Log("회전비활성화");

                //objectControlTap
            }
            else
            {
                // 회전 기능 활성화
                objectControlTap_Spin.raycastTarget = true;
                //objectControlTap_Spin.color = Color.white;
                objectControlTap_Spin.color = new Color(255, 255, 255);
            }
            objectControlTap.transform.position = target.transform.position;
            isControl = isControlParameter;
        }
        //편집 중이었다면 오브젝트 편집 종료
        else if (!isControlParameter)
        {
            objectHighlightEraser(target);
            objectControlTap.transform.position = GameObject.Find("OutviewBase").GetComponent<Transform>().position;
            isControl = isControlParameter;
        }
    
    }

    public void ObjectControl_MOVE()
    {
        // 책상을 옮긴다면, 좌우의 의자에게서 책상 정보를 삭제할 준비.
        // 비정상 배치로 인해 다시 돌아온다면 책상 정보를 삭제하지 않음

        // 타겟이 위치한 타일과 그 좌표
        originTile = target.GetComponent<FurnitureManager>().putWhere;

        if (target.CompareTag("Table"))
        {
            //originTile = target.GetComponent<FurnitureManager>().putWhere;
            //int x = tmp.GetComponent<ArrangeablePosState>().x;
            //int y = tmp.GetComponent<ArrangeablePosState>().y;
            int x = originTile.GetComponent<ArrangeablePosState>().x;
            int y = originTile.GetComponent<ArrangeablePosState>().y;

            // 의자가 있는지 확인
            // 좌상단 
            if (y > 0 && getCurFloor().GetComponent<FieldManager>().field[x, y - 1].GetComponent<ArrangeablePosState>().putHere != null)
            {
                if (getCurFloor().GetComponent<FieldManager>().field[x, y - 1].GetComponent<ArrangeablePosState>().putHere.CompareTag("Chair"))
                {
                    tmpChairLeft = getCurFloor().GetComponent<FieldManager>().field[x, y - 1].GetComponent<ArrangeablePosState>().putHere;
                }

                else tmpChairLeft = null;
            }
            // 우상단
            if (x > 0 && getCurFloor().GetComponent<FieldManager>().field[x - 1, y].GetComponent<ArrangeablePosState>().putHere != null)
            {
                if (getCurFloor().GetComponent<FieldManager>().field[x - 1, y].GetComponent<ArrangeablePosState>().putHere.CompareTag("Chair"))
                {
                    tmpChairRight = getCurFloor().GetComponent<FieldManager>().field[x - 1, y].GetComponent<ArrangeablePosState>().putHere;
                }
                else tmpChairRight = null;
            }
        }

        isMove = true; // 배치 시작
        CallObjectController(false);
    }

    public void ObjectControl_ROTATE()
    {
        target.GetComponent<FurnitureManager>().rotate();
        isRotate = true;

    }

    public void ObjectControl_ROTATE_REMOTE(GameObject obj)
    {
        obj.GetComponent<FurnitureManager>().rotate();
    }

    public void ObjectControl_DELETE()
    {
        // 의자라면, 그 층에 의자 개수 1 빼기
        if (target.CompareTag("Chair"))
        {
            //int curFloorIdx = GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx;
            //GameObject curFloor = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[curFloorIdx];
            getCurFloor().GetComponent<FieldManager>().chairCount--;
            //curFloor.GetComponent<FieldManager>().chairCount--;
        }
        target.GetComponent<FurnitureManager>().destroied();
        Destroy(target);
    }

    public void ObjectControl_DELETE_REMOTE(GameObject obj)
    {
        if (obj == null) return;

        // 의자라면, 그 층에 의자 개수 1 빼기
        if (obj.CompareTag("Chair"))
        {
            //int curFloorIdx = GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx;
            //GameObject curFloor = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[curFloorIdx];
            getCurFloor().GetComponent<FieldManager>().chairCount--;
            //curFloor.GetComponent<FieldManager>().chairCount--;
        }
        obj.GetComponent<FurnitureManager>().destroied();
        Destroy(obj);
    }

    //배치 취소 함수
    public void cancelReplace()
    {
        if (isMove)
        {
            if (isByButton)
            { // Destroy
              //타일에서 자신 정보 삭제
                Destroy(target); // ★★★★★
                isByButton = false;
            }
            else // 재배치 실패, originPos로 복귀
            {
                target.transform.position = originPos; // 기존 좌표로 복귀
                target.transform.parent = null; // 형성될 뻔 했던 부모자식 관계를 삭제

                // 오브젝트와 바닥 타일(tmp)의 하이라이팅 제거
                objectHighlightEraser(target);

                // 
                CallObjectController(false);
                target = null;

            }
            objectHighlightEraser(tmp);

            tmp = null;
            arrangeFlag = 99; // null
            isMove = false;
        }
    }

    public GameObject getCurFloor()
    {
        int curFloorIdx = GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx;
        GameObject curFloor = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[curFloorIdx];
        return curFloor;
    }

    public void chairCheck(GameObject obj, GameObject tile)
    {
        // 놓으려는 타일의 좌표
        int x = tile.GetComponent<ArrangeablePosState>().x;
        int y = tile.GetComponent<ArrangeablePosState>().y;

        // 그 위치 주변에 책상이 있는지 확인
        //int curFloorIdx = GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx;
        //FieldManager curField = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[curFloorIdx].GetComponent<FieldManager>();
        FieldManager curField = getCurFloor().GetComponent<FieldManager>();

        // 좌하단에 책상
        if (curField.field[x + 1, y].GetComponent<ArrangeablePosState>().getState() == 6)
        {
            // 그 책상을 바라봄
            obj.GetComponent<ChairSurporter>().setTable(curField.field[x + 1, y].GetComponent<ArrangeablePosState>().putHere);

            obj.transform.localScale = new Vector3(-1, 1, 1);
        }
        // 우하단에 책상
        else if (curField.field[x, y + 1].GetComponent<ArrangeablePosState>().getState() == 6)
        {
            // 그 책상을 바라봄
            obj.GetComponent<ChairSurporter>().setTable(curField.field[x, y + 1].GetComponent<ArrangeablePosState>().putHere);

            obj.transform.localScale = new Vector3(1, 1, 1);
        }
        // 책상이 없으면
        else
        {
            obj.GetComponent<ChairSurporter>().setTable(null);
        }

        // 해당 좌표를 sitable(pathfind의 목적지) 로 설정
        tile.GetComponent<ArrangeablePosState>().setState(4);

        // 해당 층에 의자 개수 1 추가
        curField.chairCount++;
    }

    public void tableCheck(GameObject obj, GameObject tile)
    {
        FieldManager curField = getCurFloor().GetComponent<FieldManager>();

        int x = tile.GetComponent<ArrangeablePosState>().x;
        int y = tile.GetComponent<ArrangeablePosState>().y;

        curField.field[x, y].GetComponent<ArrangeablePosState>().setState(6);

        // 기존 주변 의자들에게서 자신(테이블)의 정보를 삭제
        if (tmpChairLeft != null)
        {
            tmpChairLeft.GetComponent<ChairSurporter>().setTable(null);
            tmpChairLeft = null;
        }
        if (tmpChairRight != null)
        {
            tmpChairRight.GetComponent<ChairSurporter>().setTable(null);
            tmpChairRight = null;
        }

        // 주변 의자들에게 자신(테이블)의 정보를 전달
        // 좌상단이 의자라면
        if (y > 0 && curField.field[x, y - 1].GetComponent<ArrangeablePosState>().putHere != null && curField.field[x, y - 1].GetComponent<ArrangeablePosState>().putHere.CompareTag("Chair"))
        {// 내 테이블 정보 전달
            Debug.Log("책상배치, 좌상단에 의자");
            curField.field[x, y - 1].GetComponent<ArrangeablePosState>().putHere.GetComponent<ChairSurporter>().setTable(obj);
            curField.field[x, y - 1].GetComponent<ArrangeablePosState>().putHere.transform.localScale = new Vector3(1, 1, 1);
        }
        // 우상단이 의자라면
        if (x > 0 && curField.field[x - 1, y].GetComponent<ArrangeablePosState>().putHere != null && curField.field[x - 1, y].GetComponent<ArrangeablePosState>().putHere.CompareTag("Chair"))
        {// 내 테이블 정보 전달
            Debug.Log("책상배치, 우상단에 의자");
            curField.field[x - 1, y].GetComponent<ArrangeablePosState>().putHere.GetComponent<ChairSurporter>().setTable(obj);
            curField.field[x - 1, y].GetComponent<ArrangeablePosState>().putHere.transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
