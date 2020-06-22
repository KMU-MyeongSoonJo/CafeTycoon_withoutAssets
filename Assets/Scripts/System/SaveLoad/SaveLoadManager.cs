using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Data;
using System.Text;

/**

@brief
Save Load 를 구현하는 스크립트

@author 진민준

@date 2020-04-11

@version 1.1.1

@see
singleton class \n
저장 형식: SerialNumb(tile)/SerialNumb(obj)/SpinInfo  \n
ex) 0002/0001/2 : 0002 타일 설치, 0001 오브젝트 2회 회전

@details
최초 작성일: 2020-04-03

Recently modified list
- Load시 save 된 업그레이드 수준으로 자동 업그레이드하는 구문이 추가되었습니다.
- Load 직전에 Unity Editor의 모든 리소스 상태를 refresh하는 코드가 추가되었습니다.

*/

public class SaveLoadManager : MonoBehaviour
{
    /// singleton instance
    public static SaveLoadManager instance;

    public FieldConstructor FC;
    public DataStorage DS;
    public MenuUIController MUC;
    public FloorArranger FA;

    //public TextAsset floor; // 바닥 설치형 오브젝트 저장 정보
    TextAsset floor; // 바닥 설치형 오브젝트 저장 정보
    TextAsset wall; // 벽 설치형 오브젝트 저장 정보

    string[,] Sentence;
    int rowSize;

    private void Awake()
    {
        instance = this;

        //floor = Resources.Load<TextAsset>("SaveFiles/floor.csv");
        //wall= Resources.Load<TextAsset>("SaveFiles/wall.csv");
    }

    /// @brief 인테리어 정보 저장 함수
    /// @return 없음
    /// @param 없음
    public void save()
    {
        #region 1. 바닥 정보 저장
        using (var writer = new CsvFileWriter("Assets/Resources/SaveFiles/floor.csv"))
        //using (var writer = new CsvFileWriter(sourceDir))
        //var writer = new CsvFileWriter(sourceDir);
        {
            // columns.Add("str"); // 수차례
            // writer.WriteRow(columns);
            // columns.Clear();
            // 이상 반복으로 행 채워넣기
            List<string> columns = new List<string>();

            // 1층. 2~3층 추후 추가 예정
            FieldManager FM = FC.Floor[0].GetComponent<FieldManager>();

            // 각 타일을 순회하며 놓여진 오브젝트의 정보를 저장
            for (int i = 0; i < FM.field.GetLength(0); i++)
            {
                for (int j = 0; j < FM.field.GetLength(1); j++)
                {
                    ArrangeablePosState APS = FM.field[i, j].GetComponent<ArrangeablePosState>();
                    string str = null;

                    // 타일 정보 가져오기
                    str += APS.getTile() + "/";

                    // 오브젝트의 시리얼번호 가져오기
                    GameObject obj = FM.field[i, j].GetComponent<ArrangeablePosState>().putHere;
                    if (obj == null)
                    { // 오브젝트 없이 타일 정보만 저장
                    }
                    else
                    {
                        // Serial numb
                        string serial = obj.GetComponent<SerialNumbManager>().getSerialNumb();
                        str += serial + "/";

                        // Rotate state
                        string rotate = obj.GetComponent<FurnitureManager>().getRotateState().ToString();
                        str += rotate;
                    }
                    columns.Add(str);
                }
                writer.WriteRow(columns);
                columns.Clear();
            }
        }
        #endregion

        #region 2. 벽 정보 저장
        using (var writer = new CsvFileWriter("Assets/Resources/SaveFiles/wall.csv"))
        //writer = new CsvFileWriter("Assets/Resources/SaveFiles/wall.csv");
        {
            // columns.Add("str"); // 수차례
            // writer.WriteRow(columns);
            // columns.Clear();
            // 이상 반복으로 행 채워넣기
            List<string> columns = new List<string>();

            // 1층. 2~3층 추후 추가 예정
            FieldManager FM = FC.Floor[0].GetComponent<FieldManager>();


            #region leftWall save process
            // 각 타일을 순회하며 놓여진 오브젝트의 정보를 저장
            for (int i = 0; i < FM.leftWall.GetLength(1); i++)
            {
                for (int j = 0; j < FM.leftWall.GetLength(0); j++)
                {
                    // active가 false 인 벽에 대해서는 저장하는 과정을 생략한다.
                    if (!FM.leftWall[j, i].activeSelf) continue;

                    ArrangeablePosState APS = FM.leftWall[j, i].GetComponent<ArrangeablePosState>();
                    string str = null;

                    // 타일 정보 가져오기
                    str += APS.getTile() + "/";

                    // 오브젝트의 시리얼번호 가져오기
                    GameObject obj = FM.leftWall[j, i].GetComponent<ArrangeablePosState>().putHere;
                    if (obj == null)
                    { // 오브젝트 없이 타일 정보만 저장
                    }
                    else
                    {
                        // Serial numb
                        string serial = obj.GetComponent<SerialNumbManager>().getSerialNumb();
                        str += serial;
                    }
                    columns.Add(str);
                    Debug.Log("wall add");
                }
                writer.WriteRow(columns);
                columns.Clear();
            }
            #endregion
            
            #region rightWall save process
            // 각 타일을 순회하며 놓여진 오브젝트의 정보를 저장
            for (int i = 0; i < FM.rightWall.GetLength(0); i++)
            {
                for (int j = 0; j < FM.rightWall.GetLength(1); j++)
                {
                    // active가 false 인 벽에 대해서는 저장하는 과정을 생략한다.
                    if (!FM.rightWall[i, j].activeSelf) continue;

                    ArrangeablePosState APS = FM.rightWall[i, j].GetComponent<ArrangeablePosState>();
                    string str = null;

                    // 타일 정보 가져오기
                    str += APS.getTile() + "/";

                    // 오브젝트의 시리얼번호 가져오기
                    GameObject obj = FM.rightWall[i, j].GetComponent<ArrangeablePosState>().putHere;
                    if (obj == null)
                    { // 오브젝트 없이 타일 정보만 저장
                    }
                    else
                    {
                        // Serial numb
                        string serial = obj.GetComponent<SerialNumbManager>().getSerialNumb();
                        str += serial;
                    }
                    columns.Add(str);
                }
                writer.WriteRow(columns);
                columns.Clear();
            }
            #endregion

            // right wall에 대해서 좌우반전 시행
            // target.GetComponent<Transform>().localScale = new Vector3(-1, 1, 1);

        }

        #endregion

        Debug.Log("save done");
    }

    /// @brief 인테리어 정보 불러오기 함수
    /// @return 없음
    /// @param 없음
    public void load()
    {
        AssetDatabase.Refresh();

        //var reader = new CsvFileCommon("Assets/Resources/SaveFiles/floor.csv");
        floor = Resources.Load<TextAsset>("SaveFiles/floor");
        wall = Resources.Load<TextAsset>("SaveFiles/wall");

        string SerialNumb, RotateState;

        #region wall load
        string currentText = wall.text.Substring(0, wall.text.Length - 1); // 파일 끝의 개행문자 삭제
        string[] line = currentText.Split('\n'); // 2열: leftWall, 2열: rightWall, 총 4열
        rowSize = line.Length;

        // 1층. 추후 2~3층 추가 예정
        FieldManager FM = FC.Floor[0].GetComponent<FieldManager>();

        // save파일과 현재 필드 사이의 업그레이드 간극 맞추기
        string[] tmp = line[0].Split(',');
        // tmp.Length = 8 / 12 / 16
        int savedUpgrade;
            if (tmp.Length == 8) savedUpgrade = 1;
            else if (tmp.Length == 12) savedUpgrade = 2;
            else savedUpgrade = 3;

        for(int i = FM.getFieldUpgrade(); i < savedUpgrade; i++)
        {
            // ??? 왜이렇게했지
            FM.fieldUpgrade(FieldConstructor.MAX_UPGRADE);
        }

        #region leftWall load
        // 각 바닥 순회
        for (int i = 0; i < 2; i++)
        {
            string[] row = line[i].Split(',');

            Debug.Log("row.Length >> " + row.Length);


            for (int j = 0; j < row.Length; j++)
            {
                // 해당 칸 비우기
                GameObject destroied = FM.leftWall[j, i].GetComponent<ArrangeablePosState>().putHere;
                FA.ObjectControl_DELETE_REMOTE(destroied);

                // SerialNumb, RotateState 값 추출
                string[] item; // [0]: SN(tile), [1]: SN(obj), [2]: rotateState
                item = row[j].Split('/');

                // 타일 load
                int res;
                if (int.TryParse(item[0], out res))
                {
                    // 정보 추출
                    SerialNumb = string.Format("{0:D4}", res);

                    // 타일 배치
                    dispose(SerialNumb, FM.leftWall[j, i]);
                }

                // 오브젝트 load
                if (int.TryParse(item[1], out res))
                {
                    // 정보 추출
                    SerialNumb = string.Format("{0:D4}", res);
                    RotateState = "0";

                    // 좌표 설정
                    Vector3 tmpPos = FM.leftWall[j, i].GetComponent<ArrangeablePosState>().transform.position;
                    Vector3 initPos = Vector3.zero;
                    initPos = new Vector3(tmpPos.x, tmpPos.y, FM.leftWall[j, i].GetComponent<ArrangeablePosState>().getRowPos());

                    // 오브젝트 생성 및 배치
                    dispose(SerialNumb, RotateState, initPos, FM.leftWall[j, i]);
                }
                else // 해당 칸에 아무 오브젝트도 배치되어있지 않은 경우
                { }
            }
        }
        #endregion

        #region rightWall load
        // 각 바닥 순회
        for (int i = 2; i < 4; i++)
        {
            string[] row = line[i].Split(',');
            for (int j = 0; j < row.Length; j++)
            {
                // 해당 칸 비우기
                GameObject destroied = FM.rightWall[i - 2, j].GetComponent<ArrangeablePosState>().putHere;
                FA.ObjectControl_DELETE_REMOTE(destroied);

                // SerialNumb, RotateState 값 추출
                string[] item; // [0]: SN(tile), [1]: SN(obj), [2]: rotateState
                item = row[j].Split('/');

                // 타일 load
                int res;
                if (int.TryParse(item[0], out res))
                {
                    // 정보 추출
                    SerialNumb = string.Format("{0:D4}", res);

                    // 타일 배치
                    dispose(SerialNumb, FM.rightWall[i - 2, j]);
                }

                // 오브젝트 load
                if (int.TryParse(item[1], out res))
                {
                    // 정보 추출
                    SerialNumb = string.Format("{0:D4}", res);
                    RotateState = "0";

                    // 좌표 설정
                    Vector3 tmpPos = FM.rightWall[i - 2, j].GetComponent<ArrangeablePosState>().transform.position;
                    Vector3 initPos = Vector3.zero;
                    initPos = new Vector3(tmpPos.x, tmpPos.y, FM.rightWall[i - 2, j].GetComponent<ArrangeablePosState>().getRowPos());

                    // 오브젝트 생성 및 배치
                    dispose(SerialNumb, RotateState, initPos, FM.rightWall[i - 2, j], true);
                }
                else // 해당 칸에 아무 오브젝트도 배치되어있지 않은 경우
                { }
            }
        }
        #endregion

        #endregion

        #region floor load
         currentText = floor.text.Substring(0, floor.text.Length - 1); // 파일 끝의 개행문자 삭제
        line = currentText.Split('\n');
        rowSize = line.Length; // 행렬 사이즈 추출

        // 1층. 추후 2~3층 추가 예정
         FM = FC.Floor[0].GetComponent<FieldManager>();
        
        // 업그레이드 수준 비교


        // 각 바닥 순회
        for (int i = 0; i < rowSize; i++)
        {
            string[] row = line[i].Split(',');


            for (int j = 0; j < row.Length; j++)
            {
                // 해당 바닥 비우기
                GameObject destroied = FM.field[i, j].GetComponent<ArrangeablePosState>().putHere;
                FA.ObjectControl_DELETE_REMOTE(destroied);

                // SerialNumb, RotateState 값 추출
                string[] item; // [0]: SN(tile), [1]: SN(obj), [2]: rotateState
                item = row[j].Split('/');

                // 타일 load
                int res;
                if (int.TryParse(item[0], out res))
                {
                    // 정보 추출
                    SerialNumb = string.Format("{0:D4}", res);
                    
                    // 타일 배치
                    dispose(SerialNumb, FM.field[i, j]);
                }

                // 오브젝트 load
                if (int.TryParse(item[1], out res))
                {
                    // 정보 추출
                    SerialNumb = string.Format("{0:D4}", res);
                    RotateState = item[2];

                    // 좌표 설정
                    Vector3 tmpPos = FM.field[i, j].GetComponent<ArrangeablePosState>().transform.position;
                    Vector3 initPos = Vector3.zero;
                    initPos = new Vector3(tmpPos.x, tmpPos.y, FM.field[i, j].GetComponent<ArrangeablePosState>().getRowPos());

                    // 오브젝트 생성 및 배치
                    dispose(SerialNumb, RotateState, initPos, FM.field[i, j]);
                }
                else // 해당 칸에 아무 오브젝트도 배치되어있지 않은 경우
                { }
            }
        }
        #endregion

 

        Debug.Log("load done");
    }

    /// @brief
    ///     load()에서 호출되는 함수  \n
    ///     각 칸에 오브젝트를 배치하는 과정을 총괄한다.
    /// @return 없음
    /// @param serialNumb 배치할 오브젝트의 시리얼 번호
    /// @param rotateState 배치할 오브젝트의 회전 정보
    /// @param pos 오브젝트가 배치될 위치
    /// @param floor ArrangeablePosState를 지니고 있는 바닥 오브젝트
    /// @param isXFlip 배치 오브젝트의 좌우 반전이 필요한 경우(벽에 설치하는 등) true를 전달
    void dispose(string serialNumb, string rotateState, Vector3 pos, GameObject floor, bool isXFlip = false)
    {
        GameObject obj;

        // param의 serialNumb에서 특정 테마 추출
        ItemManager theme = DS.itemManager[int.Parse(serialNumb.Substring(0, 2))];

        // 해당 테마의 모든 오브젝트를 순회하며 오브젝트 추출
        for(int i = 0; i < theme.Items.Length; i++)
        {
            if(theme.Items[i].serialNumb.ToString() == serialNumb)
            {
                // SerialNumb 대조 후 해당 오브젝트 생성
                obj = MUC.ObjectInstantiater((int)theme.Items[i].itemType,
                    theme.Items[i].itemImage[0],
                    serialNumb, true);

                // 좌표 설정
                obj.transform.position = pos;

                // 회전 설정
                // 자동 회전 설정되는 경우는 추후 재설정(의자, 벽 등)
                for (int count = 0; count < int.Parse(rotateState); count++)
                { FA.ObjectControl_ROTATE_REMOTE(obj); }

                // 해당 층의 자식으로 설정
                obj.transform.parent = FA.getCurFloor().transform;

                // 바닥의 state를 set
                // obj의 putWhere를 set
                obj.GetComponent<FurnitureManager>().moveEnd(floor);

                // 바닥의 putHere를 set
                floor.GetComponent<ArrangeablePosState>().putHere = obj;

                // 책상, 의자 등 특수한 배치 실행
                switch (theme.Items[i].itemType)
                {
                    case ItemController._itemType.Chair:
                        FA.chairCheck(obj, floor);
                        break;
                    case ItemController._itemType.Table:
                        FA.tableCheck(obj, floor);
                        break;
                    default:
                        break;
                }

                // 좌우 반전이 필요한 경우(벽걸이형 오브젝트 등)
                if (isXFlip)
                    obj.GetComponent<Transform>().localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    /// @brief
    ///     override    \n
    ///     load()에서 호출되는 함수  \n
    ///     각 칸에 타일을 배치하는 과정을 총괄한다.
    /// @return 없음
    /// @param tileSerialNumb 배치할 타일의 시리얼 번호
    /// @param floor ArrangeablePosState를 지니고 있는 배치될 바닥 오브젝트
    void dispose(string tileSerialNumb, GameObject floor)
    {
        Sprite tile;

        // param의 serialNumb에서 특정 테마 추출
        ItemManager theme = DS.itemManager[int.Parse(tileSerialNumb.Substring(0, 2))];

        // 해당 테마의 모든 오브젝트를 순회하며 오브젝트 추출
        for (int i = 0; i < theme.Items.Length; i++)
        {
            if (theme.Items[i].serialNumb.ToString() == tileSerialNumb)
            {
                // SerialNumb 대조 후 해당 타일 이미지 불러오기
                tile = theme.Items[i].itemImage[0];

                // 타일 교체 및 시리얼 번호 할당
                floor.GetComponent<SpriteRenderer>().sprite = tile;
                floor.GetComponent<ArrangeablePosState>().setTile(tileSerialNumb);
            }
        }
    }
}
