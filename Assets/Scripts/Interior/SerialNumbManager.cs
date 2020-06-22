using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
시리얼 번호를 저장하는 스크립트

@author 진민준

@date 2020-04-03

@version 1.0.0

@details
최초 작성일: 2020-04-03

Recently modified list
- new script

*/

public class SerialNumbManager : MonoBehaviour
{
    [SerializeField]
    private string serialNumb;
    
    public void setSerialNumb(string numb)
    {
        serialNumb = numb;
    }

    public string getSerialNumb()
    {
        return serialNumb;
    }
}
