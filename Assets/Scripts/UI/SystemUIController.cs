using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
System 탭의 기능들을 포함하는 스크립트 (설정 , 게임 종료 등 )

@author 진민준

@date 2019-10-15

@version 1.0.0

@details
최초 작성일: 191015

Recently modified list
- new script
- 게임 종료 구현

*/

public class SystemUIController : MonoBehaviour
{
    
    // 게임 종료 버튼 클릭시 호출
    public void GameExit()
    {
        // SAVE할 파일들이 있다면 저장한다

        // 종료한다.
        Application.Quit();
    }
}
