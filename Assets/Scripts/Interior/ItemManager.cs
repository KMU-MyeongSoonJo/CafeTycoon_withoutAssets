using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
    아이템들을 관리하는 스크립트 \n
    테마별로 각기 배열을 생성해 관리

@author 진민준

@date 2019-11-20

@version 1.0.0

@details
최초 작성일: 191120

Recently modified list
- new script

*/

[System.Serializable]
public class ItemManager
{
    public string themeName;
    public ItemController[] Items;
    
    
}
