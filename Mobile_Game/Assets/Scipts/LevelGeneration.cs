using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    [Range(2, 100)]
    public int roomsAmount = 3;
    public int bossRoomAmount = 1;
    public int bonusRoomAmount = 1;
    //[SerializeField] GameObject Player;
    [SerializeField] GameObject Cursor;
    [SerializeField] GameObject[] StartRoom; //0
    [SerializeField] GameObject[] RoomSquare; //1
    [SerializeField] GameObject[] RoomI; //2
    [SerializeField] GameObject[] RoomL; //3
    [SerializeField] GameObject[] RoomX; //4
    [SerializeField] GameObject[] RoomBigSqure; //5
    [SerializeField] GameObject[] BossRoom; //6
    [SerializeField] GameObject[] BonusRoom; //6

    [SerializeField] Vector3 mapOrigin = Vector3.zero;
    int mapSize;
    Room[,] mapGrid;
    int[,] busyCells;
    int currentNumberOfRooms = 0;
    float RoomSize = 3f;
    float offset = 1f;

    int currentBossRoomsAmount = 0;
    int currentBonusRoomsAmount = 0;

    void Start()
    {
        mapSize = roomsAmount * 9 - 2 + bossRoomAmount + bonusRoomAmount;      // Na wypadek pokojow w linii plus bonus
        mapOrigin = mapOrigin + new Vector3(-(mapSize / 2) * (RoomSize + offset), 0f, (mapSize / 2) * (RoomSize + offset));     // Wysrodkowany
        mapGrid = new Room[mapSize, mapSize];
        busyCells = new int[mapSize, 2];
        PrepareMap();
    }

    private void Update()
    {

        MakeMap();
        //AddBonusRooms();
        AddBossRooms();
        DrawMap();
        WriteMap();
        //Debug.Log(currentNumberOfRooms + " | " + roomsAmount);
    }

    void PrepareMap()
    {
        for (int j = 0; j < mapSize; j++)
        {
            for (int i = 0; i < mapSize; i++)
            {
                mapGrid[i, j] = new Room();
            }
        }
    }

    void MakeMap()
    {
        if (currentNumberOfRooms == 0)
        {
            mapGrid[mapSize / 2, mapSize / 2].isFree = false;
            mapGrid[mapSize / 2, mapSize / 2].type = "StartRoom";
            AddBusyCell(mapSize / 2, mapSize / 2);
        }

        if (currentNumberOfRooms < roomsAmount)
        {
            int existingCellNumber = Random.Range(0, currentNumberOfRooms);
            int position = Random.Range(0, 4);
            int rotation = Random.Range(0, 4);
            int type = Random.Range(1, 3);    //1 : number of rooms+1

            if (Random.Range(0, NumberOfNeighbours(busyCells[existingCellNumber, 0], busyCells[existingCellNumber, 1])) == 0)
                AddRoom(busyCells[existingCellNumber, 0], busyCells[existingCellNumber, 1], position, rotation, type);

            Cursor.transform.position = new Vector3(mapOrigin.x + (RoomSize + offset) * busyCells[existingCellNumber, 0], mapOrigin.y + 5, mapOrigin.z - (RoomSize + offset) * busyCells[existingCellNumber, 1]);
        }
    }

    void DrawMap()
    {
        for (int j = 0; j < mapSize; j++)
        {
            for (int i = 0; i < mapSize; i++)
            {
                if (!mapGrid[i, j].isFree && !mapGrid[i, j].created)
                {
                    mapGrid[i, j].created = true;
                    if (mapGrid[i, j].type == "StartRoom")
                    {
                        int roomIndex = Random.Range(0, StartRoom.Length);
                        Instantiate(StartRoom[roomIndex], new Vector3(mapOrigin.x + (RoomSize + offset) * i, mapOrigin.y, mapOrigin.z - (RoomSize + offset) * j), Quaternion.identity);
                    }
                    else if (mapGrid[i, j].type == "RoomSquare")
                    {
                        int roomIndex = Random.Range(0, RoomSquare.Length);
                        Instantiate(RoomSquare[roomIndex], new Vector3(mapOrigin.x + (RoomSize + offset) * i, mapOrigin.y, mapOrigin.z - (RoomSize + offset) * j), Quaternion.identity);
                    }
                    else if (mapGrid[i, j].type == "RoomI")
                    {
                        int roomIndex = Random.Range(0, RoomI.Length);
                        Instantiate(RoomI[roomIndex], new Vector3(mapOrigin.x + (RoomSize + offset) * i, mapOrigin.y, mapOrigin.z - (RoomSize + offset) * j), Quaternion.Euler(new Vector3(0f, mapGrid[i, j].rotation * 90, 0f)));
                    }
                    else if (mapGrid[i, j].type == "BossRoom")
                    {
                        int roomIndex = Random.Range(0, BossRoom.Length);
                        Instantiate(BossRoom[roomIndex], new Vector3(mapOrigin.x + (RoomSize + offset) * i, mapOrigin.y, mapOrigin.z - (RoomSize + offset) * j), Quaternion.identity);
                    }
                    else if (mapGrid[i, j].type == "Bonus")
                    {
                        int roomIndex = Random.Range(0, BonusRoom.Length);
                        Instantiate(BonusRoom[roomIndex], new Vector3(mapOrigin.x + (RoomSize + offset) * i, mapOrigin.y, mapOrigin.z - (RoomSize + offset) * j), Quaternion.identity);
                    }

                }
            }

        }
    }

    void WriteMap()
    {
        string templine = "";
        for (int i = 0; i < mapSize; i++)
        {
            templine += "_";
        }

        for (int j = 0; j < mapSize; j++)
        {
            string line = "";
            for (int i = 0; i < mapSize; i++)
            {
                if (mapGrid[i, j].type == "None")
                    line = line + "_";
                else if (mapGrid[i, j].type == "StartRoom")
                    line = line + "S";
                else if (mapGrid[i, j].type == "RoomSquare")
                    line = line + "Q";
                else if (mapGrid[i, j].type == "Part")
                    line = line + "P";
                else if (mapGrid[i, j].type == "RoomI")
                    line = line + "I";
                else if (mapGrid[i, j].type == "BossRoom")
                    line = line + "B";
            }

            if (line != templine)
                Debug.Log(line);
        }


    }

    void AddBusyCell(int x, int y)
    {
        busyCells[currentNumberOfRooms, 0] = x;
        busyCells[currentNumberOfRooms, 1] = y;
        currentNumberOfRooms++;
    }

    void AddRoom(int xCoord, int yCoord, int positionNumber, int rotationNumber, int typeOfRoom)
    {
        switch (typeOfRoom)
        {
            case 1: //RoomSquare
                switch (positionNumber)  //from up - clockwise
                {
                    case 0:
                        if (mapGrid[xCoord, yCoord + 1].isFree)
                        {
                            mapGrid[xCoord, yCoord + 1].isFree = false;
                            mapGrid[xCoord, yCoord + 1].type = "RoomSquare";
                            mapGrid[xCoord, yCoord + 1].rotation = rotationNumber;
                            AddBusyCell(xCoord, yCoord + 1);
                        }
                        break;
                    case 1:
                        if (mapGrid[xCoord + 1, yCoord].isFree)
                        {
                            mapGrid[xCoord + 1, yCoord].isFree = false;
                            mapGrid[xCoord + 1, yCoord].type = "RoomSquare";
                            mapGrid[xCoord + 1, yCoord].rotation = rotationNumber;
                            AddBusyCell(xCoord + 1, yCoord);
                        }
                        break;
                    case 2:
                        if (mapGrid[xCoord, yCoord - 1].isFree)
                        {
                            mapGrid[xCoord, yCoord - 1].isFree = false;
                            mapGrid[xCoord, yCoord - 1].type = "RoomSquare";
                            mapGrid[xCoord, yCoord - 1].rotation = rotationNumber;
                            AddBusyCell(xCoord, yCoord - 1);
                        }
                        break;
                    case 3:
                        if (mapGrid[xCoord - 1, yCoord].isFree)
                        {
                            mapGrid[xCoord - 1, yCoord].isFree = false;
                            mapGrid[xCoord - 1, yCoord].type = "RoomSquare";
                            mapGrid[xCoord - 1, yCoord].rotation = rotationNumber;
                            AddBusyCell(xCoord - 1, yCoord);
                        }
                        break;
                    default:
                        break;
                }
                break;
            case 2: //RoomI
                switch (positionNumber)
                {
                    case 0:
                        switch (rotationNumber)
                        {
                            case 0:
                                if (mapGrid[xCoord, yCoord - 1].isFree && mapGrid[xCoord, yCoord - 2].isFree) // ?
                                {
                                    mapGrid[xCoord, yCoord - 2].isFree = false;
                                    mapGrid[xCoord, yCoord - 2].type = "Part";

                                    mapGrid[xCoord, yCoord - 1].type = "RoomI";         //      P
                                    mapGrid[xCoord, yCoord - 1].rotation = 0;           //      I
                                    mapGrid[xCoord, yCoord - 1].isFree = false;         //      O

                                    AddBusyCell(xCoord, yCoord - 1);
                                    AddBusyCell(xCoord, yCoord - 2);
                                }
                                break;
                            case 1:
                                if (mapGrid[xCoord, yCoord - 1].isFree && mapGrid[xCoord + 1, yCoord - 1].isFree)
                                {
                                    mapGrid[xCoord + 1, yCoord - 1].isFree = false;
                                    mapGrid[xCoord + 1, yCoord - 1].type = "Part";

                                    mapGrid[xCoord, yCoord - 1].isFree = false;         //      
                                    mapGrid[xCoord, yCoord - 1].type = "RoomI";         //      I P
                                    mapGrid[xCoord, yCoord - 1].rotation = 1;           //      O

                                    AddBusyCell(xCoord, yCoord - 1);
                                    AddBusyCell(xCoord + 1, yCoord - 1);
                                }
                                break;
                            // 2: doesnt exists
                            case 3:
                                if (mapGrid[xCoord, yCoord - 1].isFree && mapGrid[xCoord - 1, yCoord - 1].isFree)
                                {
                                    mapGrid[xCoord, yCoord - 1].isFree = false;
                                    mapGrid[xCoord, yCoord - 1].type = "Part";

                                    mapGrid[xCoord - 1, yCoord - 1].isFree = false;     //
                                    mapGrid[xCoord - 1, yCoord - 1].type = "RoomI";     //    I P
                                    mapGrid[xCoord - 1, yCoord - 1].rotation = 1;       //      O

                                    AddBusyCell(xCoord, yCoord - 1);
                                    AddBusyCell(xCoord - 1, yCoord - 1);
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case 1:
                        switch (rotationNumber)
                        {
                            case 0:
                                if (mapGrid[xCoord + 1, yCoord].isFree && mapGrid[xCoord + 1, yCoord - 1].isFree)
                                {
                                    mapGrid[xCoord + 1, yCoord - 1].isFree = false;
                                    mapGrid[xCoord + 1, yCoord - 1].type = "Part";

                                    mapGrid[xCoord + 1, yCoord].isFree = false;     //        P
                                    mapGrid[xCoord + 1, yCoord].type = "RoomI";     //      O I
                                    mapGrid[xCoord + 1, yCoord].rotation = 0;       //      

                                    AddBusyCell(xCoord + 1, yCoord);
                                    AddBusyCell(xCoord + 1, yCoord - 1);
                                }
                                break;
                            case 1:
                                if (mapGrid[xCoord + 1, yCoord].isFree && mapGrid[xCoord + 2, yCoord].isFree)
                                {
                                    mapGrid[xCoord + 2, yCoord].isFree = false;
                                    mapGrid[xCoord + 2, yCoord].type = "Part";

                                    mapGrid[xCoord + 1, yCoord].isFree = false;     //        
                                    mapGrid[xCoord + 1, yCoord].type = "RoomI";     //      O I P
                                    mapGrid[xCoord + 1, yCoord].rotation = 1;       //      

                                    AddBusyCell(xCoord + 1, yCoord);
                                    AddBusyCell(xCoord + 2, yCoord);
                                }
                                break;
                            case 2:
                                if (mapGrid[xCoord + 1, yCoord].isFree && mapGrid[xCoord + 1, yCoord + 1].isFree)
                                {
                                    mapGrid[xCoord + 1, yCoord].isFree = false;
                                    mapGrid[xCoord + 1, yCoord].type = "Part";

                                    mapGrid[xCoord + 1, yCoord + 1].isFree = false;     //        
                                    mapGrid[xCoord + 1, yCoord + 1].type = "RoomI";     //      O P
                                    mapGrid[xCoord + 1, yCoord + 1].rotation = 0;       //        I

                                    AddBusyCell(xCoord + 1, yCoord);
                                    AddBusyCell(xCoord + 1, yCoord + 1);
                                }
                                break;
                            // 3: doesnt exists
                            default:
                                break;
                        }
                        break;
                    case 2:
                        switch (rotationNumber)
                        {
                            // 0: doesnt exists
                            case 1:
                                if (mapGrid[xCoord, yCoord + 1].isFree && mapGrid[xCoord + 1, yCoord + 1].isFree)
                                {
                                    mapGrid[xCoord + 1, yCoord + 1].isFree = false;
                                    mapGrid[xCoord + 1, yCoord + 1].type = "Part";

                                    mapGrid[xCoord, yCoord + 1].isFree = false;     //      O
                                    mapGrid[xCoord, yCoord + 1].type = "RoomI";     //      I P
                                    mapGrid[xCoord, yCoord + 1].rotation = 1;       //

                                    AddBusyCell(xCoord, yCoord + 1);
                                    AddBusyCell(xCoord + 1, yCoord + 1);
                                }
                                break;
                            case 2:
                                if (mapGrid[xCoord, yCoord + 1].isFree && mapGrid[xCoord, yCoord + 2].isFree)
                                {
                                    mapGrid[xCoord, yCoord + 1].isFree = false;
                                    mapGrid[xCoord, yCoord + 1].type = "Part";

                                    mapGrid[xCoord, yCoord + 2].isFree = false;     //      O 
                                    mapGrid[xCoord, yCoord + 2].type = "RoomI";     //      P
                                    mapGrid[xCoord, yCoord + 2].rotation = 0;       //      I

                                    AddBusyCell(xCoord, yCoord + 1);
                                    AddBusyCell(xCoord, yCoord + 2);
                                }
                                break;
                            case 3:
                                if (mapGrid[xCoord, yCoord + 1].isFree && mapGrid[xCoord - 1, yCoord + 1].isFree)
                                {
                                    mapGrid[xCoord, yCoord + 1].isFree = false;
                                    mapGrid[xCoord, yCoord + 1].type = "Part";

                                    mapGrid[xCoord - 1, yCoord + 1].isFree = false;     //      O
                                    mapGrid[xCoord - 1, yCoord + 1].type = "RoomI";     //    I P
                                    mapGrid[xCoord - 1, yCoord + 1].rotation = 1;       //
                                                                                        //  
                                    AddBusyCell(xCoord, yCoord + 1);
                                    AddBusyCell(xCoord - 1, yCoord + 1);
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case 3:
                        switch (rotationNumber)
                        {
                            case 0:
                                if (mapGrid[xCoord - 1, yCoord].isFree && mapGrid[xCoord - 1, yCoord - 1].isFree)
                                {
                                    mapGrid[xCoord - 1, yCoord - 1].isFree = false;
                                    mapGrid[xCoord - 1, yCoord - 1].type = "Part";

                                    mapGrid[xCoord - 1, yCoord].isFree = false;     //      P 
                                    mapGrid[xCoord - 1, yCoord].type = "RoomI";     //      I O
                                    mapGrid[xCoord - 1, yCoord].rotation = 0;       //

                                    AddBusyCell(xCoord - 1, yCoord);
                                    AddBusyCell(xCoord - 1, yCoord - 1);
                                }
                                break;
                            // 1: doesnt exists
                            case 2:
                                if (mapGrid[xCoord - 1, yCoord].isFree && mapGrid[xCoord - 1, yCoord + 1].isFree)
                                {
                                    mapGrid[xCoord - 1, yCoord].isFree = false;
                                    mapGrid[xCoord - 1, yCoord].type = "Part";

                                    mapGrid[xCoord - 1, yCoord + 1].isFree = false;     //       
                                    mapGrid[xCoord - 1, yCoord + 1].type = "RoomI";     //      P O
                                    mapGrid[xCoord - 1, yCoord + 1].rotation = 0;       //      I

                                    AddBusyCell(xCoord - 1, yCoord);
                                    AddBusyCell(xCoord - 1, yCoord + 1);
                                }
                                break;
                            case 3:
                                if (mapGrid[xCoord - 1, yCoord].isFree && mapGrid[xCoord - 2, yCoord].isFree)
                                {
                                    mapGrid[xCoord - 1, yCoord].isFree = false;
                                    mapGrid[xCoord - 1, yCoord].type = "Part";

                                    mapGrid[xCoord - 2, yCoord].isFree = false;     //       
                                    mapGrid[xCoord - 2, yCoord].type = "RoomI";     //    I P O
                                    mapGrid[xCoord - 2, yCoord].rotation = 1;       //      

                                    AddBusyCell(xCoord - 1, yCoord);
                                    AddBusyCell(xCoord - 2, yCoord);
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case 3: //Room L
                switch (positionNumber)
                {
                    case 0:

                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    default:
                        break;
                }
                break;
            case 4:
                switch (positionNumber)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    default:
                        break;
                }
                break;
            case 5:
                switch (positionNumber)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    default:
                        break;
                }
                break;
            //specials
            case 6:
                mapGrid[xCoord, yCoord].isFree = false;
                mapGrid[xCoord, yCoord].type = "BossRoom";
                currentBossRoomsAmount++;
                AddBusyCell(xCoord, yCoord);
                break;
            case 7:
                mapGrid[xCoord, yCoord].isFree = false;
                mapGrid[xCoord, yCoord].type = "BonusRoom";
                currentBonusRoomsAmount++;
                AddBusyCell(xCoord, yCoord);
                break;
            default:
                break;
        }

    }

    void AddBossRooms()
    {
        
        while (currentNumberOfRooms >= roomsAmount && currentBossRoomsAmount < bossRoomAmount)
        {
            SortBusyListByDistance();
            int i = 0;

            int maxX = busyCells[i, 0];
            int maxY = busyCells[i, 1];
                if (mapGrid[maxX, maxY + 1].isFree && NumberOfNeighbours(maxX, maxY + 1) == 1)
                    AddRoom(maxX, maxY + 1, 0, 0, 6);
                else if (mapGrid[maxX + 1, maxY].isFree && NumberOfNeighbours(maxX + 1, maxY) == 1)
                    AddRoom(maxX + 1, maxY, 1, 0, 6);
                else if (mapGrid[maxX, maxY - 1].isFree && NumberOfNeighbours(maxX, maxY - 1) == 1)
                    AddRoom(maxX, maxY - 1, 2, 0, 6);
                else if (mapGrid[maxX - 1, maxY].isFree && NumberOfNeighbours(maxX - 1, maxY) == 1)
                    AddRoom(maxX - 1, maxY, 3, 0, 6);
            i++;
        }
    }

    void AddBonusRooms()
    {
        if (currentNumberOfRooms >= roomsAmount && currentBonusRoomsAmount < bonusRoomAmount)
        {
            int existingCellNumber = Random.Range(0, currentNumberOfRooms);
            int position = Random.Range(0, 4);

            AddRoom(busyCells[existingCellNumber, 0], busyCells[existingCellNumber, 1], position, 0, 7);
        }
    }

    int NumberOfNeighbours(int xCoord, int yCoord)
    {
        int number = 0;

        if (!mapGrid[xCoord, yCoord + 1].isFree)
            number++;
        if (!mapGrid[xCoord + 1, yCoord + 1].isFree)
            number++;
        if (!mapGrid[xCoord + 1, yCoord].isFree)
            number++;
        if (!mapGrid[xCoord + 1, yCoord - 1].isFree)
            number++;
        if (!mapGrid[xCoord, yCoord - 1].isFree)
            number++;
        if (!mapGrid[xCoord - 1, yCoord - 1].isFree)
            number++;
        if (!mapGrid[xCoord - 1, yCoord].isFree)
            number++;

        return number;
    }

    float Distance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }

    void SortBusyListByDistance()
    {
        int center = mapSize / 2;
        int tempX, tempY, tempIndex;

        for (int i = 0; i < roomsAmount; i++)
        {
            tempX = busyCells[i, 0];
            tempY = busyCells[i, 1];
            tempIndex = i;
            int maxX = center;
            int maxY = center;
            for (int j = i; j < roomsAmount; j++)
            {
                if (Distance(maxX, maxY, center, center) < Distance(busyCells[j, 0], busyCells[j, 1], center, center))
                {
                    maxX = busyCells[j, 0];
                    maxY = busyCells[j, 1];
                    tempIndex = j;
                }
                Debug.Log(Distance(maxX, maxY, center, center));
            }

            busyCells[tempIndex, 0] = tempX;
            busyCells[tempIndex, 1] = tempY;
            busyCells[i, 0] = maxX;
            busyCells[i, 1] = maxY;
        }
    }
}
