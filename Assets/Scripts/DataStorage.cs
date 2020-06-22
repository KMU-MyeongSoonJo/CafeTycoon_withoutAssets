using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
각 테마별 오브젝트들의 정보들이 저장되는 스크립트

@author 진민준

@date 2019-11-20

@version 1.0.1

@details
최초 작성일: 190927

@see
Theme               Each Objects        \n
ItemManager[] ->    ItemController[]    \n

Recently modified list
- 테마별 오브젝트들의 정보를 저장합니다.
.
*/

[System.Serializable]
public class DataStorage : MonoBehaviour
{
    //  Theme               Each Objects
    //  ItemManager[] ->    ItemController[]
    public ItemManager[] itemManager;
}
