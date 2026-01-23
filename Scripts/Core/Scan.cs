using XRL;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;
using System.Collections.Generic;
using System;

namespace Nexus.Core
{
	/// <summary>
	/// Static helper class for evaluating GameObject states to help determine outcomes.
	/// </summary>
	static class Scan
	{

		/// <summary>
		/// Evaluates alliance, love, and player control.
		/// </summary>
		public static bool IsFriendly(GameObject who, GameObject theVampire)
		{
			if (who is not null && theVampire is not null)
				return who.IsAlliedTowards(theVampire) || who.IsInLoveWith(theVampire) || (theVampire.IsPlayer() && (who.IsPlayerControlled() || who.IsPlayerLed()));
			else
			{
				string msg;
				if (who is null && theVampire is null)
				{
					msg = "who is null and theVampire is null";
				}
				else if (who is null)
					msg = "who is null";
				else
					msg = "theVampire is null";
				MetricsManager.LogModError(ModManager.GetMod("vampirism"), $"IsFriendly received null value, returning false. Null parameter: {msg}");
				return false;
			}
		}
		
		/// <summary>
		/// Returns true/false values from object string properties. Default true.
		/// </summary>
		public static bool ReturnProperty(GameObject theVampire, string flag1, string flag2) => ReturnProperty(theVampire, flag1) || ReturnProperty(theVampire, flag2);


		/// <summary>
		/// Returns true/false values from object string properties. Default true.
		/// </summary>
		public static bool ReturnProperty(GameObject theVampire, string flag)
			=> theVampire.GetStringProperty(flag).EqualsNoCase(true.ToString());


		/// <summary>
		/// Evaluates if the vampire is in a condition wherein they are incapable of activating Feed. Special evaluation for when frenzy is active.
		/// </summary>

		public static bool Incap(GameObject theVampire, bool frenzying)
		 =>
			 theVampire.IsFrozen()
			|| theVampire.IsInStasis()
			|| !theVampire.CanMoveExtremities()
			|| Unaware(theVampire, false)
			|| (theVampire.IsConfused && frenzying) // specifically to end frenzy if confused
			|| (theVampire.HasEffect<StunGasStun>() && !theVampire.IsPlayer());
		//even with useenergy event, still had some bugs associated with effects and conditions that youd normally expect to end a feeding

		/// <summary>
		/// Evaluates if a target lacks awareness of their surroundings, such as stun, sleep, confusion, or paralysys.
		/// </summary>
		public static bool Unaware(GameObject Victim, bool kissing)
		  =>
			//the person being attacked is technically considered aware except in stealth attacks
			                            //which is handled by completely different logic in Nightbeast.Stealthfeeding()
			 Victim.HasEffect<Stun>()
			|| Victim.HasEffect<Paralyzed>()
			|| Victim.HasEffect<Asleep>()
			|| Victim.HasEffect<Exhausted>()
			|| Victim.IsConfused && !Victim.IsPlayer();

		/// <summary>
		/// Default effects list that is recommended to be copied from before making your own custom awareness list.
		/// </summary>
		public static List<Type> DefaultEffectsList = new()
		{
			typeof(Stun), typeof(Paralyzed), typeof(Asleep), typeof(Exhausted)
		};

		///<summary>
		/// This is for setting up your own custom version of awareness, if you feel as though my code doesn't suffice.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="Effects"></param>
		/// <param name="Victim"></param>
		/// <returns></returns>
		/// 
		public static bool Unware(List<Type> Effects, GameObject Victim)
		{
			foreach (Type type in Effects)
			{
				if (Victim.IsConfused || Victim.HasEffect(type))
					return true;
			}
			return false;
		}

	}
}