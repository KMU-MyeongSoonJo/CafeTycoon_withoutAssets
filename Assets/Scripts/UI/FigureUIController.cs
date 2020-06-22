using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/**

@brief
화면 최상단에 출력될 Figure(수치) 값들이 저장되는 스크립트 \n
ex) 현재 층, 직원 수, 손님 수, 현재 날짜(시각), 운영 자금 등 \n

@author 진민준

@date 2019-11-24

@version 1.1.0

@details
최초 작성일: 191004

Recently modified list
- 낮/밤 상태, 손님 수 출력

*/

public class FigureUIController : MonoBehaviour
{
    //낮밤 이미지 및 아이콘
    [SerializeField]
    Sprite daytimeSprite;
    [SerializeField]
    Sprite nightSprite;
    [SerializeField]
    Image DaytimeIcon;

    // 현재 손님 표시 아이콘 및 해당 정보가 담긴 스크립트
    [SerializeField]
    Text curGuestCount;    
    FieldManager fieldManager;

    private void Start()
    {
        GameObject curFloor = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx];
        fieldManager = curFloor.GetComponent<FieldManager>();
    }

    private void Update()
    {
        curGuestCount.text = fieldManager.guestCount.ToString();
    }

    public void setDayNight(bool isDaytime)
    {
        if (isDaytime)
            DaytimeIcon.sprite = daytimeSprite;
        else
            DaytimeIcon.sprite = nightSprite;
    }
}
