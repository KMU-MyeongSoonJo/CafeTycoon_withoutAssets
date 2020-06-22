using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
아이템의 정보를 저장하는 스크립트

@author 진민준

@date 2019-11-20

@version 1.0.0

@details
최초 작성일: 191120

Recently modified list
- new script

*/

[System.Serializable]
public class ItemController
{
    public enum _itemType
    {
        Table, // 테이블
        Chair, // 의자
        WallAcc, // 벽장식
        FloorAcc, // 바닥장식
        WallTile, // 벽타일
        FloorTile, // 바닥타일
        Partition, // 파티션
        Counter, // 카운터 - 게임 내 하나만 존재.
                 // 계산블럭 외의 공간은 FloorAcc로 분류
    }

    public string serialNumb; // 4자리의 일련번호 ex) 0212 : 02번 테마의 12번 아이템
    public string itemName; // 이름. 내가 구분하기 위함
    public int itemPrice; // 가격
    public _itemType itemType; // 종류(enum)
    public Sprite[] itemImage; // 이미지. 경우에 따라 2개 이상의 이미지가 필요(파티션 등)

    public ItemController(string _serialNumb, int _itemPrice, _itemType itemType, Sprite[] _itemImage)
    {
        serialNumb = _serialNumb;
        itemPrice = _itemPrice;
        this.itemType = itemType;
        itemImage = _itemImage;
    }
    public ItemController() { }
}
