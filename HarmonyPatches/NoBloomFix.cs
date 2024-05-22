using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection.Emit;
using JetBrains.Annotations;

namespace Camera2.HarmonyPatches
{
    /*
     * Now that I have (Somewhat) better understanding of the issue...
     * FOR SOME REASON, the OnRenderImage() Callback in the ImageEffectController influences
     * the Camera - EVEN THO THE COMPONENT IS DISABLED AND THE METHOD IS NOT CALLED?!
     * So the patches below FORCE ENABLES it every frame, only for those to then call....
     * => MainEffectController.ImageEffectControllerCallback()
     * => this._mainEffectContainer.mainEffect(NoPostProcessMainEffectSO).Render
     * => Inherited from MainEffectSO
     * => EMPTY - So, the RenderTexture never makes it to the dest
     * BUT WHY DOES THIS MATTER WHEN IT DOESNT GET CALLED IN THE FIRST PLACE?! UNITY??
     */
    [HarmonyPatch(typeof(MainEffectSO), nameof(MainEffectSO.Render))]
    internal static class BloomRendererInstantiateFix3
    {
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.First().opcode != OpCodes.Ret 
                ? instructions 
                : new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_1), 
                    new CodeInstruction(OpCodes.Ldarg_2), 
                    new CodeInstruction(OpCodes.Call, typeof(Graphics)
                        .GetMethod("Blit", new[]
                        {
                            typeof(RenderTexture), 
                            typeof(RenderTexture)
                        })), 
                    new CodeInstruction(OpCodes.Ret)
                };
        }
    }

    [HarmonyPatch(typeof(MainEffectController), "OnPreRender")]
    internal static class BloomRendererInstantiateFix4
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(ImageEffectController ____imageEffectController)
        {
            ____imageEffectController.enabled = true;
        }
    }
}