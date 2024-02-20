using Array2DEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GenerateOptions
{
    ByCells,
    ByRooms
}

public class RoomsAssembler : MonoBehaviour
{
    [SerializeField] Vector3 LevelOrigin;

    //LevelSize represents cell amount or rooms amount
    //[SerializeField] bool SizeOfRoomDoesntMatter = true;
    [SerializeField] int LevelSize = 10;
    [SerializeField] GenerateOptions GenerateBy;
    bool GenerateByCells;
    int currentLevelSize = 0;

    [SerializeField] float CellSize = 3;
    [SerializeField] float CellSpace = 3;

    [SerializeField] GameObject startRoom;
    [SerializeField] GameObject bossRoom;
    [SerializeField] GameObject bridge;
    [SerializeField] GameObject[] uniqueRooms;
    [SerializeField] GameObject[] rooms;
    Dictionary<int, List<GameObject>> roomsBySize = new Dictionary<int, List<GameObject>>();

    int levelGridSize = 0;
    //represents busy cells    1 for current room, -1 for placed room
    int[,] levelCellsMatrix; 
    GameObject[,] levelRoomsMatrix; //represents origins of rooms

    bool roomsCreated = false;


    //Represents a place for bridge
    struct FreeWays
    {
        public bool up;
        public bool down;
        public bool left;
        public bool right;

        public FreeWays(int x = 0)
        {
            up = false;
            down = false;
            left = false;
            right = false;
        }
    }
    FreeWays[,] freeWaysMatrix;

    void Start()
    {
        if (GenerateBy == GenerateOptions.ByCells)
            GenerateByCells = true;
        else
            GenerateByCells = false;


        #region ERRORS AND INITIALIZATION
        //Boss room
        if (bossRoom.GetComponent<BossRoomParam>() == null)
        {
            UnityEngine.Debug.LogError($"<color=red>Boss room is missing RoomParam component!</color>");
            return;
        }
        if (bossRoom.GetComponent<BossRoomParam>().RoomMatrix.GridSize.x != bossRoom.GetComponent<BossRoomParam>().RoomMatrix.GridSize.y)
        {
            UnityEngine.Debug.LogError($"<color=red>Boss room: RoomMatrix is not square!</color>");
            return;
        }

        //Unique rooms
        foreach (GameObject room in uniqueRooms)
        {
            if (room.GetComponent<RoomParam>() == null)
            {
                UnityEngine.Debug.LogError($"<color=red>{room.name} is missing RoomParam component!</color>");
                return;
            }

            Array2DBool currentMatrix = room.GetComponent<RoomParam>().RoomMatrix;
            if (currentMatrix.GridSize.y != currentMatrix.GridSize.x)
            {
                UnityEngine.Debug.Log($"<color=red>{room.name}: RoomMatrix is not square!</color>");
                return;
            }
        }

        //Other rooms
        foreach (GameObject room in rooms)
        {
            if (room.GetComponent<RoomParam>() == null)
            {
                UnityEngine.Debug.LogError($"<color=red>{room.name} is missing RoomParam component!</color>");
                return;
            }

            Array2DBool currentMatrix = room.GetComponent<RoomParam>().RoomMatrix;
            if (currentMatrix.GridSize.y != currentMatrix.GridSize.x)
            {
                UnityEngine.Debug.Log($"<color=red>{room.name}: RoomMatrix is not square!</color>");
                return;
            }

            int roomSize = currentMatrix.GridSize.x;

            if (!roomsBySize.ContainsKey(roomSize))
                roomsBySize[roomSize] = new List<GameObject>();

            roomsBySize[roomSize].Add(room);
        }
        #endregion

        //Finding the most pesimistic variant of rooms arrangement (straight line of roms)
        if (GenerateByCells)
            levelGridSize = LevelSize;
        else
            levelGridSize = LevelSize * roomsBySize.Keys.Max();

        levelGridSize += levelGridSize + 1; //+1 for starting room at the middle

        levelCellsMatrix = new int[levelGridSize, levelGridSize];
        levelRoomsMatrix = new GameObject[levelGridSize, levelGridSize];
        freeWaysMatrix = new FreeWays[levelGridSize, levelGridSize];

        levelCellsMatrix[levelGridSize / 2, levelGridSize / 2] = -1;
        levelRoomsMatrix[levelGridSize / 2, levelGridSize / 2] = startRoom;
        freeWaysMatrix[levelGridSize / 2, levelGridSize / 2].up = true;
        freeWaysMatrix[levelGridSize / 2, levelGridSize / 2].down = true;
        freeWaysMatrix[levelGridSize / 2, levelGridSize / 2].left = true;
        freeWaysMatrix[levelGridSize / 2, levelGridSize / 2].right = true;

    }

    void Update()
    {
        if (currentLevelSize < LevelSize)
            AddRoom();

        else if (!roomsCreated)
        {
            AddBossRoom();
            AddSpecialRooms();
            DrawRooms();
            roomsCreated = true;
        }

    }
        
    void DrawRooms()
    {
        //Drawing rooms from room origin matrix
        for (int j = 0; j < levelGridSize; j++)
            for (int i = 0; i < levelGridSize; i++)
                if (levelRoomsMatrix[i, j] != null)
                    Instantiate(
                        levelRoomsMatrix[i, j], 
                        new Vector3(
                            LevelOrigin.x + (CellSize + CellSpace) * i - (CellSize + CellSpace) * levelGridSize / 2 + (CellSize + CellSpace) / 2,
                            LevelOrigin.y,
                            LevelOrigin.z - (CellSize + CellSpace) * j + (CellSize + CellSpace) * levelGridSize / 2 - (CellSize + CellSpace) / 2), 
                        Quaternion.identity
                        );

        //Drawing bridges from freeWayMatrix
        for (int j = 0; j < levelGridSize - 1; j++)
        {
            for (int i = 0; i < levelGridSize - 1; i++)
            {
                if (freeWaysMatrix[i, j].right && freeWaysMatrix[i + 1, j].left)
                    Instantiate(
                            bridge,
                            new Vector3(
                                LevelOrigin.x - (CellSize + CellSpace) * levelGridSize / 2 + (CellSize + CellSpace) * i + (CellSize + CellSpace) / 2 + (CellSize + CellSpace) / 2,
                                LevelOrigin.y,
                                LevelOrigin.z - (CellSize + CellSpace) * j + (CellSize + CellSpace) * levelGridSize / 2 - (CellSize + CellSpace) / 2
                                ),
                            Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f))
                            );

                if (freeWaysMatrix[i, j].down && freeWaysMatrix[i, j + 1].up)
                    Instantiate(
                                bridge,
                                new Vector3(
                                    LevelOrigin.x - (CellSize + CellSpace) * levelGridSize / 2 + (CellSize + CellSpace) * i + (CellSize + CellSpace) / 2,
                                    LevelOrigin.y,
                                    LevelOrigin.z - (CellSize + CellSpace) * j + (CellSize + CellSpace) * levelGridSize / 2 - (CellSize + CellSpace) 
                                    ),
                                Quaternion.identity
                                );
            }
        }

        #region DEBUG VISUALIZER
                //For visualize on debugger
                //string line = "\n";
                //for (int j = 0; j < levelGridSize; j++)
                //{
                //    for (int i = 0; i < levelGridSize; i++)
                //    {
                //        if (levelCellsMatrix[i, j] == -1)
                //            line += "X";
                //        else if (levelCellsMatrix[i, j] == 1)
                //            line += "H";
                //        else
                //            line += "O";
                //    }
                //    line += "\n";
                //}
                //UnityEngine.Debug.Log(line);

                //line = "\n";
                //for (int j = 0; j < levelGridSize; j++)
                //{
                //    for (int i = 0; i < levelGridSize; i++)
                //    {
                //        if (levelRoomsMatrix[i, j] != null)
                //            line += "X";
                //        else
                //            line += "O";
                //    }
                //    line += "\n";
                //}
                //UnityEngine.Debug.Log(line);
                //}
                #endregion
    }

    void AddRoom()
    {
        int currentRoomSize = 1;
        int currentRoomIndex = 0;
        GameObject currentRoom;
        int[,] currentRoomMatrix;

        int onGridPosX = 0;
        int onGridPosY = 0;

        #region GETTING NEW ROOM    
        //IF SIZE DOESNT MATTER
        //Pulling out all avaliable keys (sizes)
        List<int> keysList = new List<int>(roomsBySize.Keys);

        int randomSizeIndex = 0;
        if (GenerateByCells) //Checking free space to prevent overflow
        {
            int freeSpace = LevelSize - currentLevelSize;

            List<GameObject> avaliableRoomsList = new List<GameObject>();

            //If room with specified size doesnt exists - use bigger one
            int overflowTolerance = 0;
            while (avaliableRoomsList.Count == 0)
            {
                foreach (GameObject room in rooms)
                {
                    Array2DBool matrixToCheck = room.GetComponent<RoomParam>().RoomMatrix;
                    int numberOfCells = 0;
                               
                    for (int j = 0; j < matrixToCheck.GridSize.x; j++)
                        for (int i = 0; i < matrixToCheck.GridSize.x; i++)
                            if (matrixToCheck.GetCell(i, j) == true)
                                numberOfCells++;

                    if (numberOfCells <= freeSpace + overflowTolerance)
                        avaliableRoomsList.Add(room);
                }
                overflowTolerance++;
            }

            currentRoomIndex = Random.Range(0, avaliableRoomsList.Count);
            currentRoom = avaliableRoomsList[currentRoomIndex];
            currentRoomSize = currentRoom.GetComponent<RoomParam>().RoomMatrix.GridSize.x;
        }
        else
        {
            randomSizeIndex = Random.Range(0, keysList.Count);
            currentRoomSize = keysList[randomSizeIndex];

            currentRoomIndex = Random.Range(0, roomsBySize[currentRoomSize].Count);
            currentRoom = roomsBySize[currentRoomSize][currentRoomIndex];
        }

        currentRoomMatrix = new int[currentRoomSize, currentRoomSize];


        for (int j = 0; j < currentRoomSize; j++)
        {
            for (int i = 0; i < currentRoomSize; i++)
            {
                if (currentRoom.GetComponent<RoomParam>().RoomMatrix.GetCell(i, j))
                    currentRoomMatrix[i, j] = 1;
                else
                    currentRoomMatrix[i, j] = 0;
            }
        }
        #endregion

        //Create temporary room model on the border
        //0 up, 1 right, 2 down, 3 left
        int roomInitPosition = Random.Range(0, 4);

        switch (roomInitPosition)
        {
            case 0:
                onGridPosX = Random.Range(0, levelGridSize - currentRoomSize);
                onGridPosY = 0;
                break;
            case 1:
                onGridPosX = levelGridSize - currentRoomSize;
                onGridPosY = Random.Range(0, levelGridSize - currentRoomSize);
                break;
            case 2:
                onGridPosX = Random.Range(0, levelGridSize - currentRoomSize);
                onGridPosY = levelGridSize - currentRoomSize;
                break;
            case 3:
                onGridPosX = 0;
                onGridPosY = Random.Range(0, levelGridSize - currentRoomSize);
                break;
        }

        #region CENTERING BY ORIGIN
        //Push temporary model into the center of a level
        bool centered = false;
        int centerIndex = levelGridSize / 2;

        //Bugfix - when room had origin not in (0, 0) algorithm could not work near the center of a grid
        int onGridOriginPosX = onGridPosX + currentRoom.GetComponent<RoomParam>().OriginCoordinateX;
        int onGridOriginPosY = onGridPosY + currentRoom.GetComponent<RoomParam>().OriginCoordinateY;

        //To go in a straight line from beggining rather than only in diagonal
        //When crntRatio > initRatio - move horizontally
        //When crntRatio < initRation - move vetically
        float initAxisRatio;
        float disntanceInX = Mathf.Abs(levelGridSize / 2 - onGridOriginPosX);
        float disntanceInY = Mathf.Abs(levelGridSize / 2 - onGridOriginPosY);
        if (disntanceInY != 0)
            initAxisRatio = disntanceInX / disntanceInY;
        else
            initAxisRatio = 1;

        int overflowCheck = 0;
        while (!centered && overflowCheck < LevelSize * roomsBySize.Keys.Max() * 10)
        {
            onGridOriginPosX = onGridPosX + currentRoom.GetComponent<RoomParam>().OriginCoordinateX;
            onGridOriginPosY = onGridPosY + currentRoom.GetComponent<RoomParam>().OriginCoordinateY;

            disntanceInX = Mathf.Abs(levelGridSize / 2 - onGridOriginPosX);
            disntanceInY = Mathf.Abs(levelGridSize / 2 - onGridOriginPosY);
            float currentAxisRatio;
            if (disntanceInY == 0)
            {
                currentAxisRatio = 1;
                initAxisRatio = 0;
            }
            else if (disntanceInX == 0)
            {
                currentAxisRatio = 0;
                initAxisRatio = 1;
            }
            else
                currentAxisRatio = disntanceInX / disntanceInY;


            //Move horizontally
            if (currentAxisRatio >= initAxisRatio)
            {
                //Left side
                if (onGridOriginPosX < centerIndex)
                {
                    for (int j = 0; j < currentRoomSize; j++)
                        for (int i = 0; i < currentRoomSize; i++)
                            if (currentRoomMatrix[i, j] * levelCellsMatrix[onGridPosX + i + 1, onGridPosY + j] < 0)
                                centered = true;

                    if (!centered)
                        onGridPosX++;
                }

                //Right side
                if (onGridOriginPosX > centerIndex)
                {
                    for (int j = 0; j < currentRoomSize; j++)
                        for (int i = 0; i < currentRoomSize; i++)
                            if (currentRoomMatrix[i, j] * levelCellsMatrix[onGridPosX + i - 1, onGridPosY + j] < 0)
                                centered = true;

                    if (!centered)
                        onGridPosX--;
                }
            }

            //Move vertically
            else if (currentAxisRatio < initAxisRatio)
            {
                //Up side
                if (onGridOriginPosY < centerIndex)
                {
                    for (int j = 0; j < currentRoomSize; j++)
                        for (int i = 0; i < currentRoomSize; i++)
                            if (currentRoomMatrix[i, j] * levelCellsMatrix[onGridPosX + i, onGridPosY + j + 1] < 0)
                                centered = true;

                    if (!centered)
                        onGridPosY++;
                }

                //Down side
                if (onGridOriginPosY > centerIndex)
                {
                    for (int j = 0; j < currentRoomSize; j++)
                        for (int i = 0; i < currentRoomSize; i++)
                            if (currentRoomMatrix[i, j] * levelCellsMatrix[onGridPosX + i, onGridPosY + j - 1] < 0)
                                centered = true;

                    if (!centered)
                        onGridPosY--;
                }
            }

            overflowCheck++;
        }


        if (overflowCheck >= LevelSize * roomsBySize.Keys.Max() * 10)
            UnityEngine.Debug.LogError("Overflow!");

        for (int j = 0; j < currentRoomSize; j++)
            for (int i = 0; i < currentRoomSize; i++)
                if (currentRoomMatrix[i, j] == 1)
                    levelCellsMatrix[i + onGridPosX, j + onGridPosY] = -1;
        #endregion

        levelRoomsMatrix[onGridPosX + currentRoom.GetComponent<RoomParam>().OriginCoordinateX, onGridPosY + currentRoom.GetComponent<RoomParam>().OriginCoordinateY] = currentRoom;

        #region SETTING BRIDGES
        //Checking where is no place for bridge
        //Current room matrix is used be cause of possibility of another room in the same grid space on levelCellMatrix
        for (int j = 0; j < currentRoomSize; j++)
        {
            for (int i = 0; i < currentRoomSize; i++)
            {
                if (currentRoomMatrix[i, j] == 1)
                {
                    if (i != 0)
                    {
                        if (currentRoomMatrix[i - 1, j] == 0)
                            freeWaysMatrix[onGridPosX + i, onGridPosY + j].left = true;
                    }
                    else
                        freeWaysMatrix[onGridPosX + i, onGridPosY + j].left = true;

                    if (i != currentRoomSize - 1)
                    {
                        if (currentRoomMatrix[i + 1, j] == 0)
                            freeWaysMatrix[onGridPosX + i, onGridPosY + j].right = true;
                    }
                    else
                        freeWaysMatrix[onGridPosX + i, onGridPosY + j].right = true;

                    if (j != 0)
                    {
                        if (currentRoomMatrix[i, j - 1] == 0)
                            freeWaysMatrix[onGridPosX + i, onGridPosY + j].up = true;
                    }
                    else
                        freeWaysMatrix[onGridPosX + i, onGridPosY + j].up = true;

                    if (j != currentRoomSize - 1)
                    {
                        if (currentRoomMatrix[i, j + 1] == 0)
                            freeWaysMatrix[onGridPosX + i, onGridPosY + j].down = true;
                    }
                    else
                        freeWaysMatrix[onGridPosX + i, onGridPosY + j].down = true;
                }
            }
        }
        #endregion


        //Increase number of existing rooms
        if (GenerateByCells)
        {
            int numberOfCellsInCurrentRoom = 0;

            for (int j = 0; j < currentRoomSize; j++)
                for (int i = 0; i < currentRoomSize; i++)
                    if (currentRoomMatrix[i, j] == 1)
                        numberOfCellsInCurrentRoom++;

            currentLevelSize += numberOfCellsInCurrentRoom;
        }
        else
            currentLevelSize++;
    }

    struct coords
    {
        public int distance;
        public int x; public int y;
    }

    void AddBossRoom()
    {
        //Finding all cells which are neighbours of regular cells (-1)
        List<coords> candidates = new List<coords>();
        for (int j = 1; j < levelGridSize - 1; j++)
        {
            for (int i = 1; i < levelGridSize - 1; i++)
            {
                if (levelCellsMatrix[i, j] == 0)
                {
                    if (levelCellsMatrix[i + 1, j] == -1 ||
                        levelCellsMatrix[i - 1, j] == -1 ||
                        levelCellsMatrix[i, j + 1] == -1 ||
                        levelCellsMatrix[i, j - 1] == -1)
                    {
                        coords newCoords;
                        newCoords.x = i;
                        newCoords.y = j;
                        newCoords.distance = Mathf.Abs(levelGridSize / 2 - newCoords.x) + Mathf.Abs(levelGridSize / 2 - newCoords.y);
                        candidates.Add(newCoords);
                    }
                }
            }
        }

        List<coords> sortedCandidates = candidates.OrderByDescending(c => c.distance).ToList();

        //Pulling data from boss room
        BossRoomParam bossRoomParam = bossRoom.GetComponent<BossRoomParam>();
        int[,] bossRoomMatrix = new int [bossRoomParam.RoomMatrix.GridSize.x, bossRoomParam.RoomMatrix.GridSize.x];
        for (int j = 0; j < bossRoomParam.RoomMatrix.GridSize.x; j++)
            for (int i = 0; i < bossRoomParam.RoomMatrix.GridSize.x; i++)
                if (bossRoomParam.RoomMatrix.GetCell(i, j))
                    bossRoomMatrix[i, j] = 1;

        //Checking all possible places from cell candidates
        bool bossRoomInserted = false;
        for (int c = 0; c < sortedCandidates.Count && !bossRoomInserted; c++)
        {
            bossRoomInserted = true;

            //Treating entrance point as contact point
            int onGridPosX = sortedCandidates[c].x - bossRoomParam.EntranceCoordinatesX;
            int onGridPosY = sortedCandidates[c].y - bossRoomParam.EntranceCoordinatesY;

            //Rejection if collision is detected
            for (int j = 0; j < bossRoomParam.RoomMatrix.GridSize.x; j++)
                for (int i = 0; i < bossRoomParam.RoomMatrix.GridSize.x; i++)
                    if (levelCellsMatrix[onGridPosX + i, onGridPosY + j] * bossRoomMatrix[i, j] < 0)
                        bossRoomInserted = false;

            //If no collision detected then override level matrices
            //Special rooms tagged as -2 so it wont count as candidate next time
            if (bossRoomInserted)
            {
                for (int j = 0; j < bossRoomParam.RoomMatrix.GridSize.x; j++)
                    for (int i = 0; i < bossRoomParam.RoomMatrix.GridSize.x; i++)
                        levelCellsMatrix[onGridPosX + i, onGridPosY + j] = -2;

                levelRoomsMatrix[onGridPosX + bossRoomParam.OriginCoordinateX, onGridPosY + bossRoomParam.OriginCoordinateY] = bossRoom;

                #region SETTING BRIDGES
                for (int j = 0; j < bossRoomParam.RoomMatrix.GridSize.x; j++)
                {
                    for (int i = 0; i < bossRoomParam.RoomMatrix.GridSize.x; i++)
                    {
                        if (bossRoomMatrix[i, j] == 1)
                        {
                            if (i != 0)
                            {
                                if (bossRoomMatrix[i - 1, j] == 0)
                                    freeWaysMatrix[onGridPosX + i, onGridPosY + j].left = true;
                            }
                            else
                                freeWaysMatrix[onGridPosX + i, onGridPosY + j].left = true;

                            if (i != bossRoomParam.RoomMatrix.GridSize.x - 1)
                            {
                                if (bossRoomMatrix[i + 1, j] == 0)
                                    freeWaysMatrix[onGridPosX + i, onGridPosY + j].right = true;
                            }
                            else
                                freeWaysMatrix[onGridPosX + i, onGridPosY + j].right = true;

                            if (j != 0)
                            {
                                if (bossRoomMatrix[i, j - 1] == 0)
                                    freeWaysMatrix[onGridPosX + i, onGridPosY + j].up = true;
                            }
                            else
                                freeWaysMatrix[onGridPosX + i, onGridPosY + j].up = true;

                            if (j != bossRoomParam.RoomMatrix.GridSize.x - 1)
                            {
                                if (bossRoomMatrix[i, j + 1] == 0)
                                    freeWaysMatrix[onGridPosX + i, onGridPosY + j].down = true;
                            }
                            else
                                freeWaysMatrix[onGridPosX + i, onGridPosY + j].down = true;
                        }
                    }
                    #endregion
                }
            }
        }

        if (!bossRoomInserted)
            UnityEngine.Debug.LogError($"<color=red>Boss Room: there is no space!</color>");
    }

    void AddSpecialRooms()
    {
        //Finding all cells which are neighbours of regular cells (-1)
        List<coords> candidates = new List<coords>();
        for (int j = 1; j < levelGridSize - 1; j++)
        {
            for (int i = 1; i < levelGridSize - 1; i++)
            {
                if (levelCellsMatrix[i, j] == 0)
                {
                    if (levelCellsMatrix[i + 1, j] == -1 ||
                        levelCellsMatrix[i - 1, j] == -1 ||
                        levelCellsMatrix[i, j + 1] == -1 ||
                        levelCellsMatrix[i, j - 1] == -1)
                    {
                        coords newCoords;
                        newCoords.x = i;
                        newCoords.y = j;
                        newCoords.distance = Mathf.Abs(levelGridSize / 2 - newCoords.x) + Mathf.Abs(levelGridSize / 2 - newCoords.y);
                        candidates.Add(newCoords);
                    }
                }
            }
        }

        //Placing every unique room in random avaliable place
        foreach (GameObject uniqueRoom in uniqueRooms)
        {
            if (candidates.Count > 0)
            {
                int randomCellIndex = Random.Range(0, candidates.Count);
                coords randomCell = candidates[randomCellIndex];
                levelCellsMatrix[randomCell.x, randomCell.y] = -2;
                levelRoomsMatrix[randomCell.x, randomCell.y] = uniqueRoom;
                freeWaysMatrix[randomCell.x, randomCell.y].up = true;
                freeWaysMatrix[randomCell.x, randomCell.y].down = true;
                freeWaysMatrix[randomCell.x, randomCell.y].left = true;
                freeWaysMatrix[randomCell.x, randomCell.y].right = true;
                candidates.Remove(randomCell);
            }
            else
                UnityEngine.Debug.Log($"<color=red>Unique room \"{uniqueRoom.name}\": There is no space!</color>");

        }
    }
}