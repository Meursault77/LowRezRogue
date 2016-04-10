using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Xml;
using System.Diagnostics;
using System.Globalization;

namespace LowRezRogue {

    public static class MapGeneration {

        static int pixels = 8;
        static Random random = new Random();

        public class Room {
            public List<Point> tiles;
            public List<Point> edgeTiles;
            public HashSet<Room> connectedRooms;
            public int roomSize;
            public bool isAccessibleFromMainRoom;

            public Room() {

            }

            public Room(List<Point> roomTiles) {
                tiles = roomTiles;
                roomSize = tiles.Count;
                connectedRooms = new HashSet<Room>();

                edgeTiles = new List<Point>();
                foreach(Point tile in tiles)
                {
                    for(int x = tile.X - 1; x <= tile.X + 1; x++)
                    {
                        for(int y = tile.Y - 1; y <= tile.Y + 1; y++)
                        {
                            if((x == tile.X || y == tile.Y) && !map[x, y].walkable)
                                edgeTiles.Add(tile);
                        }
                    }
                }
                if(mainRoom == null || mainRoom.roomSize < this.roomSize)
                {
                    if(mainRoom != null)
                    {
                        mainRoom.isAccessibleFromMainRoom = false;
                        foreach(Room room in mainRoom.connectedRooms)
                        {
                            if(room.isAccessibleFromMainRoom)
                            {
                                mainRoom.isAccessibleFromMainRoom = true;
                                break;
                            }
                        }
                    }
                    mainRoom = this;
                    isAccessibleFromMainRoom = true;
                }
            }

            public void SetAccessibleFromMainRoom() {
                if(!isAccessibleFromMainRoom)
                {
                    isAccessibleFromMainRoom = true;
                    foreach(Room connected in connectedRooms)
                    {
                        connected.SetAccessibleFromMainRoom();
                    }
                }
            }

        }

        public struct Tile {
            public Rectangle[] spriteRect;
            public bool animated;
            public bool walkable;
            public TileType tileType;
            public int spriteIndex;

            public Item itemOnTop;

            public Tile(bool walkable, TileType tileType) {
                this.spriteRect = tileType.spriteRect;
                this.walkable = walkable;
                this.tileType = tileType;
                this.spriteIndex = 0;
                this.itemOnTop = null;
                this.animated = spriteRect.Length > 1;
            }

            public void UpdateAnimationIndex() {
                spriteIndex += 1;
                if(spriteIndex == spriteRect.Length)
                    spriteIndex = 0;
            }

        }

        public enum InteractionType { none, shop, entry, castle }

        public enum TileTypes { normal, blocking, interaction, deadly }

        public class TileType {
            public string name;
            public TileTypes tileType;
            public Rectangle[] spriteRect;
            public float odds;
            public int xExtraTiles;
            public int yExtraTiles;
            public InteractionType interaction;

            public TileType(string name) {
                this.name = name;
            }
        }

        #region Tile Sets

        public enum TileSets { overworld, dungeon, industrial }

        public class TileSet {
            public TileSets set;
            public TileType[] normals;
            public TileType[] blocking;
            public TileType[] interaction;
            public TileType[] deadly;
        }

        public static TileSet[] tileSets;

        public static TileSet GetTileSet(TileSets setEnum) {
            foreach(TileSet set in tileSets)
            {
                if(set.set == setEnum)
                    return set;
            }
            return null;
        }

        public static void InitTileSets() {
            XmlDocument xmlTileSets = new XmlDocument();
            xmlTileSets.Load("Content/XML/TileSets.xml");
            if(xmlTileSets == null)
                Debug.WriteLine("Fuck couldnt load TileSets.xml");

            XmlNodeList nodes = xmlTileSets.SelectNodes("tileSets/tileSet");
            tileSets = new TileSet[nodes.Count];
            int i = 0;

            foreach(XmlNode node in nodes)
            {
                var tileSet = new TileSet();

                tileSet.set = (TileSets)Enum.Parse(typeof(TileSets), node.Attributes.GetNamedItem("name").Value, true);

                XmlNodeList normalNodes = node.SelectNodes("normal/tile");
                tileSet.normals = new TileType[normalNodes.Count];
                for(int j = 0; j < normalNodes.Count; j++)
                {
                    tileSet.normals[j] = LoadTileType(normalNodes[j], TileTypes.normal);
                }

                XmlNodeList blockingNodes = node.SelectNodes("blocking/tile");
                tileSet.blocking = new TileType[blockingNodes.Count];
                for(int j = 0; j < blockingNodes.Count; j++)
                {
                    tileSet.blocking[j] = LoadTileType(blockingNodes[j], TileTypes.blocking);
                }

                XmlNodeList interactionNodes = node.SelectNodes("interaction/tile");
                tileSet.interaction = new TileType[interactionNodes.Count];
                for(int j = 0; j < interactionNodes.Count; j++)
                {
                    tileSet.interaction[j] = LoadTileType(interactionNodes[j], TileTypes.interaction);
                }

                XmlNodeList deadlyNodes = node.SelectNodes("deadly/tile");
                tileSet.deadly = new TileType[deadlyNodes.Count];
                for(int j = 0; j < deadlyNodes.Count; j++)
                {
                    tileSet.deadly[j] = LoadTileType(deadlyNodes[j], TileTypes.deadly);
                }

                tileSets[i] = tileSet;
                i++;
            }
        }

        static TileType LoadTileType(XmlNode n, TileTypes type) {
            var tileType = new TileType(n.Attributes.GetNamedItem("name").Value);
            tileType.odds = float.Parse(n.Attributes.GetNamedItem("odds").Value, CultureInfo.InvariantCulture);
            tileType.tileType = type;

            //Animation Frames if existend.
            XmlNodeList frames = n.SelectNodes("animation");
            if(frames.Count == 0)
            {
                tileType.spriteRect = new Rectangle[1];
                tileType.spriteRect[0] = new Rectangle(int.Parse(n.Attributes.GetNamedItem("x").Value) * pixels, int.Parse(n.Attributes.GetNamedItem("y").Value) * pixels, pixels, pixels);
            } else if(frames.Count > 0)
            {
                tileType.spriteRect = new Rectangle[frames.Count + 1];
                tileType.spriteRect[0] = new Rectangle(int.Parse(n.Attributes.GetNamedItem("x").Value) * pixels, int.Parse(n.Attributes.GetNamedItem("y").Value) * pixels, pixels, pixels);
                int k = 1;
                foreach(XmlNode frame in frames)
                {
                    tileType.spriteRect[k] = new Rectangle(int.Parse(frame.Attributes.GetNamedItem("x").Value) * pixels, int.Parse(frame.Attributes.GetNamedItem("y").Value) * pixels, pixels, pixels);
                    k++;
                }

            }

            tileType.xExtraTiles = 0;
            if(n.Attributes["xExtraTile"] != null)
                tileType.xExtraTiles = int.Parse(n.Attributes["xExtraTile"].Value);

            tileType.yExtraTiles = 0;
            if(n.Attributes["yExtraTile"] != null)
                tileType.yExtraTiles = int.Parse(n.Attributes["yExtraTile"].Value);

            if(n.Attributes["type"] != null)
                tileType.interaction = (InteractionType)Enum.Parse(typeof(InteractionType), n.Attributes["type"].Value, true);
            else
                tileType.interaction = InteractionType.none;


            return tileType;
        }
        #endregion

        public static Tile[,] CreateOverworld(int width, int height) {
            var map = new Tile[width, height];
            TileSet tileSet = GetTileSet(TileSets.overworld);
            if(tileSet == null)
            {
                Debug.WriteLine("CreateOverworld couldnt load tileSet...");
                return null;
            }


             // create Perlin noise function
             PerlinNoise noise = new PerlinNoise(4, 0.5, 1.0, 1.0);
             // generate clouds effect
             float[,] texture = new float[height, width];
             
             for ( int x = 0; x < width; x++ )
             {
             	for ( int y = 0; y < height; y++ )
             	{
                    if(map[x,y].spriteRect != null)
                    {
                        continue;
                    }

                    float val = Math.Max(0.0f, Math.Min(1.0f, (float)noise.Function2D(x, y)) * 0.5f + 0.5f);
                    if(val <= 0.7)
                    {
                        TileType tileType = GetRandomTileType(tileSet.normals);
                        map[x, y] = new Tile(true, tileType);
                    } else if(val <= 0.9)
                    {
                        var tileType = GetRandomTileType(tileSet.blocking);
                        map[x, y] = new Tile(false, tileType);
                    } else if(val <= 0.98)
                    {
                        var tileType = GetRandomTileType(tileSet.deadly);
                        map[x, y] = new Tile(true, tileType);
                    } else if(val <= 1.0)
                    {
                        var tileType = GetRandomTileType(tileSet.interaction);
                        map[x, y] = new Tile(true, tileType);
                    } else
                    {
                        TileType tileType = GetRandomTileType(tileSet.normals);
                        map[x, y] = new Tile(true, tileType);
                    }

                    for(int i = 1; i <= map[x,y].tileType.xExtraTiles; i++)
                    {
                        if(x + 1 >= width)
                            continue;

                        var length = map[x, y].tileType.spriteRect.Length;
                        Rectangle[] spriteRect = new Rectangle[length];

                        for(int k = 0; k < length; k++)
                        {
                            spriteRect[k] = map[x, y].tileType.spriteRect[k];
                            spriteRect[k].X += pixels;
                        }
                        map[x + i, y] = new Tile(map[x, y].walkable, map[x, y].tileType);
                    }
                    for(int i = 1; i <= map[x, y].tileType.yExtraTiles; i++)
                    {
                        if(y + 1 >= height)
                            continue;

                        var length = map[x, y].tileType.spriteRect.Length;
                        Rectangle[] spriteRect = new Rectangle[length];

                        for(int k = 0; k < length; k++)
                        {
                            spriteRect[k] = map[x, y].tileType.spriteRect[k];
                            spriteRect[k].Y += pixels;
                        }
                        map[x, y + i] = new Tile(map[x, y].walkable, map[x, y].tileType);
                    }

                }
            }

            return map;
        }

        static TileType GetRandomTileType(TileType[] typeArray) {

            if(typeArray.Length == 1)
                return typeArray[0];
            else if(typeArray.Length == 0)
                return null;

            float val = random.Next(0, 101);
            val /= 100;
            float total = 0f;

            foreach(TileType type in typeArray)
            {
                total += type.odds;
                if(val <= total)
                    return type;
            }

            //Debug.WriteLine("GetRandomTileType: SOmethig went wrong. Couldnt use odds. Returning something fully random.");
            return typeArray[random.Next(0, typeArray.Length)];
        }


        static int smoothIterations = 5;
        static int mapWidth;
        static int mapHeight;
        static Tile[,] map;
        static List<Room> rooms;
        static Room mainRoom;
        static TileSet set;
        static Map workingOn;
        static int tilesPerEnemy = 50;


        public static Map CreateDungeon(LowRezRogue game, int width, int height, int blockingTilePercentage, Dictionary<string, Item> items, bool openCave = false, bool isOverworld = false) {
            MapGeneration.mapWidth = width;
            MapGeneration.mapHeight = height;
            set = GetTileSet(TileSets.overworld);

            Map newMap = new Map(width, height);
            workingOn = newMap;
            int openess = openCave ? 4 : 3;
            mainRoom = null;

            map = new Tile[width, height];
            rooms = new List<Room>();


            Random random = new Random();// 654320);
            
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    if(x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        map[x, y] = new Tile(false, GetRandomTileType(set.blocking));
                    else
                        map[x, y] = (random.Next(0, 100) < blockingTilePercentage) ? new Tile(false, GetRandomTileType(set.blocking)) : new Tile(true, GetRandomTileType(set.normals));
                    
                }
            }

            //Smoothing
            for(int i = 0; i < smoothIterations; i++)
            {
                for(int x = 0; x < width; x++)
                {
                    for(int y = 0; y < height; y++)
                    {
                        int wallCount = 0;
                        for(int neighX = x - 1; neighX <= x + 1; neighX++)
                        {
                            for(int neighY = y - 1; neighY <= y + 1; neighY++)
                            {
                                if(IsInMapRange(neighX, neighY))
                                {
                                    if(neighX != x || neighY != y)
                                    {
                                        if(!map[neighX, neighY].walkable)
                                            wallCount += 1;
                                    }
                                } else
                                    wallCount++;
                            }
                        }

                        if(wallCount > 4)
                            map[x, y] = new Tile(false, GetRandomTileType(set.blocking));
                        else if(wallCount < openess)
                            map[x, y] = new Tile(true, GetRandomTileType(set.normals));

                    }
                }
            }

            //flood fill, checking for walkable rooms
            bool[,] isTileVisited = new bool[width, height];
            
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    if(isTileVisited[x,y] == false && map[x,y].walkable == true)
                    {
                        rooms.Add(new Room(CreateFloodFillRegion(x, y, isTileVisited)));
                    }
                }
            }


            for(int i = 0; i < rooms.Count; i++)
            {
               // Debug.WriteLine($"Room size: {rooms[i].tiles.Count}, #edge tiles: {rooms[i].edgeTiles.Count}, Connected Rooms {rooms[i].connectedRooms.Count}");
                if(rooms[i].roomSize < 5)
                    rooms.RemoveAt(i);              
            }
            for(int i = 0; i < rooms.Count; i++)
            {
                if(mainRoom == null || rooms[i].tiles.Count > mainRoom.tiles.Count)
                    mainRoom = rooms[i];
            }
            Debug.WriteLine($"Flood filled rooms count: {rooms.Count}, mapSize: {width}, {height}");

            //connect rooms
            if(rooms.Count > 1)
            {
                ConnectClosestRooms();

            }

            Room smallest = null;
            for(int i = 0; i < rooms.Count; i++)
            {
                if(smallest == null || rooms[i].tiles.Count < smallest.tiles.Count)
                    smallest = rooms[i];
            }

            //place something special in the smallest room
            if(smallest != null)
            {
                Point p = smallest.tiles[random.Next(0, smallest.tiles.Count)];
                if(smallest.tiles.Count > 10) {
                    while(smallest.edgeTiles.Contains(p))
                    {
                        p = smallest.tiles[random.Next(0, smallest.tiles.Count)];
                    }
                }
                var type = set.interaction[1];
                map[p.X, p.Y] = new Tile(false, type);
            }


            //place entries
            int numOfEntries = 1;
            if(isOverworld)
            {
                numOfEntries = 5;
            }

            for(int l = 0; l < numOfEntries; l++)
            {
                Point entry = mainRoom.tiles[random.Next(0, mainRoom.tiles.Count)];

                while(mainRoom.edgeTiles.Contains(entry) || newMap.entries.ContainsKey(entry) || !DistanceToEntriesBigEnough(entry))
                {
                    entry = mainRoom.tiles[random.Next(0, mainRoom.tiles.Count)];
                }


                if(!isOverworld)
                {
                    newMap.entries.Add(entry, game.allMaps[0]);
                    if(map[entry.X + 1, entry.Y].walkable)
                        newMap.playerPositionOnLeave = new Point(entry.X + 1, entry.Y);
                    else if(map[entry.X - 1, entry.Y].walkable)
                        newMap.playerPositionOnLeave = new Point(entry.X + 1, entry.Y);
                    else if(map[entry.X, entry.Y + 1].walkable)
                        newMap.playerPositionOnLeave = new Point(entry.X + 1, entry.Y);
                    else if(map[entry.X, entry.Y - 1].walkable)
                        newMap.playerPositionOnLeave = new Point(entry.X + 1, entry.Y);
                    else
                        newMap.playerPositionOnLeave = entry;
                } else
                {
                    newMap.entries.Add(entry, game.allMaps[l + 1]);
                }
                map[entry.X, entry.Y] = new Tile(true, set.interaction[0]);

            }

            //place enemies
            newMap.enemies = new List<Enemy>();
            for(int r = 0; r < rooms.Count; r++)
            {
                int numOfEnemies = rooms[r].tiles.Count / tilesPerEnemy;

                for(int c = 0; c < numOfEnemies; c++)
                {
                    Point p = rooms[r].tiles[random.Next(0, rooms[r].tiles.Count)];
                    if(rooms[r].tiles.Count > 7) {
                        while(rooms[r].edgeTiles.Contains(p))
                        {
                            p = rooms[r].tiles[random.Next(0, rooms[r].tiles.Count)];
                        }
                    }
                    newMap.enemies.Add(new Enemy(p, game.enemyAnimations));
                }
            }
            Debug.WriteLine("Enemies created: " + newMap.enemies.Count);

            //place castle
            if(isOverworld)
            {
                Room room = rooms[random.Next(0,rooms.Count)];
                while(room == smallest && rooms.Count > 1)
                {
                    room = rooms[random.Next(0, rooms.Count)];
                }

                Point p = room.tiles[random.Next(0, room.tiles.Count)];
                while(room.edgeTiles.Contains(p) || newMap.entries.ContainsKey(p) || !DistanceToEntriesBigEnough(p))
                {
                    p = room.tiles[random.Next(0, room.tiles.Count)];
                }

                map[p.X, p.Y] = new Tile(true, set.interaction[2]);


                for(int i = 1; i <= map[p.X, p.Y].tileType.xExtraTiles; i++)
                {
                    if(p.X + 1 >= width)
                        continue;

                    map[p.X + i, p.Y] = new Tile(map[p.X, p.Y].walkable, set.interaction[2]);

                    var length = map[p.X, p.Y].tileType.spriteRect.Length;
                    Rectangle[] spriteRect = new Rectangle[length];

                    for(int k = 0; k < length; k++)
                    {
                        spriteRect[k] = map[p.X, p.Y].tileType.spriteRect[k];
                        spriteRect[k].X += pixels;
                    }
                    map[p.X + i, p.Y].spriteRect = spriteRect;
                }
            }


            Room playerRoom = rooms[random.Next(0, rooms.Count)];
            while(playerRoom == smallest && rooms.Count > 1)
            {
                playerRoom = rooms[random.Next(0, rooms.Count)];
            }

            Point pos = playerRoom.tiles[random.Next(0, playerRoom.tiles.Count)];
            while(playerRoom.edgeTiles.Contains(pos))
            {
                pos = playerRoom.tiles[random.Next(0, playerRoom.tiles.Count)];
            }

            if(isOverworld)
            {
                newMap.playerPositionOnLeave = pos;
                //for testing
                map[pos.X + 1, pos.Y].itemOnTop = items["oldSword"];
                map[pos.X + 2, pos.Y].itemOnTop = items["fancySword"];
                map[pos.X, pos.Y-1].itemOnTop = items["okayArmor"];
                map[pos.X, pos.Y+1].itemOnTop = items["okayPotion"];
                map[pos.X-1, pos.Y].itemOnTop = items["fancyRange"];

            }
            newMap.rooms = rooms;
            newMap.map = map;

            return newMap;
        }


        static bool AreTilesNeighbours(Point p1, Point p2) {
            return AreTilesNeighbours(p1.X, p1.Y, p2.X, p2.Y);
        }

        static bool AreTilesNeighbours(int x1, int y1, int x2, int y2) {
            if(!IsInMapRange(x1, y1) || !IsInMapRange(x2, y2))
                return false;

            if((x1 == x2 + 1 || x1 == x2 - 1) && y1 == y2)
                return true;
            if((y1 == y2 + 1 || y1 == y2 - 1) && x1 == x2)
                return true;

            return false;
        }

        static bool DistanceToEntriesBigEnough(Point p) {

            foreach(Point entry in workingOn.entries.Keys)
            {
                if(AreTilesNeighbours(entry, p))
                    return false;
            }
            
            return true;
        }

        static void ConnectClosestRooms(bool forceMainRoomConnection = false) {

            List<Room> roomListA = new List<Room>();
            List<Room> roomListB = new List<Room>();

            if(forceMainRoomConnection)
            {
                foreach(Room room in rooms)
                {
                    if(room.isAccessibleFromMainRoom)
                        roomListA.Add(room);
                    else
                        roomListB.Add(room);
                }
            } else
            {
                roomListA = rooms;
                roomListB = rooms;
            }

            int bestDistance = 0; //int.MaxValue;
            Point bestTileA = new Point();
            Point bestTileB = new Point();
            Room bestRoomA = new Room();
            Room bestRoomB = new Room();
            bool possConnectionFound = false;

            foreach(Room roomA in roomListA)
            {
                if(!forceMainRoomConnection)
                {
                    possConnectionFound = false;
                    if(roomA.connectedRooms.Count > 0)
                        continue;
                }
                foreach(Room roomB in roomListB)
                {
                    if(roomA == roomB || roomA.connectedRooms.Contains(roomB))
                        continue;

                    for(int a = 0; a < roomA.edgeTiles.Count; a++)
                    {
                        for(int b = 0; b < roomB.edgeTiles.Count; b++)
                        {
                            Point tileA = roomA.edgeTiles[a];
                            Point tileB = roomB.edgeTiles[b];
                            int dist = ((tileA.X - tileB.X) * (tileA.X - tileB.X)) + ((tileA.Y - tileB.Y) * (tileA.Y - tileB.Y));

                            
                            if(dist < random.Next(8, 15))
                                ConnectRooms(roomA, roomB, tileA, tileB);

                            if(dist < bestDistance || !possConnectionFound)
                            {
                                bestDistance = dist;
                                possConnectionFound = true;
                                bestTileA = tileA;
                                bestTileB = tileB;
                                bestRoomA = roomA;
                                bestRoomB = roomB;
                            }
                        }
                    }
                }
                if(possConnectionFound && !forceMainRoomConnection)
                {
                    ConnectRooms(bestRoomA, bestRoomB, bestTileA, bestTileB);
                }
            }

            if(possConnectionFound && forceMainRoomConnection)
            {
                ConnectRooms(bestRoomA, bestRoomB, bestTileA, bestTileB);
                ConnectClosestRooms(true);      //because it is only finding one connection, have to be called again.
            }

            if(!forceMainRoomConnection)
                ConnectClosestRooms(true);

        }

        public static HashSet<Point[]> Lines;

        static void ConnectRooms(Room roomA, Room roomB, Point tileA, Point tileB) {
            if(roomA.isAccessibleFromMainRoom)
                roomB.SetAccessibleFromMainRoom();
            else if(roomB.isAccessibleFromMainRoom)
                roomA.SetAccessibleFromMainRoom();

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);

            HashSet<Point> line = GetLine(tileA, tileB);
            if(Lines == null)
                Lines = new HashSet<Point[]>();

            var temp = new Point[2];
            temp[0] = tileA;
            temp[1] = tileB;
            Lines.Add(temp);
            
            int r = 1;

            foreach(Point p in line)
            {
                for(int x = -r; x <= r; x++)
                {
                    for(int y = -r; y <= r; y++)
                    {
                        if(x*x + y*y <= r * r)
                        {
                            int drawX = p.X + x;
                            int drawY = p.Y + y;
                            if(IsInMapRange(drawX, drawY))
                                map[drawX, drawY] = new Tile(true, GetRandomTileType(set.normals));
                        }
                    }
                }
            }
        }

        static List<Point> CreateFloodFillRegion(int startX, int startY, bool[,] metaVisited) {
            bool[,] isTileVisited = new bool[mapWidth, mapHeight];
            List<Point> tiles = new List<Point>();
            bool valueToCheckFor = map[startX, startY].walkable;

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(startX, startY));
            isTileVisited[startX, startY] = true;

            while(queue.Count > 0)
            {
                Point tile = queue.Dequeue();
                tiles.Add(tile);

                for(int x = tile.X - 1; x <= tile.X + 1; x++)
                {
                    for(int y = tile.Y - 1; y <= tile.Y + 1; y++)
                    {
                        if(IsInMapRange(x, y) && (x == tile.X || y == tile.Y))
                        {
                            if(isTileVisited[x,y] == false && map[x,y].walkable == true)
                            {
                                isTileVisited[x, y] = true;
                                metaVisited[x, y] = true;
                                queue.Enqueue(new Point(x, y));
                            }
                        }
                    }
                }

            }

            return tiles;
        }

        static HashSet<Point> GetLine(Point from, Point to) {
            HashSet<Point> line = new HashSet<Point>();
            int x = from.X;
            int y = from.Y;

            int deltaX = to.X - from.X;
            int deltaY = to.Y - from.Y;

            bool inverted = false;
            int step = Math.Sign(deltaX);
            int gradientStep = Math.Sign(deltaY);

            int longest = Math.Abs(deltaX);
            int shortest = Math.Abs(deltaY);

            if(longest < shortest)
            {
                inverted = true;
                longest = Math.Abs(deltaY);
                shortest = Math.Abs(deltaX);
                step = Math.Sign(deltaY);
                gradientStep = Math.Sign(deltaX);
            }
            int gradientAccumulation = longest / 2;
            for(int i = 0; i < longest; i++)
            {
                line.Add(new Point(x, y));

                if(inverted)
                    y += step;
                else
                    x += step;

                gradientAccumulation += shortest;
                if(gradientAccumulation >= longest)
                {
                    if(inverted)
                        x += gradientStep;
                    else
                        y += gradientStep;
                    gradientAccumulation -= longest;

                }
            }

            return line;

        }

        static bool IsInMapRange(int x, int y) {
            return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
        }
    }
}
