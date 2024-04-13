using Content.Shared.Chemistry.Reagent;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server._WL.Chemistry.ReagentEffectConditions
{
    public sealed partial class ComponentsCondition : ReagentEffectCondition
    {
        [DataField("whitelist")]
        public EntityWhitelist? Whitelist = null;

        [DataField("blacklist")]
        public EntityWhitelist? Blacklist = null;

        public override bool Condition(ReagentEffectArgs args)
        {
            if (Blacklist != null)
                if (Blacklist.IsValid(args.SolutionEntity))
                    return false;

            if (Whitelist != null)
                if (!Whitelist.IsValid(args.SolutionEntity))
                    return false;

            return true;
        }

        public override string GuidebookExplanation(IPrototypeManager prototype)
        {
            return Loc.GetString("reagent-effect-condition-guidebook-components",
                ("comp", Whitelist?.Components == null ? 0 : string.Join(", ", Whitelist?.Components ?? [""])),
                ("black", Blacklist?.Components == null ? 0 : string.Join(", ", Blacklist?.Components ?? [""])),
                ("null", ""));
        }
    }
}
