using BepInEx;
using RoR2;
using UnityEngine;
using RoR2.Networking;
using UnityEngine.Networking;
using R2API.Utils;
using MonoMod.Cil;
using System.Reflection;
using BepInEx.Configuration;

namespace Paddywan
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Paddywan." + modname, modname, modver)]
    public class BepinTemplate : BaseUnityPlugin
    {
        private const string modname = "BepinTemplate", modver = "1.0.0";
    }
}