using System;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public struct Feature
    {
        public float Distance; // = float.MaxValue;
        public Vector2 Normal; // = Vector2.Zero;
        public Vector2 Position; // = Vector2.Zero;

        public Feature(Vector2 position)
        {
            Position = position;
            Normal = new Vector2(0, 0);
            Distance = float.MaxValue;
        }

        public Feature(Vector2 position, Vector2 normal, Single distance)
        {
            Position = position;
            Normal = normal;
            Distance = distance;
        }

        //TODO: There might be a better way to generate the hashcode
        public override int GetHashCode()
        {
            return (int)(Normal.X + Normal.Y + Position.X + Position.Y + Distance);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Feature))
                return false;

            return Equals((Feature)obj);
        }

        public bool Equals(Feature other)
        {
            return ((Normal == other.Normal) && (Position == other.Position) && (Distance == other.Distance));
        }

        public static bool operator ==(Feature first, Feature second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(Feature first, Feature second)
        {
            return !first.Equals(second);
        }
    }
}