﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using System.Xml.Serialization;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Quests;
using Newtonsoft.Json;
using System.IO;
using Revitalize.Resources;
using Revitalize.Objects;
using Revitalize.Objects.Machines;
using StardewValley.Locations;
using Revitalize.Locations;
using Revitalize.Menus;
using Microsoft.Xna.Framework.Input;
using xTile;
using Revitalize.Persistance;
using Revitalize.Draw;
using Revitalize.Aesthetics;
using Revitalize.Aesthetics.WeatherDebris;
using System.Reflection;
using StardewValley.Menus;
using Revitalize.Resources.DataNodes;

namespace Revitalize
{
    /// <summary>
    /// TODO:
    /// Get Time lapse code working so that machines propperly work though the night since I technically remove them.
    /// Art. Lots of Art.
    /// Clean up the freaking code. Geeze it's messy.
    /// 
    /// </summary>


    public class Class1 :Mod
    {
        public static string key_binding="P";
        public static string key_binding2 = "E";
        public static string key_binding3 = "F";
      public static  bool useMenuFocus;
        public static string path;
        public static string contentPath;
        const int range = 1;

        public static MouseState mState;

      public static  MapSwapData persistentMapSwap;

      public static bool mouseAction;

        bool gametick;

        bool mapWipe;
      public static  bool hasLoadedTerrainList;
        List<GameLoc> newLoc;


        public static bool paintEnabled;

      public static  bool gameLoaded;

       public static List<LocalizedContentManager> modContent;


        public override void Entry(IModHelper helper)
        {
           
            string first=StardewModdingAPI.Program.StardewAssembly.Location;
            contentPath= first.Remove(first.Length - 19, 19);
            StardewModdingAPI.Events.ControlEvents.KeyPressed += ShopCall;
            StardewModdingAPI.Events.ControlEvents.MouseChanged += ControlEvents_MouseChanged;
            StardewModdingAPI.Events.GameEvents.UpdateTick +=gameMenuCall;
           //  StardewModdingAPI.Events.GameEvents.UpdateTick += BedCleanUpCheck;
            StardewModdingAPI.Events.GameEvents.GameLoaded += GameEvents_GameLoaded;
            StardewModdingAPI.Events.GameEvents.OneSecondTick += MapWipe;
            StardewModdingAPI.Events.TimeEvents.DayOfMonthChanged += Util.ResetAllDailyBooleans;
            StardewModdingAPI.Events.SaveEvents.AfterLoad += SaveEvents_AfterLoad;

            StardewModdingAPI.Events.SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            StardewModdingAPI.Events.SaveEvents.AfterSave += SaveEvents_AfterSave;
            StardewModdingAPI.Events.SaveEvents.AfterLoad += SaveEvents_AfterSave;

            StardewModdingAPI.Events.GameEvents.UpdateTick += GameEvents_UpdateTick;

            StardewModdingAPI.Events.GraphicsEvents.OnPreRenderHudEvent += GraphicsEvents_OnPreRenderHudEvent;

            StardewModdingAPI.Events.GraphicsEvents.OnPostRenderHudEvent += draw;

          

            //StardewModdingAPI.Events.TimeEvents.DayOfMonthChanged += Util.WaterAllCropsInAllLocations;
            hasLoadedTerrainList = false;
            path = Helper.DirectoryPath;
            newLoc = new List<GameLoc>();


            PlayerVariables.initializePlayerVariables();
            Log.AsyncG("Revitalize: Running on API Version: " +StardewModdingAPI.Constants.ApiVersion);
          
            useMenuFocus = true;
            Lists.loadAllListsAtEntry();

            if (File.Exists(Path.Combine(path, "xnb_node.cmd")))
            {
                paintEnabled = true;
                Log.AsyncG("Revitalize: Paint Module Enabled");
            }
            else
            {
                paintEnabled = false;
                Log.AsyncG("Revitalize: Paint Module Disabled");
            }

        }



        private void draw(object sender, EventArgs e)
        {
            WeatherDebrisSystem.draw();
        }

        private void GraphicsEvents_OnPreRenderHudEvent(object sender, EventArgs e)
        {
            if (gameLoaded == true)
            {
                ThingsToDraw.drawAllHuds();
            }
        }



        /// <summary>
        /// Used to load in information post load such as custom farm maps.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            Serialize.createDirectories();
            Util.loadMapSwapData();
            gameLoaded = true; 
        }//end of function;

        private void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            Serialize.createDirectories();
            Serialize.restoreAllModObjects();
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            Serialize.cleanUpInventory(); 
            Serialize.cleanUpWorld(); //grabs all of the items that im tracking and serializes them
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.player.isMoving() == true && hasLoadedTerrainList == false)
            {
                Lists.loadAllListsAfterMovement();
                Util.WaterAllCropsInAllLocations();
            }
            WeatherDebrisSystem.update();
        }
   

        private void ControlEvents_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
          
            if (Game1.activeClickableMenu != null) return;
            if (Game1.eventUp == true) return;
             mState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            if (mState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed || mState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                if (Game1.player.ActiveObject != null)
                {
                    string s = Game1.player.ActiveObject.getCategoryName();
                   // Log.AsyncC(s);
                    if (Dictionaries.interactionTypes.ContainsKey(s))
                    {
                        Dictionaries.interactFunction f;
                        Dictionaries.interactionTypes.TryGetValue(s,out f);
                        if (mouseAction == false)
                        {
                            f.Invoke();
                            mouseAction = true;
                            return;
                        }  
                    }
                }
                else {
                    return;
                }
                //this.minutesUntilReady = 30;
            }
            else if (mState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released && mState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                mouseAction = false;
                //this.minutesUntilReady = 30;
            }
        }

        private void GameEvents_GameLoaded(object sender, EventArgs e)
        {
            modContent = new List<LocalizedContentManager>();//new LocalizedContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
            Dictionaries.initializeDictionaries();
            Lists.initializeAllLists();

            mapWipe = false;

        }


        public void MapWipe(object sender, EventArgs e)
        {
          
            if (mapWipe == false) return;
            if (Game1.hasLoadedGame == false) return;
            if (Game1.player == null) return;
            if (Game1.player.currentLocation == null) return;
            if (Game1.player.isMoving() == true)
            {


                foreach (var v in Game1.locations)
                {
                    GameLoc R = (new GameLoc(v.Map, v.name));

                    if (R.name == "Town" || R.name == "town")
                    {
                        Log.AsyncO("Adding Town");
                        R = new ModTown(v.Map, v.name);
                    }
                    newLoc.Add(R);
                    Log.AsyncC("DONE1");
                }
                Game1.locations.Clear();
                foreach (var v in newLoc)
                {
                    Game1.locations.Add(v);
                    Log.AsyncC("DONE2");
                }
                Log.AsyncC("DONE");
                mapWipe = false;
            }


            
            
        }

        /*
        private void BedCleanUpCheck(object sender, EventArgs e)
        {
            //Game1.options.menuButton = null;

            if (Game1.hasLoadedGame == false) return;
            if (Game1.player == null) return;
            if (Game1.player.currentLocation == null) return;
            //Log.Info(Game1.activeClickableMenu.GetType());

            if (Game1.player.currentLocation.name == "FarmHouse")
            {
                Vector2 playerAdj = Game1.player.mostRecentBed;

                int x = Convert.ToInt32(playerAdj.X)/Game1.tileSize;
                int y = Convert.ToInt32(playerAdj.Y)/Game1.tileSize;


                
                    if ((Game1.player.getTileY() >= y - range && Game1.player.getTileY() <= y + range) && (Game1.player.getTileX() >= x - range && Game1.player.getTileY() <= x + range))
                    {
                    if (hasCleanedUp == false)
                    {
                        Log.AsyncC("CleanUp!");
                        Serialize.cleanUpInventory();
                        hasCleanedUp = true;
                    }
                }
                    else
                    {
                    Serialize.restoreInventory();
                        hasCleanedUp = false;
                    }     
            }
        }
        */
       

        private void gameMenuCall(object sender, EventArgs e)
        {

            
            if (gametick == true)
            {
               // System.Threading.Thread.Sleep(1);
               
                 //  Game1.activeClickableMenu = new  Revitalize.Menus.GameMenu();
            }
            gametick = false;
            
        }


        private void ShopCall(object sender, StardewModdingAPI.Events.EventArgsKeyPressed e)
        {
            if (e.KeyPressed.ToString() == "J")
            {
                Log.AsyncC("Mouse Position " + Game1.getMousePosition());
            }

            // Game1.currentSeason = "spring";
            Game1.player.money = 9999;
          //  Log.AsyncG(Game1.tileSize);

            //Game1.timeOfDay = 2500;
            if (Game1.activeClickableMenu != null) return;
            if (e.KeyPressed.ToString() == key_binding)
            {

                List<Item> objShopList = new List<Item>();
                List<Item> newInventory = new List<Item>();

                TextureDataNode font;
                Dictionaries.spriteFontList.TryGetValue("0", out font);
                objShopList.Add(new SpriteFontObject(0, Vector2.Zero, font.path, Color.White));
                objShopList.Add(new Magic.Alchemy.Objects.BagofHolding(0, Vector2.Zero, new List<List<Item>>()
                {
                    new List<Item>()
                    {
                        Capacity=6
                    },
                    new List<Item>()
                    {
                        Capacity=10
                    },
                    new List<Item>()
                    {
                        Capacity=15
                    }
                }, Color.White));
                //  objShopList.Add(new Spawner(3, Vector2.Zero, 9));
                objShopList.Add(new Light(0, Vector2.Zero, LightColors.Aquamarine,LightColors.Aquamarine,false));
                objShopList.Add(new Quarry(3, Vector2.Zero,9,"copper"));
                objShopList.Add(new Quarry(3, Vector2.Zero, 9, "iron"));
                objShopList.Add(new Decoration(3, Vector2.Zero));
                objShopList.Add(new StardewValley.Object(495, 1));
                objShopList.Add(new StardewValley.Object(496, 1));
                objShopList.Add(new StardewValley.Object(497, 1));
                objShopList.Add(new StardewValley.Object(498, 1));
                objShopList.Add(new StardewValley.Object(770, 1));

                objShopList.Add(Canvas.addCanvasWithCheck(0, Vector2.Zero, Canvas.blankTexture, null));
                
                objShopList.Add(new StardewValley.Object(475, 1));
                foreach (var v in objShopList)
                {
                    newInventory.Add(v);
                 //   Log.AsyncG("GRRR");
                }
                objShopList.Add(new GiftPackage(1,"Generic Gift Package",Vector2.Zero,newInventory,1000,Util.invertColor(LightColors.Orange)));

                // my_shop_list.Add((new Decoration(1120, Vector2.Zero)));
                objShopList.Add(new ExtraSeeds(1, Vector2.Zero));
                objShopList.Add(new ExtraSeeds(2, Vector2.Zero));


                foreach(KeyValuePair<int,Spell> v in Dictionaries.spellList)
                {
                    objShopList.Add(v.Value);
                }

                Game1.activeClickableMenu = new StardewValley.Menus.ShopMenu(objShopList, 0, null);
                
                if (Game1.player == null) return;
                
            }

            
            if (e.KeyPressed.ToString() == key_binding2)
            {
                gametick = true;
            }

            if (e.KeyPressed.ToString() == "J")
            {
                //if(Game1.activeclickablemenu is TitleMenu or CharacterCreationMenu
                if (Game1.player.currentLocation.name != "Farm")
                {
                    Game1.activeClickableMenu = new FarmOptionsMenu();
                }
                else
                {
                    Game1.showRedMessage("Can't change farm map here!");
                }
            }

            if (e.KeyPressed.ToString() == "V")
            {
                newDebris();
            }

            if (e.KeyPressed.ToString() == "G")
            {
                WeatherDebrisSystem.speedUpWindAndClear(0.001f);
            }

            if (e.KeyPressed.ToString() == "J")
            {
                Log.AsyncC("Player Position " + Game1.player.getTileLocation());
                Log.AsyncC("Mouse Position " + Game1.currentCursorTile);
            }
        }
            

        public void newDebris()
        {
            //  Game1.debrisWeather.Add(new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Width), (float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Height)), 0, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
            //  Game1.debrisWeather.Add(new WeatherDebrisPlus(new Vector2((float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Width), (float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Height)), new Rectangle(338, 400, 8, 8),0,999, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
            //  WeatherDebris w = new WeatherDebris();
            //   Game1.debrisWeather.Add(new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Width), (float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Height)), 0, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
            // WeatherDebris w = new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Width), (float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Height)), 0, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f);
            WeatherDebrisPlus w= new WeatherDebrisPlus(new Vector2((float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Width), (float)Game1.random.Next(0, Game1.graphics.GraphicsDevice.Viewport.Height)), new Rectangle(338, 400, 8, 8), 0, 4, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f);
            WeatherDebrisSystem.addMultipleDebrisFromSingleType(new weatherNode(w, 20));
           Game1.isDebrisWeather = true;

        }

    }
}
