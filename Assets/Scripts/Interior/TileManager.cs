using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
타일, 벽지 교체를 관리하는 스크립트

@author 진민준

@date 2019-11-02

@version 1.0.0

@details
최초 작성일: 191102

Recently modified list
- new script

*/

public class TileManager : MonoBehaviour
{
    
    Sprite tile;

    private void Awake()
    {
        // 자신 스프라이트
        tile = gameObject.GetComponent<SpriteRenderer>().sprite;
    }

    public Sprite getTile()
    {
        return tile;
    }
    public void setTile(Sprite _tile)
    {
        tile = _tile;
    }
}
