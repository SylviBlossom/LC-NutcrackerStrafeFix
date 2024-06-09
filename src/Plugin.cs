using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace NutcrackerStrafeFix;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
	private void Awake()
	{
		new ILHook(
			typeof(NutcrackerEnemyAI).GetMethod("AimGun", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(),
			NutcrackerEnemyAI_AimGun
		);

		// Plugin startup logic
		Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
	}

	private void NutcrackerEnemyAI_AimGun(ILContext il)
	{
		var cursor = new ILCursor(il);

		if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<EnemyAI>("inSpecialAnimation")))
		{
			Logger.LogError("Failed IL hook for NutcrackerEnemyAI.AimGun @ First inSpecialAnimation assignment");
			return;
		}

		cursor.Emit(OpCodes.Ldloc_1);
		cursor.EmitDelegate<Action<NutcrackerEnemyAI>>(self =>
		{
			self.inSpecialAnimation = false;
			self.updatePositionThreshold = 0.25f;
		});

		if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<EnemyAI>("inSpecialAnimation")))
		{
			Logger.LogError("Failed IL hook for NutcrackerEnemyAI.AimGun @ Last inSpecialAnimation assignment");
			return;
		}

		cursor.Emit(OpCodes.Ldloc_1);
		cursor.EmitDelegate<Action<NutcrackerEnemyAI>>(self =>
		{
			self.updatePositionThreshold = 1f;
		});

		Logger.LogInfo("Hooked successfully");
	}
}