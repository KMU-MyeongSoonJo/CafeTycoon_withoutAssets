using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**

@brief
Interactive한 ui들을 컨트롤하는 스크립트 \n
default로 화면 하단에 위치한 버튼을이 이에 해당된다. \n
- 층 이동, 메뉴 버튼 등이 포함된다.
.    
@author 진민준

@date 2019-11-11

@version 1.3.2

@details
최초 작성일: 190915

Recently modified list
- 생성할 오브젝트 prefab 변수목록을 MenuUIController.cs로 이동

*/

public class InteractiveUIController: MonoBehaviour
{
    const int CAMERA_ZOOM_OUT_SIZE = 8;
    const int CAMERA_ZOOM_IN_SIZE = 6;
    const float CAMERA_ZOOM_OUT_XPOS = 4.0f;
    const float CAMERA_ZOOM_IN_XPOS = 0.0f;

    DataStorage dataStorage; // 버튼을 할당하기 위한 아이템 정보가 담겨있는 클래스
    Canvas menuCanvas; // menuCanvas (child)
    Canvas systemCanvas; // systemCanvas
    public bool systemTapIsOn; // 시스템 탭이 on/off 인지 확인하기위한 변수

    GameObject OutviewBase; // 비활성화된 System UI는 이곳에 위치
    
    FieldConstructor fieldConstructor;

    Camera mainCamera;

    private void Awake()
    {
        systemTapIsOn = false;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        fieldConstructor = GameObject.Find("GameManager").GetComponent<FieldConstructor>();
        menuCanvas = GameObject.Find("MenuCanvas").GetComponent<Canvas>();
        systemCanvas = GameObject.Find("SystemCanvas").GetComponent<Canvas>();

        OutviewBase = GameObject.Find("OutviewBase");
        dataStorage = GameObject.Find("GameManager").GetComponent<DataStorage>();
    }

    private void Start()
    {
        menuCanvas.gameObject.SetActive(false); // 메뉴 비활성화
        SystemBtnClicked(true);// 시스템 탭 비활성화
        
    }

    // 버튼 셋터
    void setButtons()
    {
        // 각 테마 순회
        for(int i = 0; i < dataStorage.itemManager.Length; i++)
        {
            // 각 테마별 아이템 목록 순회
            for(int j = 0; j < dataStorage.itemManager[i].Items.Length; j++)
            {
                // 버튼 할당

            }

        }
    }

    // MenuButton is clicked
    // Canvas change
    // Camera move
    public void MenuBtnClicked()
    {
        if(menuCanvas == null)
        {
            return;
        }

        SystemBtnClicked(true);

        if (menuCanvas.gameObject.activeSelf == false)
        {
            //StartCoroutine("menuCameraZoomOut");
            menuCanvas.gameObject.SetActive(true);
        }
        else
        {
            //StartCoroutine("menuCameraZoomIn");
            menuCanvas.gameObject.SetActive(false);
        }
    }

    // 닫기 위한 함수 호출이라면 true
    public void SystemBtnClicked(bool offFlag = false)
    {
        // 비활성화
        if (offFlag)
        {
            systemTapIsOn = false;
            systemCanvas.transform.position = OutviewBase.transform.position;
        }
        else
        {
            systemTapIsOn = true;
            if (systemCanvas.transform.position == transform.position)
                systemCanvas.transform.position = OutviewBase.transform.position;
            else
                systemCanvas.transform.position = transform.position;
        }
    }


    // Camera Move
    // ZoomOut  : 메뉴 진입                 // Size = 8, xPos = 4.0
    // ZoomIn   : 메뉴 탈출, 인 게임 복귀   // SIze = 6, xPos = 0.0
    IEnumerator menuCameraZoomOut()
    {
        float size = mainCamera.orthographicSize; float xPos = mainCamera.transform.position.x;

        while (mainCamera.transform.position.x < CAMERA_ZOOM_OUT_XPOS)
        {
            mainCamera.transform.Translate(new Vector3(CAMERA_ZOOM_OUT_XPOS * Time.deltaTime * 5, 0, 0));
            mainCamera.orthographicSize += (float)(CAMERA_ZOOM_OUT_SIZE - CAMERA_ZOOM_IN_SIZE) * Time.deltaTime * 5;
            yield return null;
        }
        // Menu 탭 화면에 표시
        menuCanvas.gameObject.SetActive(true);

        mainCamera.transform.position = new Vector3(CAMERA_ZOOM_OUT_XPOS, mainCamera.transform.position.y, mainCamera.transform.position.z);
        mainCamera.orthographicSize = CAMERA_ZOOM_OUT_SIZE;
    }

    IEnumerator menuCameraZoomIn()
    {
        float size = mainCamera.orthographicSize; float xPos = mainCamera.transform.position.x;

        // Menu 탭 화면에서 삭제
        menuCanvas.gameObject.SetActive(false);

        while (mainCamera.transform.position.x > CAMERA_ZOOM_IN_XPOS)
        {
            mainCamera.transform.Translate(new Vector3(- CAMERA_ZOOM_OUT_XPOS * Time.deltaTime * 5, 0, 0));
            mainCamera.orthographicSize -= (float)(CAMERA_ZOOM_OUT_SIZE - CAMERA_ZOOM_IN_SIZE) * Time.deltaTime * 5;
            yield return null;
        }
        
        mainCamera.transform.position = new Vector3(CAMERA_ZOOM_IN_XPOS, mainCamera.transform.position.y, mainCamera.transform.position.z);
        mainCamera.orthographicSize = CAMERA_ZOOM_IN_SIZE;
    }
}
