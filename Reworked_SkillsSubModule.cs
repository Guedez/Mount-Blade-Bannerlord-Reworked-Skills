using System;
using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using System.Linq;
using System.Text;
using System.Reflection;
using TaleWorlds.Core;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection;
using System.IO;
using TaleWorlds.Library;
using System.Xml;

namespace Reworked_Skills {
    public class Reworked_SkillsSubModule : MBSubModuleBase {
        public static float __ATTR_VALUE = 5;
        public static float __FOCUS_VALUE = 10;
        public static bool __DEFAULTLEARNING = false;
        public static bool __DEFAULTNORMALIZEDLEARNING = true;
        public static bool __FIXEDRATE = false;
        public static float __FIXEDLEARNINGRATE = 9;
        public static float __SKILLPNLT = 100;
        public static float __SKILLPNLTDV = 15;
        public static int __LEANRINGLIMIT = 240;


        public static MethodInfo GetTooltipForAccumulatingPropertyWithResult;
        protected override void OnSubModuleLoad() {
            base.OnSubModuleLoad();
            try {
                XmlDocument xmlDocument = new XmlDocument();
                string appSettings = String.Concat(BasePath.Name, "Modules/Reworked_Skills/config.xml");
                xmlDocument.Load(appSettings);
                XmlNode xmlNodes = xmlDocument.SelectSingleNode("ReworkedSkills");
                foreach (XmlNode n in xmlNodes.ChildNodes) {
                    switch (n.Name.ToLower()) {
                        case "bonusperattribute":
                            float.TryParse(n.InnerText, out __ATTR_VALUE);
                            break;
                        case "bonusperfocus":
                            float.TryParse(n.InnerText, out __FOCUS_VALUE);
                            break;
                        case "experiencemultipliermode":
                            switch (n.InnerText.ToLower()) {
                                case "default":
                                    __DEFAULTLEARNING = true;
                                    break;
                                case "defaultnormalized":
                                    __DEFAULTNORMALIZEDLEARNING = true;
                                    break;
                                case "fixed":
                                    __FIXEDRATE = true;
                                    break;
                            }
                            break;
                        case "fixedlearningrate":
                            float.TryParse(n.InnerText, out __FIXEDLEARNINGRATE);
                            break;
                        case "normalizedbasepenalty":
                            float.TryParse(n.InnerText, out __SKILLPNLT);
                            break;
                        case "normalizeddivider":
                            float.TryParse(n.InnerText, out __SKILLPNLTDV);
                            break;
                        case "learninglimit":
                            int.TryParse(n.InnerText, out __LEANRINGLIMIT);
                            break;

                    }

                }

                if (GetTooltipForAccumulatingPropertyWithResult == null) {
                    GetTooltipForAccumulatingPropertyWithResult = typeof(CampaignUIHelper).GetMethod("GetTooltipForAccumulatingPropertyWithResult", BindingFlags.NonPublic | BindingFlags.Static);
                }

                Harmony patcher = new Harmony("Reworked_SkillsSubModulePatcher");
                patcher.PatchAll();
            } catch (Exception exception1) {
                string message;
                Exception exception = exception1;
                string str = exception.Message;
                Exception innerException = exception.InnerException;
                if (innerException != null) {
                    message = innerException.Message;
                } else {
                    message = null;
                }
                MessageBox.Show(string.Concat("Reworked_SkillsSubModule Error patching:\n", str, " \n\n", message));
            }
        }
    }
    [HarmonyPatch(typeof(CharacterObject), "GetSkillValue")]
    class Patch1 {
        static bool Prefix(CharacterObject __instance, CharacterSkills ____characterSkills, ref int __result, SkillObject skill) {
            int focus = __instance.HeroObject.HeroDeveloper.GetFocus(skill);
            int attr = __instance.HeroObject.GetAttributeValue(skill.CharacterAttributesEnum);
            if (__instance.IsHero) {
                __result = (int)(__instance.HeroObject.GetSkillValue(skill) + focus * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
            } else {
                __result = (int)(____characterSkills.GetPropertyValue(skill) + focus * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(BasicCharacterObject), "GetSkillValue")]
    class Patch2 {
        static bool Prefix(CharacterObject __instance, CharacterSkills ____characterSkills, ref int __result, SkillObject skill) {
            int focus = __instance.HeroObject.HeroDeveloper.GetFocus(skill);
            int attr = __instance.HeroObject.GetAttributeValue(skill.CharacterAttributesEnum);
            __result = (int)(____characterSkills.GetPropertyValue(skill) + focus * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
            return false;
        }
    }

    [HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "CalculateLearningLimit", typeof(Hero), typeof(SkillObject), typeof(StatExplainer))]
    public class Patch3 {
        public static TextObject desription = new TextObject("{=MRktqZwu}Skill Focus", (Dictionary<string, TextObject>)null);
        static bool Prefix(DefaultCharacterDevelopmentModel __instance, Hero hero, SkillObject skill, StatExplainer explainer, ref int __result) {
            __result = Reworked_SkillsSubModule.__LEANRINGLIMIT;
            return false; // make sure you only skip if really necessary
        }
    }

    [HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "CalculateLearningLimit", typeof(int), typeof(int), typeof(TextObject), typeof(StatExplainer))]
    class Patch4 {
        private static TextObject _LevelText = new TextObject("{=RSaSKILV}Base Skill", (Dictionary<string, TextObject>)null);
        public static int SKILLLEVEL;

        static bool Prefix(DefaultCharacterDevelopmentModel __instance, int attributeValue, int focusValue, TextObject attributeName, StatExplainer explainer, ref int __result) {
            if (explainer != null) {
                ExplainedNumber explainedNumber = new ExplainedNumber(0.0f, explainer, (TextObject)null);
                explainedNumber.Add(SKILLLEVEL, _LevelText);
                explainedNumber.Add(attributeValue * Reworked_SkillsSubModule.__ATTR_VALUE, attributeName);
                explainedNumber.Add(focusValue * Reworked_SkillsSubModule.__FOCUS_VALUE, Patch3.desription);
                explainedNumber.LimitMin(0.0f);
                __result = (int)explainedNumber.ResultNumber;
                return false;
            }
            __result = (int)Reworked_SkillsSubModule.__LEANRINGLIMIT;
            return false;
        }
    }

    [HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "CalculateLearningRate", typeof(Hero), typeof(SkillObject), typeof(StatExplainer))]
    public class Patch5 {
        static bool Prefix(DefaultCharacterDevelopmentModel __instance, Hero hero, SkillObject skill, StatExplainer explainer, ref float __result) {
            __result = Reworked_SkillsSubModule.__LEANRINGLIMIT;
            return false; // make sure you only skip if really necessary
        }
    }

    [HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "CalculateLearningRate",
        typeof(int), typeof(int), typeof(int), typeof(int), typeof(TextObject), typeof(StatExplainer))]
    class Patch6 {
        private static TextObject _LevelText = new TextObject("{=RSaFRMLV}From Level", (Dictionary<string, TextObject>)null);
        private static TextObject _NormalizedPenaltyText = new TextObject("{=RSaNRLPT}Normalized Penalty", (Dictionary<string, TextObject>)null);
        private static TextObject _FixedRateText = new TextObject("{=RSaFXDRT}Fixed Rate", (Dictionary<string, TextObject>)null);
        private static TextObject _skillFocusText = new TextObject("{=MRktqZwu}Skill Focus", (Dictionary<string, TextObject>)null);
        private static TextObject _overLimitText = new TextObject("{=bcA7ZuyO}Learning Limit Exceeded", (Dictionary<string, TextObject>)null);

        static bool Prefix(DefaultCharacterDevelopmentModel __instance,
            int attributeValue, int focusValue, int skillValue, int characterLevel, TextObject attributeName, StatExplainer explainer, ref float __result) {
            if (attributeValue == 11) {
                skillValue -= (int)(attributeValue * Reworked_SkillsSubModule.__ATTR_VALUE + focusValue * Reworked_SkillsSubModule.__FOCUS_VALUE);
            }
            ExplainedNumber explainedNumber = new ExplainedNumber(0, explainer, (TextObject)null);
            float BaseByLevel = (float)(20.0 / (10.0 + (double)characterLevel));
            float Attribute = 0.4f * (float)10;
            float Focus = (float)5 * 1f;
            if (Reworked_SkillsSubModule.__DEFAULTLEARNING) {
                explainedNumber.Add(BaseByLevel, _LevelText);
                explainedNumber.Add(Attribute, attributeName);
                explainedNumber.Add(Focus, _skillFocusText);
            } else if (Reworked_SkillsSubModule.__DEFAULTNORMALIZEDLEARNING) {
                float penalty = Math.Max(0, Reworked_SkillsSubModule.__SKILLPNLT - skillValue) / Reworked_SkillsSubModule.__SKILLPNLTDV;
                explainedNumber.Add(BaseByLevel, _LevelText);
                explainedNumber.Add(Attribute, attributeName);
                explainedNumber.Add(Focus, _skillFocusText);
                explainedNumber.Add(-penalty, _NormalizedPenaltyText);
            } else if (Reworked_SkillsSubModule.__FIXEDRATE) {
                explainedNumber.Add(Reworked_SkillsSubModule.__FIXEDLEARNINGRATE, _FixedRateText);
            }
            int learningLimit = Reworked_SkillsSubModule.__LEANRINGLIMIT;// __instance.CalculateLearningLimit(10, 5, (TextObject)null, (StatExplainer)null);
            if (skillValue > learningLimit) {
                int num = skillValue - learningLimit;
                explainedNumber.AddFactor((float)(-1.0 - 0.100000001490116 * (double)num), _overLimitText);
            }
            explainedNumber.LimitMin(0.0f);
            __result = explainedNumber.ResultNumber;
            return false; // make sure you only skip if really necessary
        }
    }

    [HarmonyPatch(typeof(SkillVM), "RefreshValues")]
    class Patch7 {
        private static TextObject _learningLimitStr = new TextObject("{=RWaLRNLM}Effective Skill", (Dictionary<string, TextObject>)null);
        private static TextObject _learningRateStr = new TextObject("{=RWaLRNRT}Learning Rate", (Dictionary<string, TextObject>)null);
        static void Postfix(SkillVM __instance, ref int ____fullLearningRateLevel, CharacterVM ____developerVM) {
            int attr = ____developerVM.GetCurrentAttributePoint(__instance.Skill.CharacterAttributesEnum);
            ____fullLearningRateLevel = (int)(__instance.Level + __instance.CurrentFocusLevel * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
            TextObject attrname = CharacterAttributes.GetCharacterAttribute(__instance.Skill.CharacterAttributeEnum).Name;

            __instance.LearningRateTooltip = new BasicTooltipViewModel(() => GetLearningRateTooltip(attr, __instance.CurrentFocusLevel, __instance.Level, ____developerVM.Hero.CharacterObject.Level, attrname));
            __instance.LearningLimitTooltip = new BasicTooltipViewModel(() => {
                Patch4.SKILLLEVEL = __instance.Level;
                return GetLearningLimitTooltip(attr, __instance.CurrentFocusLevel, attrname);
            });
        }
        public static List<TooltipProperty> GetLearningLimitTooltip(int attributeValue, int focusValue, TextObject attributeName) {
            StatExplainer statExplainer = new StatExplainer();
            int learningLimit = Campaign.Current.Models.CharacterDevelopmentModel.CalculateLearningLimit(attributeValue, focusValue, attributeName, statExplainer);
            var ret = (List<TooltipProperty>)Reworked_SkillsSubModule.GetTooltipForAccumulatingPropertyWithResult.Invoke(null, new object[] { _learningLimitStr.ToString(), (float)learningLimit, statExplainer });
            return ret;
        }
        public static List<TooltipProperty> GetLearningRateTooltip(int attributeValue, int focusValue, int skillValue, int characterLevel, TextObject attributeName) {
            StatExplainer statExplainer = new StatExplainer();
            float learningRate = Campaign.Current.Models.CharacterDevelopmentModel.CalculateLearningRate(attributeValue, focusValue, skillValue, characterLevel, attributeName, statExplainer);
            var ret = (List<TooltipProperty>)Reworked_SkillsSubModule.GetTooltipForAccumulatingPropertyWithResult.Invoke(null, new object[] { _learningRateStr.ToString(), learningRate, statExplainer });
            return ret;
        }

        [HarmonyPatch(typeof(SkillVM), "RefreshWithCurrentValues")]
        class Patch8 {
            static void Postfix(SkillVM __instance, ref int ____fullLearningRateLevel, CharacterVM ____developerVM) {
                int attr = ____developerVM.GetCurrentAttributePoint(__instance.Skill.CharacterAttributeEnum);
                ____fullLearningRateLevel = (int)(__instance.Level + __instance.CurrentFocusLevel * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
                __instance.OnPropertyChanged(nameof(__instance.FullLearningRateLevel));

                __instance.SkillEffects.Clear();
                int skillValue = (int)(__instance.Level + __instance.CurrentFocusLevel * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
                foreach (SkillEffect effect in DefaultSkillEffects.GetAllSkillEffects().Where<SkillEffect>((Func<SkillEffect, bool>)(x => ((IEnumerable<SkillObject>)x.EffectedSkills).Contains<SkillObject>(__instance.Skill))))
                    __instance.SkillEffects.Add(new BindingListStringItem(CampaignUIHelper.GetSkillEffectText(effect, skillValue)));

            }
        }
    }

    //[HarmonyPatch(typeof(CampaignUIHelper), "GetLearningLimitTooltip")]
    //class Patch9 {
    //    private static readonly TextObject _learningLimitStr2 = new TextObject("{=RWaEFFSK}Effective Skill", (Dictionary<string, TextObject>)null);
    //    static bool Prefix(int attributeValue, int focusValue, TextObject attributeName, ref List<TooltipProperty> __result) {
    //        StatExplainer statExplainer = new StatExplainer();
    //        int learningLimit = Campaign.Current.Models.CharacterDevelopmentModel.CalculateLearningLimit(attributeValue, focusValue, attributeName, statExplainer);
    //        __result = (List<TooltipProperty>)Reworked_SkillsSubModule.GetTooltipForAccumulatingPropertyWithResult.Invoke(null, new object[] { _learningLimitStr2.ToString(), (float)learningLimit, statExplainer });
    //        return false;
    //    }
    //}
}
