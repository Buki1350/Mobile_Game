using Array2DEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsAssembler : MonoBehaviour
{
    [SerializeField] Vector3 LevelOrigin;

    //LevelSize represents cell amount or rooms amount
    [SerializeField] bool GenerateByCells = true;
    [SerializeField] bool SizeOfRoomDoesntMatter = true;
    [SerializeField] int LevelSize = 10;
    int currentLevelSize = 0;

    [SerializeField] float CellSize = 3;
    [SerializeField] float CellSpace = 3;

    [SerializeField] GameObject startRoom;
    [SerializeField] GameObject[] rooms;
    Dictionary<int, List<GameObject>> roomsBySize = new Dictionary<int, List<GameObject>>();

    int levelGridSize = 0;
    //represents busy cells    1 for current room, -1 for placed room
    int[,] levelCellsMatrix; 
    GameObject[,] levelRoomsMatrix; //represents origins of rooms

    bool IsLevelDrown = false;

    void Start()
    {
        foreach (GameObject room in rooms)
        {
            RoomParam roomParam = room.GetComponent<RoomParam>();
            if (roomParam == null)
            {
                Debug.LogError($"<color=red>{room.name} is missing RoomParam component!</color>");
                return;
            }

            Array2DBool currentMatrix = roomParam.RoomMatrix;
            if (currentMatrix.GridSize.y != currentMatrix.GridSize.x)
            {
                Debug.Log($"<color=red>{room.name}: RoomMatrix is not square!</color>");
                return;
            }

            int roomSize = currentMatrix.GridSize.x;

            if (!roomsBySize.ContainsKey(roomSize))
                roomsBySize[roomSize] = new List<GameObject>();

            roomsBySize[roomSize].Add(room);
        }

        //Finding the most pesimistic variant of rooms arrangement (straight line of roms)
        foreach (var kvp in roomsBySize)
        {
            //Debug.Log($"Rooms of size {kvp.Key}: {kvp.Value.Count}");
            levelGridSize += kvp.Key * kvp.Value.Count;
        }
        levelGridSize += levelGridSize + 1; //+1 for starting room at the middle

        levelCellsMatrix = new int[levelGridSize, levelGridSize];
        levelRoomsMatrix = new GameObject[levelGridSize, levelGridSize];

        levelCellsMatrix[levelGridSize / 2, levelGridSize / 2] = -1;
        levelRoomsMatrix[levelGridSize / 2, levelGridSize / 2] = startRoom;
    }

    void Update()
    {
        if (!IsLevelDrown)
        {
            AddRoom();
            DrawRooms();
            IsLevelDrown = true;
        }

    }

    void DrawRooms()
    {
        for (int j = 0; j < levelGridSize; j++)
        {
            for (int i = 0; i < levelGridSize; i++)
            {
                if (levelRoomsMatrix[i, j] != null)
                {
                    Instantiate(levelRoomsMatrix[i, j], new Vector3((LevelOrigin.x / 2) + (i * CellSize) + (i * CellSpace), LevelOrigin.y, LevelOrigin.z / 2 + (j * CellSize) + (j * CellSpace)), Quaternion.identity);
                }
            }
        }

        string line = "\n";
        for (int j = 0; j < levelGridSize; j++)
        {
            for (int i = 0; i < levelGridSize; i++)
            {
                if (levelCellsMatrix[i, j] == -1 || levelCellsMatrix[i, j] == 1)
                    line += "X";
                else
                    line += "O";
            }
            line += "\n";
        }
        Debug.Log(line);
    }

    void AddRoom()
    {
        int currentRoomSize = 0;
        int currentRoomIndex = 0;
        GameObject currentRoom;
        int[,] currentRoomMatrix;

        int onGridPosX = 0;
        int onGridPosY = 0;

        if (SizeOfRoomDoesntMatter)
        {
            List<int> keysList = new List<int>(roomsBySize.Keys);
            int randomIndex = Random.Range(0, keysList.Count);
            currentRoomSize = keysList[randomIndex];
        }


        currentRoomIndex = Random.Range(0, roomsBySize[currentRoomSize].Count);
        currentRoom = roomsBySize[currentRoomSize][currentRoomIndex];

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

        //0 up, 1 right, 2 down, 3 left
        int roomInitPosition = Random.Range(0, 4);

        switch(roomInitPosition)
        {
            case 0:
                onGridPosX = Random.Range(0, levelGridSize - currentRoomSize);
                onGridPosY = 0;
                break;
            case 1:
                onGridPosX = levelGridSize - 1 - currentRoomSize;
                onGridPosY = Random.Range(0, levelGridSize - currentRoomSize);
                break;
            case 2:
                onGridPosX = Random.Range(0, levelGridSize - currentRoomSize);
                onGridPosY = levelGridSize - 1 - currentRoomSize;
                break;
            case 3:
                onGridPosX = 0;
                onGridPosY = Random.Range(0, levelGridSize - currentRoomSize);
                break;
        }

        for (int j = 0; j < currentRoomSize; j++)
            for (int i = 0; i < currentRoomSize; i++)
                levelCellsMatrix[onGridPosX + i, onGridPosY + j] = currentRoomMatrix[i, j];

        if (GenerateByCells)
        {

        }
    }
}
