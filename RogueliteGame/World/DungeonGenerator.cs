using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RogueliteGame.World
{
    public class DungeonGenerator
    {
        private Random rng;

        public DungeonGenerator(int seed)
        {
            rng = new Random(seed);
        }

        public Dungeon Generate(int width, int height)
        {
            Dungeon dungeon = new Dungeon(width, height);
            
            // Create BSP tree
            BSPNode root = new BSPNode(new Rectangle(0, 0, width, height));
            root.Split(rng, 8);  // Minimum room size of 8 tiles
            
            // Create rooms in leaf nodes
            root.CreateRooms(rng, 4, 10);  // Rooms between 4x4 and 10x10
            
            // Carve rooms into dungeon
            CarveRooms(root, dungeon);
            
            // Connect rooms with corridors
            ConnectRooms(root, dungeon);
            
            return dungeon;
        }

        private void CarveRooms(BSPNode node, Dungeon dungeon)
        {
            if (node == null)
                return;

            if (node.Room.HasValue)
            {
                Rectangle room = node.Room.Value;
                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    for (int x = room.X; x < room.X + room.Width; x++)
                    {
                        dungeon.SetTile(x, y, TileType.Floor);
                    }
                }
            }

            CarveRooms(node.Left, dungeon);
            CarveRooms(node.Right, dungeon);
        }

        private void ConnectRooms(BSPNode node, Dungeon dungeon)
        {
            if (node == null)
                return;

            if (node.Left != null && node.Right != null)
            {
                Rectangle leftRoom = node.Left.GetRoom();
                Rectangle rightRoom = node.Right.GetRoom();

                Point leftCenter = leftRoom.Center;
                Point rightCenter = rightRoom.Center;

                // Create L-shaped corridor
                if (rng.Next(2) == 0)
                {
                    // Horizontal then vertical
                    CreateHorizontalCorridor(dungeon, leftCenter.X, rightCenter.X, leftCenter.Y);
                    CreateVerticalCorridor(dungeon, rightCenter.X, leftCenter.Y, rightCenter.Y);
                }
                else
                {
                    // Vertical then horizontal
                    CreateVerticalCorridor(dungeon, leftCenter.X, leftCenter.Y, rightCenter.Y);
                    CreateHorizontalCorridor(dungeon, leftCenter.X, rightCenter.X, rightCenter.Y);
                }
            }

            ConnectRooms(node.Left, dungeon);
            ConnectRooms(node.Right, dungeon);
        }

        private void CreateHorizontalCorridor(Dungeon dungeon, int x1, int x2, int y)
{
    int startX = Math.Min(x1, x2);
    int endX = Math.Max(x1, x2);

    for (int x = startX; x <= endX; x++)
    {
        // Make corridor 3 tiles wide
        dungeon.SetTile(x, y - 1, TileType.Floor);
        dungeon.SetTile(x, y, TileType.Floor);
        dungeon.SetTile(x, y + 1, TileType.Floor);
    }
}

       private void CreateVerticalCorridor(Dungeon dungeon, int x, int y1, int y2)
{
    int startY = Math.Min(y1, y2);
    int endY = Math.Max(y1, y2);

    for (int y = startY; y <= endY; y++)
    {
        // Make corridor 3 tiles wide
        dungeon.SetTile(x - 1, y, TileType.Floor);
        dungeon.SetTile(x, y, TileType.Floor);
        dungeon.SetTile(x + 1, y, TileType.Floor);
    }
}
    }
}