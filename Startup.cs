using System.Collections.Generic;
using System.Linq;
using System;

using Qud.UI;

using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

using static StealthSystemPrototype.Options;

namespace StealthSystemPrototype
{
    [HasModSensitiveStaticCache]
    [HasGameBasedStaticCache]
    [HasCallAfterGameLoaded]
    public static class Startup
    {
        [ModSensitiveCacheInit]
        public static void ModSensitiveCacheInit()
        {
            // Called at game startup and whenever mod configuration changes
        }

        [GameBasedCacheInit]
        public static void GameBasedCacheInit()
        {
            // Called once when world is first generated.

            // The.Game registered events should go here.
        }

        // [PlayerMutator]

        // The.Player.FireEvent("GameRestored");
        // AfterGameLoadedEvent.Send(Return);  // Return is the game.

        [CallAfterGameLoaded]
        public static void OnLoadGameCallback()
        {
            // Gets called every time the game is loaded but not during generation
        }
    }

    // [ModSensitiveCacheInit]

    // [GameBasedCacheInit]

    [PlayerMutator]
    public class OnPlayerLoad : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            // Gets called once when the player is first generated
        }
    }

    [PlayerMutator]
    public class TestKit_OnPlayerLoad : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            if (DebugEnableTestKit)
                AssignTestKit(player);
        }

        public static void AssignTestKit(GameObject player)
        {
            if (player == null)
                return;

            List<string> attributeNames = new()
            {
                "Intelligence",
                "Agility",
                "Ego",
            };

            foreach (string attribute in attributeNames)
                player.AddBaseStat(attribute, 10);

            if (player.GetStat("SP") is Statistic sPStat)
                sPStat.BaseValue += 2500;


            List<string> prereqSkillClasses = new()
            {
                nameof(Cudgel),
                nameof(Cudgel_Bludgeon),
                nameof(ShortBlades),
                nameof(ShortBlades_Expertise),
            };

            foreach (string prereqSkillClass in prereqSkillClasses)
                player.AddSkill(prereqSkillClass);
        }
    }

    // [CallAfterGameLoaded]
}