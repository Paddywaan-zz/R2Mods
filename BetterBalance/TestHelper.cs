#define DEBUG
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Paddywan
{
    /// <summary>
    /// 
    /// </summary>
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Paddywan.TestHelper", "TestHelper", "1.0.0")]
    public class TestHelper : BaseUnityPlugin
    {

        /*
         * BoilerPlate's itemSpawner for testing active droptables.
         */
        public static void itemSpawnHelper()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                //We grab a list of all available Tier 1 drops:
                var dropList = Run.instance.availableTier1DropList;

                //Randomly get the next item:
                var nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);

                //Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then finally drop it infront of the player.
                PickupDropletController.CreatePickupDroplet(dropList[nextItem], transform.position, transform.forward * 20f);
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                //We grab a list of all available Tier 2 drops:
                var dropList = Run.instance.availableTier2DropList;

                //Randomly get the next item:
                var nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);

                //Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then finally drop it infront of the player.
                PickupDropletController.CreatePickupDroplet(dropList[nextItem], transform.position, transform.forward * 20f);
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                //We grab a list of all available Tier 3 drops:
                var dropList = Run.instance.availableTier3DropList;

                //Randomly get the next item:
                var nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);

                //Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then finally drop it infront of the player.
                PickupDropletController.CreatePickupDroplet(dropList[nextItem], transform.position, transform.forward * 20f);
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                //We grab a list of all available Equipment drops:
                var dropList = Run.instance.availableEquipmentDropList;

                //Randomly get the next item:
                var nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);

                //Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then finally drop it infront of the player.
                PickupDropletController.CreatePickupDroplet(dropList[nextItem], transform.position, transform.forward * 20f);
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                //We grab a list of all available Lunar drops:
                var dropList = Run.instance.availableLunarDropList;

                //Randomly get the next item:
                var nextItem = Run.instance.treasureRng.RangeInt(0, dropList.Count);

                //Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then finally drop it infront of the player.
                PickupDropletController.CreatePickupDroplet(dropList[nextItem], transform.position, transform.forward * 20f);
            }
        }
    }
}
