/*
    Challenegrs Katarina
 * v1.0
    - Initial release.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using System.Drawing;
using SharpDX;

namespace Challengers_Katarina
{
    class Program
    {
        public const string CharName = "Katarina";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;
        public static float DoingCombo;
        public static SpellSlot IgniteSlot;
        public static SpellSlot FlashSlot;
        public static Menu Config;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName)
            {
                return;
            }

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            Q = new Spell(SpellSlot.Q, 675f);
            W = new Spell(SpellSlot.W, 375f);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 550f);

            Q.SetTargetted(0.25f, 1400f);
            E.SetTargetted(0.25f, 1400f);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu(CharName, CharName, true);

            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

            var ts = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(ts);
            Config.AddSubMenu(ts);

            Config.AddSubMenu(new Menu("Combo settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("itemsCombo", "Use Items")).SetValue(true);

            Config.AddToMainMenu();

            Game.OnUpdate += OnGameUpdate;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;

            Game.PrintChat(">> Challengers Katarina Loaded!");
        }

        private static void OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Orbwalker.SetMovement(false);
                    Orbwalker.SetAttack(false);
                    Combo(target);
                    Orbwalker.SetAttack(true);
                    Orbwalker.SetMovement(true);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Game.PrintChat(">> Not working!");
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Game.PrintChat(">> Not working!");
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Game.PrintChat(">> Not working!");
                    break;
            }
        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = Environment.TickCount > DoingCombo;
        }

        private static void Combo(Obj_AI_Base target)
        {
            if ((target == null) || Environment.TickCount < DoingCombo)
            {
                return;
            }

            if (Config.Item("itemsCombo").GetValue<bool>() && target != null)
            {
                //Items.UseItem(3128, target);
            }

            var useQ = Config.Item("useQ").GetValue<bool>();
            var useW = Config.Item("useW").GetValue<bool>();
            var useE = Config.Item("useE").GetValue<bool>();
            var useR = Config.Item("useR").GetValue<bool>();

            if (Q.IsReady() && useQ)
            {
                Q.Cast(target);
            }

            if (E.IsReady() && useE)
            {
                E.Cast(target);
            }

            if (W.IsReady() && useW)
            {
                W.Cast();
            }

            if (R.IsReady() && useR && !Q.IsReady() && !W.IsReady() && !E.IsReady())
            {
                R.Cast(target);
            }

            if (IgniteSlot != SpellSlot.Unknown && target != null &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.Distance(target, false) < 600 &&
                ObjectManager.Player.GetSpellDamage(target, IgniteSlot) > target.Health)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
            }
        }
    }
}