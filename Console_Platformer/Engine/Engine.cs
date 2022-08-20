﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_Platformer.Engine
{
    abstract class Engine
    {
        // Public variables
        public bool gameShouldClose = false;
        public int deltaTime = 0; // Miliseconds since last frame
        public string title = "default title";
        public readonly Vec2i worldSize = new Vec2i(100 * chunkSize, 100 * chunkSize); //TODO: make it so chunk count and world size must match
        public readonly string pixelSpacingCharacters = " ";
        public readonly char backgroudPixel = ' ';
        public readonly int spriteLevelCount = 10;
        public readonly int spriteMaxCount = 10;

        public const int chunkSize = 100;
        public readonly int chunkLoadRadius = 3;
        public readonly Chunk[,] chunks = new Chunk[100, 100];
        public static Random gRandom = new Random();

        //"  " <and> font 20 | width 70 | height 48 <or> font 10 | width 126 | height 90 <or> font 5 | width 316 | height 203
        //" " <and> font 10 | width 189 | height 99
        public Camera Camera { get; private set; } 
        protected Renderer Renderer { get; private set; }
        public ImputManager ImputManager { get; private set; }

        private List<GameObject>[] gameObjectRenderLists;
        private DateTime lastFrame;
        private readonly int milisecondsForNextFrame = 40;

        //debug 
        public readonly bool allowDebug = true;
        public readonly int debugLinesCount = 10;
        public readonly int debugLinesLength = 40;
        public string[] debugLines;


        // Applicaton loop
        public Engine()
        {
            // Loading 
            OnEngineLoad();
            OnLoad();

            while (!gameShouldClose)
            {
                if (deltaTime > milisecondsForNextFrame)
                {

                    debugLines[1] = $"DeltaTime:  {deltaTime}";
                    debugLines[2] = $"FPS: {1000 / deltaTime}";

                    lastFrame = DateTime.Now;

                    // Updating
                    EngineUpdate();
                    Update();

                    // Rendering
                    Renderer.Render();
                }

                deltaTime = (int)(DateTime.Now - lastFrame).TotalMilliseconds;
            }
        }

        // Functions required to run each frame by the engine
        private void EngineUpdate()
        {
            // Registers imput
            ImputManager.UpdateImput(this);

            // Updates all GameObject
            foreach(var chunk in chunks)
            {
                if (chunk.IsLoaded)
                {
                    foreach (var gameObject in chunk.gameObjects)
                    {
                        gameObject.Update();
                    } 
                }
            }

            // Lazy removing game objects. 
            foreach (var chunk in chunks)
            {
                foreach (var gameObject in chunk.gameObjectsToRemove)
                {
                    chunk.gameObjects.Remove(gameObject);
                }
                chunk.gameObjectsToRemove.Clear();
            }

            // Lazy adding GameObjects
            foreach (var chunk in chunks)
            {
                foreach (var gameObject in chunk.gameObjectsToAdd)
                {
                    chunk.gameObjects.Add(gameObject);
                }
                chunk.gameObjectsToAdd.Clear();
            }
        }


        private void OnEngineLoad()
        {
            // Console settings
            Console.CursorVisible = false;
            Console.Title = title;
            Console.OutputEncoding = Encoding.Unicode;

            // Initialise chunks
            for (var x = 0; x < chunks.GetLength(0); x++)
            {
                for (var y = 0; y < chunks.GetLength(1); y++)
                {
                    chunks[x, y] = new Chunk();
                }
            }

            // Debug
            debugLines = new string[debugLinesCount];
            for (var i = 0; i < debugLines.Length; i++)
            {
                debugLines[i] = "";
            }

            // Set up render lists
            gameObjectRenderLists = new List<GameObject>[spriteLevelCount];
            for (var i = 0; i < gameObjectRenderLists.Length; i++)
            {
                gameObjectRenderLists[i] = new List<GameObject>();
            }

            // Set up frame timers
            lastFrame = DateTime.Now;
            //deltaTime = TimeSpan.Zero;

            // Set up the Renderer, the Camera, the ImputManager and initialize the static Resourcemanager
            Camera = new Camera(new Vec2i(0, 0), new Vec2i(189, 99));
            Renderer = new Renderer(this, gameObjectRenderLists);
            ImputManager = new ImputManager();
            ResourceManager.Init();
        }


        // Adds and deletes gameobjects
        public void AddGameObject(GameObject gameObject)
        {
            chunks[gameObject.Position.X / chunkSize, gameObject.Position.Y / chunkSize].gameObjectsToAdd.Add(gameObject);
            gameObjectRenderLists[gameObject.SpriteLevel].Add(gameObject);
        }
        public void RemoveGameObject(GameObject gameObject)
        {
            gameObject.Chunk.gameObjectsToRemove.Add(gameObject);
            gameObjectRenderLists[gameObject.SpriteLevel].Remove(gameObject);
        }

        protected abstract void OnLoad();
        protected abstract void Update();
    }
}