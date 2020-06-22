using System.Collections;   
using System.Collections.Generic;
using UnityEngine;

/**

@brief
오브젝트 정보를 저장하는 스크립트

@author 진민준

@date 2019-11-23

@version 1.3.5

@details
최초 작성일: 190921

Recently modified list
- 구조 개선, 중복 기능 변수 삭제

*/

public class FurnitureManager : MonoBehaviour
{
    /**
     * 0: 3->0 , sprite change \n
     * 1: 0->1 , flipY \n
     * 2: 1->2 , sprite change \n
     * 3: 2->3 , flipY \n
     */
    int rotateState;

    // 파티션과 같이 모양이 하나가 아닌 경우
    // - < > | X 모양 등
    // 회전시킴에 따라 여러 모양을 제공
    [SerializeField]
    bool isNeedPrefabChange = false;
    [SerializeField]
    Sprite[] shapes;

    // 놓여져 있는 타일 정보
    public GameObject putWhere;

    /***
     * 
     * 파티션의 경우
     *  일자형 , 코너(상, 하, 좌우) , 십자형 총 5개 필요
     * 
     * **/

    SpriteRenderer spriteRenderer;
    Transform transform;

    //기존 배치되어있던 Pos의 정보
    [SerializeField]
    //ArrangeablePosState AP_1;
    //새로 배치된 Pos의 정보
    public ArrangeablePosState newPos;

    private void Awake()
    {
        rotateState = 0;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        transform = gameObject.GetComponent<Transform>();
        //spriteRenderer.sprite = sprites[rotateState];
    }
    
    public void rotate()
    {
        // 모양(shape)이 바뀌지 않는 오브젝트라면
        // 단순 회전 오브젝트라면
        if (!isNeedPrefabChange)
        {
            switch (rotateState)
            {
                case 0:
                    transform.localScale = new Vector3(-1, 1, 1);
                    //spriteRenderer.flipX = true;
                    rotateState = 1;
                    break;
                case 1:
                    transform.localScale = new Vector3(1, 1, 1);
                    //spriteRenderer.flipX = false;
                    rotateState = 0;
                    break;
            }
        }
        else
        {
            // 파티션의 경우
            // 일자(좌우), 코너(상하좌우), 십자 7개 필요

            switch (rotateState)
            {
                case 0:
                    // 일자(좌우반전)
                    spriteRenderer.flipX = true;
                    //spriteRenderer.flipX = true;
                    rotateState = 1;
                    break;

                // 코너 상하좌우
                case 1:
                    // 코너 상
                    spriteRenderer.flipX = false;
                    spriteRenderer.sprite = shapes[1];
                    rotateState = 2;
                    break;
                case 2:
                    // 코너 하
                    spriteRenderer.sprite = shapes[2];
                    rotateState = 3;
                    break;
                case 3:
                    // 코너 좌
                    spriteRenderer.sprite = shapes[3];
                    spriteRenderer.flipX = false;
                    rotateState = 4;
                    break;
                case 4:
                    // 코너 우(좌우반전)
                    spriteRenderer.flipX = true;
                    rotateState = 5;
                    break;
                case 5:
                    // 십자(X) 모양
                    spriteRenderer.flipX = false;
                    spriteRenderer.sprite = shapes[4];
                    rotateState = 6;
                    break;

                case 6:
                    // 일자
                    spriteRenderer.sprite = shapes[0];
                    spriteRenderer.flipX = false;
                    rotateState = 0;
                    break;
            }
        }
    }

    public int getRotateState()
    {
        return rotateState;
    }

    // stack 되는 경우는 조건
    public void moveEnd(GameObject _newPos)
    {
        

        if (putWhere!=null && putWhere.GetComponent<ArrangeablePosState>() != null)
            //기존 좌표에서 삭제
            putWhere.GetComponent<ArrangeablePosState>().setState(0);

        //스택 자리가 아닐 경우에 좌표 갱신
        //즉, 바닥에 놓여지는 경우에만 해당 좌표에 정보를 갱신
        if (_newPos != null && !_newPos.CompareTag("Arrangeable_Stack"))
        {
            //새 위치에 정보 갱신
            newPos = _newPos.GetComponent<ArrangeablePosState>();
            newPos.setState(1);


            //기존 위치 갱신
            putWhere = newPos.gameObject;

        }
    }

    //삭제될 때 호출
    //해당 필드에서 자신의 정보를 삭제
    public void destroied()
    {
        putWhere.GetComponent<ArrangeablePosState>().setState(0);
    }
}
