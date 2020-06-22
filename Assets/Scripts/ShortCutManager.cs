using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**

@brief
게임 내 단축키를 통합 관리하는 스크립트

@author 진민준

@date 2019-11-29

@version 1.1.0

@details
최초 작성일: 191111

Recently modified list
- 카메라 이동 범위가 변경되었습니다.
    - 이제 카메라가 게임 화면 밖으로 크게 벗어나지 못합니다.
    .
.

*/

public class ShortCutManager : MonoBehaviour
{
    const float MIN_CAMERA_SIZE = 1.5f; // 3 -> 4.5 -> 6
    const float BASIC_CAMERA_SIZE = 3.0f;
    const int MAX_CAMERA_SIZE = 1;

    // Up/Down의 경우 현재 층에 따라 좌표값 수정 필요   
    //const int MAX_CAMERA_UP_POS = 5;
    //const int MIN_CAMERA_DOWN_POS = -5;
    const float MAX_CAMERA_UP_POS = 3f;
    const float MIN_CAMERA_DOWN_POS = -1.5f; // -4f -> -6.5f -> -9f
    const float MIN_CAMERA_LEFT_POS = -2.8f; // -5f -> -7.5f -> 10f
    const float MAX_CAMERA_RIGHT_POS = 2.8f; // 5f -> 7.5f -> 10f

    // Main Camera
    Camera mainCamera;

    float scrollSpeed = 10f;
    float moveSpeed = 5f;

    // curActivateFloorIdx 를 받아오기 위함
    FieldConstructor fieldConstructor;

    // 키 입력시 배치를 취소하기 위함(cancelReplace())
    // CallObjectController(false);
    FloorArranger floorArranger;

    // System / Menu tap 의 enable 여부를 확인
    InteractiveUIController interActiveUIController;

    // 세부 Menu Taps의 enable 여부를 확인
    MenuUIController menuUIController;

    // 캐릭터 편집 탭의 호출 여부를 확인
    // CallCharacterController(bool);
    CharacterManager characterManager;

    private void Awake()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        floorArranger = GameObject.Find("GameManager").GetComponent<FloorArranger>();
        interActiveUIController = GameObject.Find("InteractiveCanvas").GetComponent<InteractiveUIController>();
        menuUIController = GameObject.Find("MenuCanvas").GetComponent<MenuUIController>();
        fieldConstructor = GameObject.Find("GameManager").GetComponent<FieldConstructor>();
        characterManager = GameObject.Find("GameManager").GetComponent<CharacterManager>();
        //Cursor.lockState = CursorLockMode.Confined; // 게임 창 밖으로 마우스가 안나감
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //마우스를 통한 화면 이동
        //Left
        if (Input.mousePosition.x < 50 || Input.GetKey(KeyCode.A))
        {
            // 해당 층의 upgrade 정보 반환
            int upgrade = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx].GetComponent<FieldManager>().getFieldUpgrade();

            if (mainCamera.ViewportToWorldPoint(new Vector3(0f, .5f)).x > MIN_CAMERA_LEFT_POS - 2.5f * upgrade)
            {
                mainCamera.transform.position += new Vector3(-1f, 0, 0) * Time.deltaTime * moveSpeed;
            }
        }
        //Right
        if(Input.mousePosition.x > Screen.width - 50 || Input.GetKey(KeyCode.D))
        {
            // 해당 층의 upgrade 정보 반환
            int upgrade = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx].GetComponent<FieldManager>().getFieldUpgrade();
            
            //Debug.Log(mainCamera.ViewportToWorldPoint(new Vector3(0f, .5f)).x);
            if (mainCamera.ViewportToWorldPoint(new Vector3(1f, .5f)).x < MAX_CAMERA_RIGHT_POS + 2.5f * upgrade)
            {
                mainCamera.transform.position += new Vector3(1f, 0, 0) * Time.deltaTime * moveSpeed;
            }
        }
        
        //Down
        if(Input.mousePosition.y < 50 || Input.GetKey(KeyCode.S))
        {
            // 해당 층의 upgrade 정보 반환
            int upgrade = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx].GetComponent<FieldManager>().getFieldUpgrade();

            if (mainCamera.ViewportToWorldPoint(new Vector3(.5f, 0f)).y > MIN_CAMERA_DOWN_POS - 2.5f * upgrade)
            {
                mainCamera.transform.position += new Vector3(0, -1f, 0) * Time.deltaTime * moveSpeed;
            }
        }
        //Up
        if(Input.mousePosition.y > Screen.height - 50 || Input.GetKey(KeyCode.W))
        {
            if (mainCamera.ViewportToWorldPoint(new Vector3(.5f, 1f)).y < MAX_CAMERA_UP_POS)
            {
                mainCamera.transform.position += new Vector3(0, 1f, 0) * Time.deltaTime * moveSpeed;
            }
        }


        // Tap move
        if (Input.GetKeyDown(KeyCode.Escape)) {
            floorArranger.cancelReplace();
            floorArranger.CallObjectController(false);
            characterManager.CallCharacterController(false);

            // Menu Taps를 순회하며 켜진 탭이 있는지 확인
            for (int i = 0; i < menuUIController.MenuTaps.Length; i++)
            {
                if (menuUIController.MenuTaps[i].activeSelf == true)
                {
                    menuUIController.MenuTaps[i].SetActive(false);
                    return;
                }
            }

            // Menu 혹은 System 탭을 모두 끔
            // 메뉴탭이 켜져있다면 카메라 무브 실행하며 메뉴 탭 종료
            // 시스템 탭 종료가 MenuBtnClicked() 내에서 함께 이루어짐
            if (menuUIController.gameObject.activeSelf == true) {
                interActiveUIController.MenuBtnClicked();
                return;
            }

            // 시스템 탭이 켜져있었다면
            else if (interActiveUIController.systemTapIsOn)
            {
                interActiveUIController.SystemBtnClicked(true);
                return;
            }

            // 아무 탭도 켜져있지 않았다면(홈화면이었다면)
            // 시스템 탭을 킴
            else
            {
                interActiveUIController.SystemBtnClicked();
            }

        }
        else if (Input.GetKeyDown(KeyCode.Tab)) {
            interActiveUIController.MenuBtnClicked();
        }
        else if (menuUIController.gameObject.activeSelf == true)
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                floorArranger.cancelReplace();
                floorArranger.CallObjectController(false);
                characterManager.CallCharacterController(false);

                menuUIController.TapClicked(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                floorArranger.cancelReplace();
                floorArranger.CallObjectController(false);
                characterManager.CallCharacterController(false);

                menuUIController.TapClicked(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                floorArranger.cancelReplace();
                floorArranger.CallObjectController(false);
                characterManager.CallCharacterController(false);

                menuUIController.TapClicked(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                floorArranger.cancelReplace();
                floorArranger.CallObjectController(false);
                characterManager.CallCharacterController(false);

                menuUIController.TapClicked(3);
            }
        }

        

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        // 카메라 축소
        // scroll : -
        // orthographicSize를 + 한다
        if (scroll < 0 || Input.GetKey(KeyCode.C))
        {
            // 해당 층의 upgrade 정보 반환
            int upgrade = GameObject.Find("GameManager").GetComponent<FieldConstructor>().Floor[GameObject.Find("GameManager").GetComponent<FieldConstructor>().curActivateFloorIdx].GetComponent<FieldManager>().getFieldUpgrade();
            scroll = -0.6f;
            if (mainCamera.orthographicSize < MIN_CAMERA_SIZE + 1.5f * upgrade)
            {
                mainCamera.orthographicSize += scroll * scrollSpeed * -1 * Time.deltaTime;


                // 좌 혹은 우측으로 카메라가 화면 밖으로 벗어난다면
                // 반대 방향으로 카메라를 이동시켜 뒷 배경이 가능한 보이지 않게
                // 좌측
                if (mainCamera.ViewportToWorldPoint(new Vector3(0f, .5f)).x < MIN_CAMERA_LEFT_POS - 2.5f * upgrade)
                {
                    mainCamera.transform.Translate(new Vector3(MIN_CAMERA_LEFT_POS - 2.5f * upgrade - mainCamera.ViewportToWorldPoint(new Vector3(0f, .5f)).x, 0));
                }
                // 우측
                if (mainCamera.ViewportToWorldPoint(new Vector3(1f, .5f)).x > MAX_CAMERA_RIGHT_POS + 2.5f * upgrade)
                {
                    mainCamera.transform.Translate(new Vector3(MAX_CAMERA_RIGHT_POS + 2.5f * upgrade - mainCamera.ViewportToWorldPoint(new Vector3(1f, .5f)).x, 0));
                }
                // 하단
                if (mainCamera.ViewportToWorldPoint(new Vector3(.5f, 0f)).y < MIN_CAMERA_DOWN_POS - 2.5f * upgrade)
                {
                    mainCamera.transform.Translate(new Vector3(0, MIN_CAMERA_DOWN_POS - 2.5f * upgrade - mainCamera.ViewportToWorldPoint(new Vector3(.5f, 0f)).y));
                }
                // 상단
                if (mainCamera.ViewportToWorldPoint(new Vector3(.5f, 1f)).y > MAX_CAMERA_UP_POS )
                {
                    mainCamera.transform.Translate(new Vector3(0, MAX_CAMERA_UP_POS - mainCamera.ViewportToWorldPoint(new Vector3(.5f, 1f)).y));
                }
            }
            else
            {
                mainCamera.orthographicSize = MIN_CAMERA_SIZE + 1.5f * upgrade;
            }
        }
        // 카메라 확대
        // scroll : +
        // orthographicSize를 - 한다
        else if (scroll > 0 || Input.GetKey(KeyCode.Z))
        {
            scroll = 0.6f;
            if (mainCamera.orthographicSize > MAX_CAMERA_SIZE)
                mainCamera.orthographicSize -= scroll * scrollSpeed * Time.deltaTime;
            else
            {
                mainCamera.orthographicSize = MAX_CAMERA_SIZE;
            }
        }
    }
}
