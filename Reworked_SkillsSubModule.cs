using System;
using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;
using System.Xml;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;

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
                                    __FIXEDRATE = false;
                                    __DEFAULTNORMALIZEDLEARNING = false;
                                    __DEFAULTLEARNING = true;
                                    break;
                                case "defaultnormalized":
                                    __DEFAULTLEARNING = false;
                                    __FIXEDRATE = false;
                                    __DEFAULTNORMALIZEDLEARNING = true;
                                    break;
                                case "fixed":
                                    __DEFAULTLEARNING = false;
                                    __DEFAULTNORMALIZEDLEARNING = false;
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
        static bool Prefix(CharacterObject __instance, MBCharacterSkills ___DefaultCharacterSkills, ref int __result, SkillObject skill) {
            int focus = 0;
            int attr = 0;
            if (__instance.HeroObject != null) {
                focus = __instance.HeroObject.HeroDeveloper.GetFocus(skill);
                attr = __instance.HeroObject.GetAttributeValue(skill.CharacterAttribute);
            }
            if (__instance.IsHero && __instance.HeroObject != null) {
                __result = (int)(__instance.HeroObject.GetSkillValue(skill) + focus * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
            } else {
                __result = (int)(___DefaultCharacterSkills.Skills.GetPropertyValue(skill) + focus * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(BasicCharacterObject), "GetSkillValue")]
    class Patch2 {
        static bool Prefix(CharacterObject __instance, MBCharacterSkills ___DefaultCharacterSkills, ref int __result, SkillObject skill) {
            int focus = __instance.HeroObject.HeroDeveloper.GetFocus(skill);
            int attr = __instance.HeroObject.GetAttributeValue(skill.CharacterAttribute);
            __result = (int)(___DefaultCharacterSkills.Skills.GetPropertyValue(skill) + focus * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
            return false;
        }
    }

    [HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "CalculateLearningLimit", typeof(int), typeof(int), typeof(TextObject), typeof(bool))]
    public class Patch3 {
        private static readonly TextObject _Learning_Limit_Text = new TextObject("{=MRktqZwo}Learning Limit");

        static bool Prefix(DefaultCharacterDevelopmentModel __instance, int attributeValue, int focusValue, TextObject attributeName, bool includeDescriptions, ref ExplainedNumber __result) {
            ExplainedNumber learningLimit = new ExplainedNumber(includeDescriptions: includeDescriptions);
            //ORIGINAL
            // learningLimit.Add((float) ((attributeValue - 1) * 10), attributeName);
            // learningLimit.Add((float) (focusValue * 30), _skillFocusText);
            // learningLimit.LimitMin(0.0f);
            learningLimit.Add((float)(Reworked_SkillsSubModule.__LEANRINGLIMIT), _Learning_Limit_Text);
            __result = learningLimit;

            return false;
        }
    }


    // public override float CalculateLearningRate(Hero hero, SkillObject skill);
    // public override ExplainedNumber CalculateLearningRate(
    //     int attributeValue,
    //     int focusValue,
    //     int skillValue,
    //     int characterLevel,
    //     TextObject attributeName,
    //     bool includeDescriptions = false);

    [HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "CalculateLearningRate",
        typeof(int), typeof(int), typeof(int), typeof(int), typeof(TextObject), typeof(bool))]
    class Patch6 {
        public static readonly TextObject _LevelText = new TextObject("{=RSaFRMLV}From Level");
        public static readonly TextObject _BaseText = new TextObject("{=RSaFRMLF}Base Level");
        public static readonly TextObject _NormalizedPenaltyText = new TextObject("{=RSaNRLPT}Normalized Penalty");
        public static readonly TextObject _FixedRateText = new TextObject("{=RSaFXDRT}Fixed Rate");
        public static readonly TextObject _skillFocusText = new TextObject("{=MRktqZwu}Skill Focus");
        public static readonly TextObject _overLimitText = new TextObject("{=bcA7ZuyO}Learning Limit Exceeded");

        static bool Prefix(DefaultCharacterDevelopmentModel __instance,
            int attributeValue, int focusValue, int skillValue, int characterLevel, TextObject attributeName, bool includeDescriptions, ref ExplainedNumber __result) {
            ExplainedNumber learningRate = new ExplainedNumber(1.25f, includeDescriptions);
            //ORIGINAL
            // learningRate.AddFactor(0.4f * (float) attributeValue, attributeName);
            // learningRate.AddFactor((float) focusValue * 1f, DefaultCharacterDevelopmentModel._skillFocusText);
            // int num1 = MathF.Round(this.CalculateLearningLimit(attributeValue, focusValue, (TextObject) null, false).ResultNumber);
            // if (skillValue > num1)
            // {
            //     int num2 = skillValue - num1;
            //     learningRate.AddFactor((float) (-1.0 - 0.10000000149011612 * (double) num2), DefaultCharacterDevelopmentModel._overLimitText);
            // }
            // learningRate.LimitMin(0.0f);
            // return learningRate;

            if (attributeValue == 11) {
                skillValue -= (int)(attributeValue * Reworked_SkillsSubModule.__ATTR_VALUE + focusValue * Reworked_SkillsSubModule.__FOCUS_VALUE);
            }
            float BaseByLevel = (float)(20.0 / (10.0 + (double)characterLevel));
            float Attribute = attributeValue * 0.4f;
            float Focus = focusValue * 1f;
            if (Reworked_SkillsSubModule.__DEFAULTLEARNING) {
                learningRate.Add(BaseByLevel, _LevelText);
                learningRate.Add(Attribute, attributeName);
                learningRate.Add(Focus, _skillFocusText);
            } else if (Reworked_SkillsSubModule.__DEFAULTNORMALIZEDLEARNING) {
                float penalty = Math.Max(1, Math.Max(0, Reworked_SkillsSubModule.__SKILLPNLT - skillValue) / Reworked_SkillsSubModule.__SKILLPNLTDV);
                learningRate.Add(BaseByLevel, _LevelText);
                learningRate.Add(Attribute, attributeName);
                learningRate.Add(Focus, _skillFocusText);
                if (penalty > 1) {
                    learningRate.AddFactor(1 / penalty, _NormalizedPenaltyText);
                }
            } else if (Reworked_SkillsSubModule.__FIXEDRATE) {
                learningRate.Add(Reworked_SkillsSubModule.__FIXEDLEARNINGRATE, _FixedRateText);
            }
            int learningLimit = Reworked_SkillsSubModule.__LEANRINGLIMIT; // __instance.CalculateLearningLimit(10, 5, (TextObject)null, (StatExplainer)null);
            if (skillValue > learningLimit) {
                int num = skillValue - learningLimit;
                learningRate.AddFactor((float)(-1.0 - 0.100000001490116 * (double)num), _overLimitText);
            }
            learningRate.LimitMin(0.0f);
            __result = learningRate;
            return false; // make sure you only skip if really necessary
        }
    }

    [HarmonyPatch(typeof(SkillVM), "RefreshValues")]
    class Patch7 {
        private static TextObject _learningLimitStr = new TextObject("{=RWaLRNLM}Effective Skill");
        private static TextObject _learningRateStr = new TextObject("{=RWaLRNRT}Learning Rate");

        static void Postfix(SkillVM __instance, ref int ____fullLearningRateLevel, ref TextObject ____boundAttributeName, CharacterVM ____developerVM) {
            int attr = ____developerVM.GetCurrentAttributePoint(__instance.Skill.CharacterAttribute);
            ____fullLearningRateLevel = (int)(__instance.Level + __instance.CurrentFocusLevel * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);

            TextObject o = ____boundAttributeName;
            // __instance.LearningRateTooltip = new BasicTooltipViewModel(() => GetLearningRateTooltip(attr, __instance.CurrentFocusLevel, __instance.Level, ____developerVM.Hero.CharacterObject.Level, o));
            int i = ____fullLearningRateLevel;
            __instance.LearningLimitTooltip = new BasicTooltipViewModel(() => GetLearningLimitTooltip(attr, __instance.CurrentFocusLevel, o, attr, __instance, i));
        }

        public static List<TooltipProperty> GetLearningLimitTooltip(int attributeValue, int focusValue, TextObject attributeName, int attr, SkillVM __instance, int SkillLevel) {
            ExplainedNumber learningLimit = new ExplainedNumber(0, true);
            learningLimit.Add(__instance.Level, Patch6._BaseText);
            learningLimit.Add(attr * Reworked_SkillsSubModule.__ATTR_VALUE, attributeName);
            learningLimit.Add(__instance.CurrentFocusLevel * Reworked_SkillsSubModule.__FOCUS_VALUE, Patch6._skillFocusText);
            return CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult(_learningLimitStr.ToString(), SkillLevel, ref learningLimit);
        }

        // public static List<TooltipProperty> GetLearningRateTooltip(int attributeValue, int focusValue, int skillValue, int characterLevel, TextObject attributeName) {
        //     ExplainedNumber learningLimit = Campaign.Current.Models.CharacterDevelopmentModel.CalculateLearningLimit(attributeValue, focusValue, attributeName, true);
        //     return CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult(_learningRateStr.ToString(), learningLimit.ResultNumber, ref learningLimit);
        // }

        [HarmonyPatch(typeof(SkillVM), "RefreshWithCurrentValues")]
        class Patch8 {
            // float resultNumber = Campaign.Current.Models.CharacterDevelopmentModel.CalculateLearningRate(this._boundAttributeCurrentValue, this.CurrentFocusLevel, this.Level, this._heroLevel, this._boundAttributeName).ResultNumber;
            // GameTexts.SetVariable("COUNT", resultNumber.ToString("0.00"));
            // this.CurrentLearningRateText = GameTexts.FindText("str_learning_rate_COUNT").ToString();
            //     this.CanLearnSkill = (double) resultNumber > 0.0;
            // this.LearningRate = resultNumber;
            // this.FullLearningRateLevel = MathF.Round(Campaign.Current.Models.CharacterDevelopmentModel.CalculateLearningLimit(this._boundAttributeCurrentValue, this.CurrentFocusLevel, this._boundAttributeName).ResultNumber);
            // int withCurrentFocus = this._developerVM.GetRequiredFocusPointsToAddFocusWithCurrentFocus(this.Skill);
            // GameTexts.SetVariable("COSTAMOUNT", withCurrentFocus);
            // this.FocusCostText = withCurrentFocus.ToString();
            // GameTexts.SetVariable("COUNT", withCurrentFocus);
            // GameTexts.SetVariable("RIGHT", "");
            // GameTexts.SetVariable("LEFT", GameTexts.FindText("str_cost_COUNT"));
            // MBTextManager.SetTextVariable("FOCUS_ICON", GameTexts.FindText("str_html_focus_icon"), false);
            // this.NextLevelCostText = GameTexts.FindText("str_sf_text_with_focus_icon").ToString();
            //     this.RefreshCanAddFocus();


            static void Postfix(SkillVM __instance, ref int ____fullLearningRateLevel, CharacterVM ____developerVM) {
                int attr = ____developerVM.GetCurrentAttributePoint(__instance.Skill.CharacterAttribute);
                ____fullLearningRateLevel = (int)(__instance.Level + __instance.CurrentFocusLevel * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
                __instance.OnPropertyChanged(nameof(__instance.FullLearningRateLevel));

                __instance.SkillEffects.Clear();
                int skillValue = (int)(__instance.Level + __instance.CurrentFocusLevel * Reworked_SkillsSubModule.__FOCUS_VALUE + attr * Reworked_SkillsSubModule.__ATTR_VALUE);
                foreach (SkillEffect effect in SkillEffect.All.Where(x => x.EffectedSkills.Contains(__instance.Skill)))
                    __instance.SkillEffects.Add(new BindingListStringItem(CampaignUIHelper.GetSkillEffectText(effect, skillValue)));
            }
        }
    }

    // public static List<TooltipProperty> GetLearningLimitTooltip(
    //     int attributeValue,
    //     int focusValue,
    //     TextObject attributeName);

    // [HarmonyPatch(typeof(CampaignUIHelper), "GetLearningLimitTooltip")]
    // class Patch9 {
    //     private static readonly TextObject _learningLimitStr2 = new TextObject("{=RWaEFFSK}Effective Skill");
    //     static bool Prefix(int attributeValue, int focusValue, TextObject attributeName, ref List<TooltipProperty> __result) {
    //         ExplainedNumber learningLimit = Campaign.Current.Models.CharacterDevelopmentModel.GetSkillValue(attributeValue, focusValue, attributeName, true);
    //         __result= CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult(_learningLimitStr2.ToString(), learningLimit.ResultNumber, ref learningLimit);
    //
    //         // StatExplainer statExplainer = new StatExplainer();
    //         // int learningLimit = Campaign.Current.Models.CharacterDevelopmentModel.CalculateLearningLimit(attributeValue, focusValue, attributeName, statExplainer);
    //        //__result = (List<TooltipProperty>)Reworked_SkillsSubModule.GetTooltipForAccumulatingPropertyWithResult.Invoke(null, new object[] { _learningLimitStr2.ToString(), (float)learningLimit, statExplainer });
    //         return false;
    //     }
    // }
}