using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief 배치가 가능한 각 타일들의 속성 및 상태(State)를 저장하는 스크립트

@author 진민준

@date 2020-04-05

@version 1.0.3

@details
최초 작성일: 190915

Recently modified list
- 이제 바닥이 설치된 타일의 정보(시리얼 번호)를 저장합니다.

*/

public class ArrangeablePosState : MonoBehaviour
{
    [SerializeField]
    /**
    state of ArrangeablePos
    - -1: lock
    - 0: passable (usable)
    - 1: unpassable (unusable)
    - 2: unpassable (passfinding에 사용한 후 0으로 되돌린다)
    - 3: enterance (in)
    - 4: sitable (nobody sit on this chair)
    - 5: unsitable (somebody sit on this chair)
    - 6: table */
    int state; 
   

    /// 배치되어있는 오브젝트
    public GameObject putHere;

    public string __DEFAULT_TILE__;

    /// 배치되어있는 타일의 시리얼 번호
    [SerializeField]
    string putHereTile;

    /** ArrangeablePos가 '벽'일 경우 배치되는 오브젝트의 좌/우를 구분하기 위한 플래그 */
    bool ifWall; // true: left  ,  false: right

    /** field[,] 상의 좌표 */
    public int x, y;

    /**
    - 배치될 오브젝트의 z pos값
    - sortingLayout의 역할을 대신한다 */
     public float rowPos;

    private void Start()
    {
        if (state != -1)
            putHereTile = __DEFAULT_TILE__;
    }

    public void setPos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void setState(float rowPos, int state, bool ifWall = false)
    {
        this.rowPos = rowPos;
        this.state = state;
        this.ifWall = ifWall;
    }
    public void setState(int state)
    {
        this.state = state;
    }
    public float getRowPos()
    {
        return rowPos;
    }
    
    public int getState()
    {
        return state;
    }

    public void setTile(string serial)
    {
        putHereTile = serial;
    }

    public string getTile()
    {
        return putHereTile;
    }

    public bool getIfWall()
    {
        return ifWall;
    }


}
