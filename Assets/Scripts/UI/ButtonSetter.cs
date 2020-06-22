using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**

@brief
인벤토리 창 내부 버튼들을 총괄하는 스크립트

@author 진민준

@date 2019-11-24

@version 1.0.2

@details
최초 작성일: 191120

Recently modified list
- 횡스크롤이 삭제되었습니다.
- 테마에 아이템이 많아 가로 버튼 칸을 넘어가는 경우 바로 아래 줄에 출력됩니다.

*/

public class ButtonSetter : MonoBehaviour
{
    DataStorage dataStorage;
    MenuUIController menuUIController;

    [SerializeField]
    public Button[] btns;
    // 한 행에 8칸씩
    // 넘어가면 다음 행으로 밀려남

    private void Awake()
    {
        dataStorage = GameObject.Find("GameManager").GetComponent<DataStorage>();
        menuUIController = GameObject.Find("MenuCanvas").GetComponent<MenuUIController>();

        btns = GetComponentsInChildren<Button>();

        foreach(var b in btns)
        {
            b.gameObject.SetActive(false);
        }

        // 테마 수
        //for(int i = 0; i < dataStorage.itemManager.Length; i++)
        for (int i = 0, themeIdx=0, j; themeIdx < dataStorage.itemManager.Length; i++, themeIdx++)
        {
            // ruins 테마의 아이템 순회
            for( j = 0 ; j < dataStorage.itemManager[themeIdx].Items.Length; j++)
            {
                //각 버튼 이미지 세팅
                //btns[i * 15 + j].GetComponent<Image>().sprite = dataStorage.itemManager[i].Themes[j].itemImage[0];
                btns[i * 8 + j].transform.Find("Image").GetComponent<Image>().sprite = dataStorage.itemManager[themeIdx].Items[j].itemImage[0];
                btns[i * 8 + j].gameObject.SetActive(true);

                // 각 버튼 Listener 세팅
                // base prefab instantiate
                // 해당 instantiate 된 object에 dataStorage의 정보 덮어씌우기
                // 화면 내로 오브젝트 이동
                int itemType = -1;
                switch (dataStorage.itemManager[themeIdx].Items[j].itemType)
                {
                    case ItemController._itemType.Table:
                        itemType = (int)ItemController._itemType.Table; // Table
                        break;
                    case ItemController._itemType.Chair:
                        itemType = (int)ItemController._itemType.Chair; 
                        break;
                    case ItemController._itemType.WallAcc:
                        itemType = (int)ItemController._itemType.WallAcc; 
                        break;
                    case ItemController._itemType.FloorAcc:
                        itemType = (int)ItemController._itemType.FloorAcc; 
                        break;
                    case ItemController._itemType.WallTile:
                        itemType = (int)ItemController._itemType.WallTile;
                        break;
                    case ItemController._itemType.FloorTile:
                        itemType = (int)ItemController._itemType.FloorTile; 
                        break;
                    case ItemController._itemType.Partition:
                        itemType = (int)ItemController._itemType.Partition;
                        break;
                    case ItemController._itemType.Counter:
                        itemType = (int)ItemController._itemType.Counter;
                        break;

                }

                //if (dataStorage.itemManager[i].Themes[j].itemType == ItemController._itemType.Table)
                //    idx = 0; // Table
                //else if (dataStorage.itemManager[i].Themes[j].itemType == ItemController._itemType.Chair)
                //    idx = 1; // Chair
                Sprite sprite = dataStorage.itemManager[themeIdx].Items[j].itemImage[0];
                string serial = dataStorage.itemManager[themeIdx].Items[j].serialNumb;

                btns[i * 8 + j].onClick.AddListener(delegate () { menuUIController.ObjectInstantiater(itemType, sprite, serial); });

            }
            // 한 테마의 아이템이 두 줄 이상 차지했다면
            // 그 다음 테마의 아이템은 새로운 줄에서부터 배치
            if (j > 8) i++;
        }
    }

}
