using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrooxEngine;
using System.Reflection.Emit;

namespace CustomJoinVerifierFix
{
    public class CustomJoinVerifierFix : ResoniteMod
    {
        public override string Name => "CustomJoinVerifierFix";
        public override string Author => "art0007i";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/art0007i/CustomJoinVerifierFix/";
        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("me.art0007i.CustomJoinVerifierFix");
            harmony.PatchAll();

        }
        [HarmonyPatch(typeof(SessionControlDialog), "OnCommonUpdate")]
        class CustomJoinVerifierFixPatch
        {
            public static bool PermCheck(World wld)
            {
                return wld.Configuration.CanChangeProperties();
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
            {
                var worldVerif = typeof(WorldConfiguration).GetField(nameof(WorldConfiguration.CustomJoinVerifier));
                var isAuthor = typeof(World).GetProperty(nameof(World.IsAuthority)).GetMethod;

                var proxFunc = typeof(CustomJoinVerifierFixPatch).GetMethod(nameof(PermCheck));
                
                var foundMatch = false;
                foreach (var code in codes)
                {
                    if (code.LoadsField(worldVerif))
                    {
                        foundMatch = true;
                    }
                    if(foundMatch && code.Calls(isAuthor))
                    {
                        foundMatch = false;
                        yield return new(OpCodes.Call, proxFunc);
                    }
                    else
                    {
                        yield return code;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Security.VerifyJoinRequest), "OnSetAsCustomJoinRequestVerifier")]
        class SetCustomJoinVerifierFixPatch
        {
            public static bool PermCheck(User user)
            {
                return CustomJoinVerifierFixPatch.PermCheck(user.World);
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
            {
                var isHost = typeof(User).GetProperty(nameof(User.IsHost)).GetMethod;

                var proxFunc = typeof(SetCustomJoinVerifierFixPatch).GetMethod(nameof(PermCheck));

                foreach (var code in codes)
                {
                    if (code.Calls(isHost))
                    {
                        yield return new(OpCodes.Call, proxFunc);
                    }
                    else
                    {
                        yield return code;
                    }
                }
            }
        }
    }
}