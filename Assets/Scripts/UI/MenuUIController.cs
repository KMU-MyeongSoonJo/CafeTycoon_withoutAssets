using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


/**

@brief
Menu UI의 동작을 컨트롤하는 스크립트 \n
Menu UI의 세부 탭들을 컨트롤하는 스크립트 \n

@author 진민준

@date 2020-04-11

@version 1.3.11

@details
최초 작성일: 191007

Recently modified list
- Load시 Inventory 탭으로 자동으로 넘어가던 버그가 수정되었습니다.

*/

public class MenuUIController : MonoBehaviour
{

    public GameObject[] MenuTaps;
    

    // MenuTap들은 OutviewBase <-> MenuTapsBase 를 오가며 플레이어와 상호작용한다
    // 비활성화된 Canvas가 위치할 화면 밖의 공간
    GameObject MenuTapsBase;

    // 메뉴 세부 탭 너머로 필드를 터치할 수 없게 한다.
    // 세부 탭 활성화될 때 같이 활성화
    GameObject MenuBackground;
    
    // Instantiate 할 때 쓰일 target
    GameObject target;

    // 생성할 오브젝트의 베이스 프리펩
    // Menu UI의 상점과 인벤토리가 FInd해서 사용
    // 테마에 따라 외관을 바꿔가며 사용
    [SerializeField]
    public GameObject[] ObjectsPrefab;

    [SerializeField]
    GameObject[] CharactersPrefab;

    private void Awake()
    {
        //OutviewBase = GameObject.Find("OutviewBase");
        MenuTapsBase = GameObject.Find("MenuTapsBase");


        MenuBackground = GameObject.Find("MenuBackground");
        MenuBackground.SetActive(false); // 메뉴 백그라운드 비활성화

        //메뉴 탭 씬들 화면 중앙으로 가져오기
        for (int i=0;i<MenuTaps.Length; i++)
       {
            MenuTaps[i].transform.position = MenuTapsBase.transform.position;
       }
    }


    private void Update()
    {
        // 탭 외부(background)를 클릭하면 탭을 꺼지게 하기 위함
        if (Input.GetMouseButtonDown(0))
        {
            // 게임 오브젝트 위라면 false ?
            // 아니라면 true ?
            // 탭 닫기
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                // 메뉴 닫기
                for (int i = 0; i < MenuTaps.Length; i++)
                {
                    // 활성화 되어있다면 -> 비활성화
                    if (MenuTaps[i] == true)
                    {
                        MenuTaps[i].SetActive(false);
                    }
                }
                MenuBackground.SetActive(false);

            }
        }
    }

    // 스크립트가 활성화될 때 호출되는 함수
    private void OnEnable()
    {
        
    }

    // 스크립트가 비활성화될 때 호출되는 함수
    // 모든 탭 캔버스를 비활성화
    private void OnDisable()
    {
        for (int i = 0; i < MenuTaps.Length; i++)
        {
            // 활성화 되어있다면 -> 비활성화
            //if (MenuTaps[i] == true)
            if (MenuTaps[i].activeSelf == true)
            {
                MenuTaps[i].SetActive(false);
            }
        }
        MenuBackground.SetActive(false);
    }

    // esc를 누르면 창을 닫는다. 메뉴 탭은 여전히 활성화.
    // 메뉴 하위 탭을 다시 눌러도 창을 닫는다 ?

    // 각 탭 클릭
    // idx 탭 제외 모든 탭 비활성화
    public void TapClicked(int idx)
    {
        // 탭 클릭시, 배치 취소 -버그 방지
        GameObject.Find("GameManager").GetComponent<FloorArranger>().cancelReplace();


        for (int i = 0; i < MenuTaps.Length; i++)
        {
            // 다른 탭이 활성화되어있다면 비활성화하고 새 탭을 연다
            if (i != idx && MenuTaps[i].activeSelf == true)
            {
                MenuTaps[i].SetActive(false);
                MenuBackground.SetActive(true);
            }
            else if (i == idx)
            {
                //활성화하려는 탭이 활성화 상태였다면 비활성화한다.
                if (MenuTaps[i].activeSelf == true)
                {
                    MenuTaps[i].SetActive(false);
                    MenuBackground.SetActive(false);
                }
                // 활성화하려는 탭이 비활성화 상태였다면 활성화한다.
                else if (MenuTaps[i].activeSelf == false)
                {
                    MenuTaps[i].SetActive(true);
                    MenuBackground.SetActive(true);
                }
            }
        }
    }
    
    // 오브젝트 배치
    // 클릭시 해당 버튼에 대해 프리팹 셋팅 된 오브젝트를 생성
    // 클릭시 인벤토리 탭, ray 차단 Background SetActive(false)
    // isLoad: Save/Load 시 오브젝트 생성에 사용하기위한 플래그
    public GameObject ObjectInstantiater(int itemType, Sprite sprite, string serial, bool isLoad = false)
    {
        // idx
        // : ItemController._itemType.[enum]
        // 낮이라면 오브젝트 배치 불가
        if (GameObject.Find("GameManager").GetComponent<DayController>().isDaytime) return null;
        
        // 인벤토리 탭 닫기
        if(!isLoad) TapClicked(2);
        
        // 베이스 오브젝트(의자, 테이블 등) 동적 생성
        target = Instantiate(ObjectsPrefab[itemType], new Vector3(-600f, 0, 0), transform.rotation);

        // 그 오브젝트에 정보 덮어씌우기(이미지, 시리얼 번호 등)
        // 이미지
        target.GetComponent<SpriteRenderer>().sprite = sprite;

        TileManager tmpTileManager = target.GetComponent<TileManager>();

        // 타일이라면 타일 이미지 설정
        if (tmpTileManager != null)
            tmpTileManager.setTile(sprite);

        // 시리얼 번호 할당
        target.GetComponent<SerialNumbManager>().setSerialNumb(serial);
            

        // 활성화 Floor의 하위에 오브젝트 생성 및 전달
        // renderer.enabled()를 제어하기 위함
        if (!isLoad)
        {
            GameObject.Find("GameManager").GetComponent<FloorArranger>().ButtonClicker(target); return null;
        }
        else // Load를 위한 오브젝트 생성이라면
        {
            return target;
        }
    }

    // 버튼, 캐릭터 씬? 방으로 넘어가는 버튼
    public void EnterRoom()
    {
        SceneManager.LoadScene(2);
    }
}
