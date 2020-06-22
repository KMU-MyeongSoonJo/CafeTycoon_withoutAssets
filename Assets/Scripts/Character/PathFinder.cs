using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**

@brief
A* 길찾기 구현 스크립트

@author 진민준

@date 2019-11-16

@version 1.1.3

@details
최초 작성일: 191001

Recently modified list
- 의자 위치 탐색시, 누가 사용중인 의자로 중복 접근 불가하게 설정
- 출구 위치 탐색시, 자신이 앉아있던 의자에 타인이 접근 가능하게 변경

*/

public class PathFinder : MonoBehaviour
{
    // PathFinder 스크립트는 Floor Prefab 내에 포함된다 
    FieldManager fieldManager; // 동 Floor 내의 FieldManager.cs
    WayPoint[,] wayPoint = new WayPoint[FieldManager.MAX_ROW, FieldManager.MAX_COL];
    List<WayPoint> openList; // 열린 블럭. null이 되면 길이 없음을 의미한다
    List<WayPoint> closeList; // 닫힌 블럭

    /**
     * @brief
     * A* 알고리즘을 적용하기위한 각 타일의 정보 및 속성
     */
    public class WayPoint
    {
        public int x { get; set; }// 본인 좌표 
        public int y { get; set; }
        //public bool isOpen { get; set; } // 열렸나(통과 가능) 닫혔나(통과 불가능)
        public int G { get; set; }// G = 시작점으로부터의 이동 비용
        public int H { get; set; }// H = 도착지점으로의 예상 이동 비용
        public int F{ get; set; } // F = G + H   
        //public bool isDest { get; set; }
        
        public WayPoint lastP; // 지나온 블럭

        // 생성자
        // 본인 좌표 설정
        public WayPoint(int x, int y)
        {
            this.x = x; this.y = y;
            G = 0;
            H = 0;
            F = 0;
            //isDest = false;

            lastP = null;
        }
    }

    void Awake()
    {
        fieldManager = gameObject.GetComponent<FieldManager>();
    }

    void init()
    {
        fieldManager = gameObject.GetComponent<FieldManager>();
        for (int i = 0; i < FieldManager.MAX_ROW; i++)
        {
            for (int j = 0; j < FieldManager.MAX_COL; j++)
            {
                // field의 각 좌표에 WayPoint를 하나씩 생성
                wayPoint[i, j] = new WayPoint(i, j);
            }
        }
        openList = new List<WayPoint>();
        closeList = new List<WayPoint>();
    }

    // 본 함수의 호출로써 길 찾기 시작
    //  본 함수는 CharacterController에서 호출?
    // parameter : (시작 좌표, 도착 좌표)
    // 도착지점으로의 이동 좌표를 2차원 배열로 반환
    //public List<WayPoint> startPathFinding(WayPoint startP, WayPoint destP)
    public int[,] startPathFinding(int startX, int startY, int destX, int destY)
    {
        init();
        //WayPoint startP = new WayPoint(startX, startY);
        WayPoint curP = new WayPoint(startX, startY);
        WayPoint destP = new WayPoint(destX, destY);

        //현재 탐색이 진행중인 좌표
        //WayPoint curP = startP;
        

        // 도착 지점을 정의
        //destP.isDest = true;
        // 시작 지점을 열린 목록에 저장
        openList.Add(curP);
        // 1. 시작사각형의 인접사각형(자신 제외)들을 열린목록에 추가
        checkAround(destP, true);

        
        // 시작 지점을 열린 목록에서 삭제하고 닫힌 목록에 저장
        openList.Remove(curP);
        closeList.Add(curP);
        
    
        // 2. 다음 과정 반복 ( 함수 반복 호출 )
    
        // checkAround() -> 열린 목록에서 다음 노드 선택
        // GHF 계산
        // 작은 F 선택
        // 포인터(돌아갈 곳) 설정
        // 반복
        //checkAround(curP, destP);
        // 부모 노드로 연결된 최종 도착지점의 노드를 반환
        WayPoint a = checkAround(destP);

        // 경로가 없다면 종료
        if (a == null) return null;

        // 의자로 가는 중이라면. 해당 의자에 중복으로 앉을 수 없게 설정
        if (fieldManager.field[a.x, a.y].GetComponent<ArrangeablePosState>().getState() == 4)
            // 해당 필드(fieldManager.field[][])의 도착지점 state를 5(unsitable)로 변경
            fieldManager.field[a.x, a.y].GetComponent<ArrangeablePosState>().setState(5);

        // 출구로 가는 중이라면.
        if (fieldManager.field[a.x, a.y].GetComponent<ArrangeablePosState>().getState() == 3)
        {
            // 앉아있던 의자의 state가 여전히 4(unsitable)이라면
            if(fieldManager.field[startX, startY].GetComponent<ArrangeablePosState>().getState() == 5)
            // 앉아있던 의자의 state를 4(sitable)으로 변경
            fieldManager.field[startX, startY].GetComponent<ArrangeablePosState>().setState(4);
        }


        // 경로 저장
        List<WayPoint> route = new List<WayPoint>();


        if (a == null)
        {
            Debug.Log("경로 x");
            return null;
        }
        while (a.lastP != null)
        {
            // 도착 지점 노드부터 출발 지점까지 거꾸로 저장됨
            route.Add(a);
            a = a.lastP;
        }
        
        int[,] routeArray = new int[2, route.Count];
        int i = 0;
        foreach(var tmp in route)
        {
            routeArray[0, i] = route[i].x;
            routeArray[1, i] = route[i].y;
            i++;
        }


        //// 경로 반환
        //for(int j = 0; j < routeArray.Length / 2; j++)
        //{

        //}
        
        return routeArray;
    }
    

    WayPoint checkAround(WayPoint dest, bool startProcess = false)
    {
        //첫 시작 노드의 주변을 확인하는 프로세스라면.
        //무한루프x. 주변 노드들 GHF 계산 후 종료
        if (startProcess)
        {
            // 유일하게 추가되어있는 시작 노드
            WayPoint curP = openList[0];

            // currentPoint를 둘러싼 주변 8칸의 블럭을 체크
            for (int x = curP.x - 1; x < curP.x + 2; x++)
            {
                for (int y = curP.y - 1; y < curP.y + 2; y++)
                {
                    // 정상 체크 범위를 벗어나지 않았는지 여부를 체크
                    if (!(x < 0 || y < 0 || x >= fieldManager.CUR_ROW || y >= fieldManager.CUR_COL))
                    {
                        // 지나갈 수 있는 길인지의 여부를 체크
                        // 길 혹은 출구 혹은 의자인지 여부 체크
                        int tmpState = fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState();
                        //if (fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState() == 0 || fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState() == 3 || fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState() == 4)
                        //if(tmpState == 0 || tmpState == 3 || tmpState == 4)
                        if (tmpState == 0 || tmpState == 3 )
                        {
                            // CloseList에 포함되어 있지 않다면
                            if (!isInList(closeList, wayPoint[x, y]))
                            {

                                // 대각선이 아닌, 직선상에 위치한다면 G 10 증가
                                // 대각선상에 위치한다면 G 14 증가(약 루트2)
                                if (curP.x == x || curP.y == y)
                                {
                                    // OpenList에 포함되어 있다면
                                    // 이미 탐색된 전적이 있는 블록이라면
                                    if (isInList(openList, wayPoint[x, y]))
                                    {
                                        // 더 짧은 길을 탐색하는 코드
                                        if (wayPoint[x, y].G > curP.G + 10)
                                        {
                                            wayPoint[x, y].G = curP.G + 10;
                                            wayPoint[x, y].lastP = curP; // 부모 노드 교체
                                        }
                                    }
                                    // OpenList에 포함되어 있지 않다면 - 신규 탐색
                                    else
                                    {
                                        wayPoint[x, y].G = 10;
                                        wayPoint[x, y].lastP = curP;
                                        openList.Add(wayPoint[x, y]);
                                    }
                                }

                                /*
                                // 대각선상에 위치
                                else
                                {
                                    // OpenList에 포함되어 있다면
                                    // 이미 탐색된 전적이 있는 블록이라면
                                    //if (wayPoint[x, y].G != 0)
                                    if (isInList(openList, wayPoint[x, y]))
                                    {
                                        // 더 짧은 길을 탐색하는 코드
                                        if (wayPoint[x, y].G > curP.G + 14)
                                        {
                                            wayPoint[x, y].G = curP.G + 14;
                                            wayPoint[x, y].lastP = curP; // 부모 노드 교체
                                        }
                                    }
                                    // OpenList에 포함되어 있지 않다면 - 신규 탐색
                                    else
                                    {
                                        wayPoint[x, y].G = 14;
                                        wayPoint[x, y].lastP = curP;
                                        openList.Add(wayPoint[x, y]);
                                    }
                                }
                                */

                                // 각 wayPoint로부터 도착지점(dest)까지의 예상 이동 비용
                                wayPoint[x, y].H = 0;
                                for (int i = x, j = y; i != dest.x && j != dest.y;)
                                {
                                    if (i < dest.x) i++;
                                    else if (i > dest.x) i--;
                                    else
                                    {
                                        if (j < dest.y) j++;
                                        else if (j > dest.y) j--;
                                    }
                                    wayPoint[x, y].H += 10;
                                }
                                wayPoint[x, y].F = wayPoint[x, y].G + wayPoint[x, y].H;

                                /*
                                //aroundCheck()종료 후 진행할 다음 블럭
                                if (nextP == null) { nextP = wayPoint[x, y]; }
                                else
                                {
                                    // 목적지까지 더 빨리 갈 수 있는 블럭을 선택
                                    if (nextP.F > wayPoint[x, y].F) nextP = wayPoint[x, y];
                                }
                                */
                            }
                        }
                    }
                }
            } // 주변의 8칸 체크 완료
            return null;
        }
        else
        {
            // 목적지(dest)에 도착했거나, 경로가 없다면(openList가 비게되면) 종료된다 
            while (true)
            {
                // openList가 비었다면
                // == 길이 없다면?
                if (openList.Count == 0)
                {
                    Debug.Log("탐색 가능한 루트가 존재하지 않습니다");
                    return null;
                }
                //Debug.Log("실행은 되어씁니다");
                //열린 목록 중 가장 낮은 F비용의 좌표를 탐색, 후 현재 사각형으로 선택
                WayPoint curP = openList[0];
                // 열린 목록들을 담았다가 동일한 비용의 경로 중 랜덤으로 하나를 추출.
                // 한 목표 지점으로 다르게 나아가게 하기 위함
                List<WayPoint> tmpOpenList = new List<WayPoint>();

                for (int i = 1; i < openList.Count; i++)
                {
                    if(openList[i].F == curP.F)
                    {
                        tmpOpenList.Add(openList[i]);
                    }
                    else if (openList[i].F < curP.F)
                    {
                        tmpOpenList.Clear();
                        //min 은 열린목록 중 가장 낮은 F 비용
                        curP = openList[i];
                    }
                }
                if(tmpOpenList.Count > 0)
                {
                    curP = tmpOpenList[Random.Range(0, tmpOpenList.Count)];
                }
                /*
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].F < curP.F)
                    {
                        //min 은 열린목록 중 가장 낮은 F 비용
                        curP = openList[i];
                    }
                }
                */
                //Debug.Log(curP.F);
                // 열린 목록에 목적지가 추가되었다면
                // curP가 목적지를 가리키는 좌표.
                // curP의 부모 좌표를 탐색해 경로 확인 가능
                if (curP.x == dest.x && curP.y == dest.y)
                {
                    return curP;
                }

                // F가 가장 작았던 좌표에 대해 열린 목록에서 제거 후 닫힌 목록에 추가
                // 다시 탐색 x
                openList.Remove(curP);
                closeList.Add(curP);

                // 다음 나아갈 블럭, F값이 가장 작은 블럭
                //WayPoint nextP = null;

                // currentPoint를 둘러싼 주변 8칸의 블럭을 체크
                for (int x = curP.x - 1; x < curP.x + 2; x++)
                {
                    for (int y = curP.y - 1; y < curP.y + 2; y++)
                    {
                        // 정상 체크 범위를 벗어나지 않았는지 여부를 체크
                        if (!(x < 0 || y < 0 || x >= fieldManager.CUR_ROW || y >= fieldManager.CUR_COL))
                        {
                            // 지나갈 수 있는 길인지의 여부를 체크
                            // 길 혹은 출구 혹은 의자인지 여부 체크
                            int tmpState = fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState();
                            //if (fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState() == 0 || fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState() == 3 || fieldManager.field[x, y].GetComponent<ArrangeablePosState>().getState() == 4)
                            if (tmpState == 0 || tmpState == 3 || tmpState == 4)
                            {

                                
                                // CloseList에 포함되어 있지 않다면
                                if (!isInList(closeList, wayPoint[x, y]))
                                {

                                    // 대각선이 아닌, 직선상에 위치한다면 G 10 증가
                                    // 대각선상에 위치한다면 G 14 증가(약 루트2)
                                    if (curP.x == x || curP.y == y)
                                    {
                                        // OpenList에 포함되어 있다면
                                        // 이미 탐색된 전적이 있는 블록이라면
                                        //if (wayPoint[x, y].G != 0)
                                        if (isInList(openList, wayPoint[x, y]))
                                        {
                                            // 더 짧은 길을 탐색하는 코드
                                            if (wayPoint[x, y].G > curP.G + 10)
                                            {
                                                wayPoint[x, y].G = curP.G + 10;
                                                wayPoint[x, y].lastP = curP; // 부모 노드 교체
                                            }


                                        }
                                        // OpenList에 포함되어 있지 않다면 - 신규 탐색
                                        else
                                        {
                                            wayPoint[x, y].G = 10;
                                            wayPoint[x, y].lastP = curP;
                                            openList.Add(wayPoint[x, y]);
                                        }
                                    }
                                    /*
                                    // 대각선상에 위치
                                    else
                                    {
                                        // OpenList에 포함되어 있다면
                                        // 이미 탐색된 전적이 있는 블록이라면
                                        //if (wayPoint[x, y].G != 0)
                                        if (isInList(openList, wayPoint[x, y]))
                                        {
                                            // 더 짧은 길을 탐색하는 코드
                                            if (wayPoint[x, y].G > curP.G + 14)
                                            {
                                                wayPoint[x, y].G = curP.G + 14;
                                                wayPoint[x, y].lastP = curP; // 부모 노드 교체
                                            }


                                        }
                                        // OpenList에 포함되어 있지 않다면 - 신규 탐색
                                        else
                                        {
                                            wayPoint[x, y].G = 14;
                                            wayPoint[x, y].lastP = curP;
                                            openList.Add(wayPoint[x, y]);

                                        }
                                    }
                                    */

                                    // 각 wayPoint로부터 도착지점(dest)까지의 예상 이동 비용
                                    wayPoint[x, y].H = 0;
                                    for (int i = x, j = y; i != dest.x && j != dest.y;)
                                    {
                                        if (i < dest.x) i++;
                                        else if (i > dest.x) i--;
                                        else
                                        {
                                            if (j < dest.y) j++;
                                            else if (j > dest.y) j--;
                                        }
                                        wayPoint[x, y].H += 10;
                                    }
                                    wayPoint[x, y].F = wayPoint[x, y].G + wayPoint[x, y].H;
                                    
                                }
                            }
                        }
                    }
                } // 주변의 8칸 체크 완료
            }
        }
    }

    //OpenList에 담겨있는 WayPoint 인지 확인하는 함수
    bool isInList(List<WayPoint> list, WayPoint target)
    {
        foreach(var m in list)
        {
            if (m.x == target.x && m.y == target.y) return true;
        }
        return false;
    }
    
}
