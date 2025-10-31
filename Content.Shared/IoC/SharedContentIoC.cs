using Content.Shared.Humanoid.Markings;
using Content.Shared.Localizations;
using Content.Shared.Tag; /// EE
using Content.Shared.Whitelist; /// EE

namespace Content.Shared.IoC
{
    public static class SharedContentIoC
    {
        public static void Register(IDependencyCollection deps)
        {
            IoCManager.Register<MarkingManager, MarkingManager>();
            IoCManager.Register<ContentLocalizationManager, ContentLocalizationManager>();
            // Goob: to port EE Interaction Verbs. I hate this.
            IoCManager.Register<EntityWhitelistSystem>();
            IoCManager.Register<TagSystem>();
            // End
        }
    }
}
