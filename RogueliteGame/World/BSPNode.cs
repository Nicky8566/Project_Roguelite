using Microsoft.Xna.Framework;
using System;

namespace RogueliteGame.World
{
    public class BSPNode
    {
        public Rectangle Bounds { get; set; }
        public BSPNode Left { get; set; }
        public BSPNode Right { get; set; }
        public Rectangle? Room { get; set; }

        public BSPNode(Rectangle bounds)
        {
            Bounds = bounds;
        }

        public void Split(Random rng, int minSize)
        {
            // Stop if too small to split
            if (Bounds.Width < minSize * 2 || Bounds.Height < minSize * 2)
                return;

            // Choose split direction
            bool splitHorizontally = Bounds.Width > Bounds.Height
                ? false  // Split vertically if wider
                : Bounds.Height > Bounds.Width
                    ? true  // Split horizontally if taller
                    : rng.Next(2) == 0;  // Random if square

            int splitPosition;
            if (splitHorizontally)
            {
                // Split top/bottom
                splitPosition = rng.Next(minSize, Bounds.Height - minSize);
                Left = new BSPNode(new Rectangle(
                    Bounds.X, Bounds.Y,
                    Bounds.Width, splitPosition
                ));
                Right = new BSPNode(new Rectangle(
                    Bounds.X, Bounds.Y + splitPosition,
                    Bounds.Width, Bounds.Height - splitPosition
                ));
            }
            else
            {
                // Split left/right
                splitPosition = rng.Next(minSize, Bounds.Width - minSize);
                Left = new BSPNode(new Rectangle(
                    Bounds.X, Bounds.Y,
                    splitPosition, Bounds.Height
                ));
                Right = new BSPNode(new Rectangle(
                    Bounds.X + splitPosition, Bounds.Y,
                    Bounds.Width - splitPosition, Bounds.Height
                ));
            }

            // Recursively split children
            Left.Split(rng, minSize);
            Right.Split(rng, minSize);
        }

        public void CreateRooms(Random rng, int minRoomSize, int maxRoomSize)
        {
            // If this is a leaf node (no children), create a room
            if (Left == null && Right == null)
            {
                int roomWidth = rng.Next(minRoomSize, Math.Min(maxRoomSize, Bounds.Width - 2));
                int roomHeight = rng.Next(minRoomSize, Math.Min(maxRoomSize, Bounds.Height - 2));
                
                int roomX = Bounds.X + rng.Next(1, Bounds.Width - roomWidth - 1);
                int roomY = Bounds.Y + rng.Next(1, Bounds.Height - roomHeight - 1);
                
                Room = new Rectangle(roomX, roomY, roomWidth, roomHeight);
            }
            else
            {
                // Recursively create rooms in children
                Left?.CreateRooms(rng, minRoomSize, maxRoomSize);
                Right?.CreateRooms(rng, minRoomSize, maxRoomSize);
            }
        }

        public Rectangle GetRoom()
        {
            if (Room.HasValue)
                return Room.Value;

            Rectangle leftRoom = default;
            Rectangle rightRoom = default;

            if (Left != null)
                leftRoom = Left.GetRoom();
            if (Right != null)
                rightRoom = Right.GetRoom();

            // Return a random room from children
            if (leftRoom != default && rightRoom != default)
                return new Random().Next(2) == 0 ? leftRoom : rightRoom;
            
            return leftRoom != default ? leftRoom : rightRoom;
        }
    }
}