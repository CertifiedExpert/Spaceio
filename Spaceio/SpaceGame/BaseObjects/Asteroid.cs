﻿using Spaceio.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame
{
    [DataContract(IsReference = true)]
    class Asteroid : BaseObject
    {
        public Asteroid(Vec2i position, Vec2i maxSize, Game game) : base(position, game)
        {
            SpriteLevel = 7;

            RandomlyGenerate(maxSize.X, maxSize.Y);
        }

        private void RandomlyGenerate(int x, int y)
        {
            // Generates 2 random colliders and sprites for the asteroid
            var borderX = (int)(x * 0.15f);
            var minXLength = (int)(x * 0.35f);
            var borderY = (int)(y * 0.15f);
            var minYLength = (int)(y * 0.35f);
            int corner1, corner2, corner3, corner4;
            corner1 = Game.Random.Next(borderX, x / 2 - minXLength / 2);             //top
            corner2 = Game.Random.Next(x / 2 + minXLength / 2, x - borderX); //bottom
            corner3 = Game.Random.Next(borderY, y / 2 - minYLength / 2);             //left
            corner4 = Game.Random.Next(y / 2 + minYLength / 2, y - borderY); //right

            var size1 = new Vec2i(Math.Abs(corner1 - corner2) + 1, y);
            var attach1 = new Vec2i(Math.Min(corner1, corner2), 0);
            var bitmap1 = Bitmap.CreateStaticFillBitmap(size1, '⬛');
            Sprites[0] = new Sprite(bitmap1, attach1); 
            Colliders.Add(new Collider(size1, attach1));

            var size2 = new Vec2i(x, Math.Abs(corner3 - corner4) + 1);
            var attach2 = new Vec2i(0, Math.Min(corner3, corner4));
            var bitmap2 = Bitmap.CreateStaticFillBitmap(size2, '⬛');
            Sprites[1] = new Sprite(bitmap2, attach2);
            Colliders.Add(new Collider(size2, attach2));
        }
    }
}