using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief 의자와 관계된 테이블이 있는지 확인하기위한 스크립트

@author 진민준

@date 2019-11-16

@version 1.0.0

@details
최초 작성일: 191116

Recently modified list
- 신규스크립트추가

*/

public class ChairSurporter : MonoBehaviour
{
    [SerializeField]
    GameObject myTable;

    
    public void setTable(GameObject obj)
    {
        myTable = obj;
    }

    public GameObject getTable()
    {
        return myTable;
    }
}


