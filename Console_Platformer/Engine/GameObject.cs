﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Console_Platformer.Engine
{
    [DataContract(IsReference = true)]
    abstract class GameObject
    {
        // Which game engine the gameobject belongs to
        public Engine Engine { get; private set; }
        // In which chunk the GameObject resides
        public Chunk Chunk { get; set; }
        [DataMember]
        public Vec2i Position { get; private set; } //TODO: maybe change this to a readonly Vec2i so that the position cannot be accessed directly
        [DataMember]
        public Sprite[] Sprites { get; private set; } //TODO: perhaps try to add some safety features for indexes etc.
        [DataMember]
        public List<Collider> Colliders { get; private set; }
        [DataMember]
        public bool Collidable { get; set; } // Flag whether the GameObject can collide with other GameObjects //u
        [DataMember]
        public List<GoBind> Binds { get; private set; }
        [DataMember]
        private int _spriteLevel;
        public int SpriteLevel //u
        {
            get { return _spriteLevel; }
            set
            {
                // Checks whether the SpriteLevel value is valid and sets it to minimum priority if it's invalid
                if (value >= 0 && value < Engine.spriteLevelCount) _spriteLevel = value;
                else _spriteLevel = Engine.spriteLevelCount;
            }
        }


        public GameObject(Vec2i position, Engine engine)
        {
            Position = position.Copy();
            Engine = engine;
            Collidable = true;
            SpriteLevel = 5;
            Sprites = new Sprite[Engine.spriteMaxCount];
            Colliders = new List<Collider>();
            Binds = new List<GoBind>();
        }

        // Moves the gameObject and return a boolean to indicate whether the object was moved successfully
        public virtual bool MoveGameObject(int x, int y)
        {
            // Checks if the whole moved sprite is in world bounds and if it is then moves the sprite
            if (Position.X + x < Engine.worldSize.X && // Most far right part of the screen - position + size to get the most far right part of the sprite but - 1 because position is counted as the first character to the left INCLUDING it
                Position.X + x >= 0 &&
                Position.Y + y < Engine.worldSize.Y &&
                Position.Y + y >= 0)
            {
                Position.X += x;
                Position.Y += y; // Moves the player

                // Collision detection
                if (Collidable)
                {
                    var isColliding = CollisionDetection();

                    if (isColliding)
                    {
                        Position.X -= x; // Unmove them because such a movement would result in gameObjects overlaping
                        Position.Y -= y;
                        return false;
                    }
                }

                // Chunk traverse detection
                var newChunkX = Position.X / Engine.chunkSize;
                var newChunkY = Position.Y / Engine.chunkSize;
                if (Chunk != Engine.chunks[newChunkX, newChunkY])
                {
                    Chunk.UnInsertGameObject(this);
                    Chunk = Engine.chunks[newChunkX, newChunkY];

                    if (Engine.IsChunkLoaded(newChunkX, newChunkY))
                    {
                        Chunk.InsertGameObject(this);
                        OnChunkTraverse(newChunkX, newChunkY); 
                    }
                    else
                    {
                        Engine.unloadedChunkTransitionAddGameObjects[newChunkX, newChunkY].Add(this);
                    }
                    
                }

                return true;
            }
            else return false;
        }

        // Returns a boolean to indicate whether a collision was detected
        private bool CollisionDetection()
        {
            var isColliding = false;
            for (int x = 0; x < Engine.chunks.GetLength(0); x++)
            {
                for (int y = 0; y < Engine.chunks.GetLength(1); y++)
                {
                    if (Engine.IsChunkLoaded(x, y))
                    {
                        foreach (var gameObject in Engine.chunks[x, y].gameObjects)
                        {
                            if (this != gameObject && gameObject.Collidable)
                            {
                                if (IsCollidingWith(gameObject))
                                {
                                    isColliding = true;

                                    // Collsion detected
                                    OnCollision(gameObject);
                                    gameObject.OnCollision(this);
                                }
                            }
                        }
                    }
                }
            }

            return isColliding;
        }
        public bool IsCollidingWith(GameObject other)
        {
            foreach (var col in Colliders)
            {
                foreach (var otherCol in other.Colliders)
                {
                    if (Position.X + col.AttachmentPos.X < other.Position.X + otherCol.AttachmentPos.X + otherCol.Size.X &&
                        Position.X + col.AttachmentPos.X + col.Size.X > other.Position.X + otherCol.AttachmentPos.X &&
                        Position.Y + col.AttachmentPos.Y < other.Position.Y + otherCol.AttachmentPos.Y + otherCol.Size.Y &&
                        Position.Y + col.AttachmentPos.Y + col.Size.Y > other.Position.Y + otherCol.AttachmentPos.Y)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected virtual void OnChunkTraverse(int chunkX, int chunkY) { }

        // Updates the animators of GameObject if it has any (if it doesn't, Animation property is set to null and is ignored)
        public virtual void Update()
        {
            foreach (var sprite in Sprites)
            {
                sprite?.Animator?.Update();
            }
        }

        public void OnUnloadedChunkAwake(int chunkX, int chunkY)
        {
            Chunk = Engine.chunks[chunkX, chunkY];
            OnChunkTraverse(chunkX, chunkY);
        }
        public virtual void CompleteDataAfterSerialization(Engine engine, Vec2i index)
        {
            Engine = engine;
            Chunk = engine.chunks[index.X, index.Y];

            foreach (var sprite in Sprites)
            {
                if (sprite != null) sprite.OnDeserialization();
            }

            foreach (var bind in Binds) bind.OnDeserialized(this);
        }

        public virtual void PrepareForDeserialization()
        {
            foreach (var sprite in Sprites)
            {
                if (sprite != null) sprite.PrepareForDeserialization();
            }

            foreach (var bind in Binds)
            {
                bind.IsActive = false;
            }
        }
        public abstract void OnCollision(GameObject collidingObject);
    }
}
