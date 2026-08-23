using System.Linq;
using Content.Client.Lobby.UI.Roles;
using Content.Client.Stylesheets;
using Content.Shared._WL.Paper;
using Content.Shared.Traits;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{

    /// <summary>
    /// Refreshes traits selector
    /// </summary>
    public void RefreshTraits()
    {
        TraitsList.RemoveAllChildren();

        var traits = _prototypeManager.EnumeratePrototypes<TraitPrototype>().OrderBy(t => Loc.GetString(t.Name)).ToList();
        // TabContainer.SetTabTitle(3, Loc.GetString("humanoid-profile-editor-traits-tab")); // Corvax-TTS-Edit

        if (traits.Count < 1)
        {
            TraitsList.AddChild(new Label
            {
                Text = Loc.GetString("humanoid-profile-editor-no-traits"),
                FontColorOverride = Color.Gray,
            });
            return;
        }

        // Setup model
        Dictionary<string, List<string>> traitGroups = new();
        List<string> defaultTraits = new();
        traitGroups.Add(TraitCategoryPrototype.Default, defaultTraits);

        foreach (var trait in traits)
        {
            if (trait.Category == null)
            {
                defaultTraits.Add(trait.ID);
                continue;
            }

            if (!_prototypeManager.HasIndex(trait.Category))
                continue;

            var group = traitGroups.GetOrNew(trait.Category);
            group.Add(trait.ID);
        }

        // Create UI view from model
        foreach (var (categoryId, categoryTraits) in traitGroups)
        {
            TraitCategoryPrototype? category = null;

            if (categoryId != TraitCategoryPrototype.Default)
            {
                category = _prototypeManager.Index<TraitCategoryPrototype>(categoryId);
                // Label
                TraitsList.AddChild(new Label
                {
                    Text = Loc.GetString(category.Name),
                    Margin = new Thickness(0, 10, 0, 0),
                    StyleClasses = { StyleClass.LabelHeading },
                });
            }

            List<TraitPreferenceSelector?> selectors = new();
            var selectionCount = 0;

            foreach (var traitProto in categoryTraits)
            {
                var trait = _prototypeManager.Index<TraitPrototype>(traitProto);
                var selector = new TraitPreferenceSelector(trait);

                // WL-StructuredPaper-Start: show handwriting choices in their actual in-game font.
                if (trait.Components.TryGetComponent(
                        _entManager.ComponentFactory,
                        out PaperHandwritingComponent? handwriting))
                {
                    var (path, size) = GetHandwritingPreview(handwriting.Style);
                    selector.Checkbox.Label.FontOverride = new VectorFont(
                        _resManager.GetResource<FontResource>(path),
                        size);
                }
                // WL-StructuredPaper-End

                selector.Preference = Profile?.TraitPreferences.Contains(trait.ID) == true;
                if (selector.Preference)
                    selectionCount += trait.Cost;

                selector.PreferenceChanged += preference =>
                {
                    if (preference)
                    {
                        Profile = Profile?.WithTraitPreference(trait.ID, _prototypeManager);
                    }
                    else
                    {
                        Profile = Profile?.WithoutTraitPreference(trait.ID, _prototypeManager);
                    }

                    SetDirty();
                    RefreshTraits(); // If too many traits are selected, they will be reset to the real value.
                };
                selectors.Add(selector);
            }

            // Selection counter
            if (category is { MaxTraitPoints: >= 0 })
            {
                TraitsList.AddChild(new Label
                {
                    Text = Loc.GetString("humanoid-profile-editor-trait-count-hint", ("current", selectionCount), ("max", category.MaxTraitPoints)),
                    FontColorOverride = Color.Gray
                });
            }

            foreach (var selector in selectors)
            {
                if (selector == null)
                    continue;

                if (category is { MaxTraitPoints: >= 0, MutuallyExclusive: false } &&
                    selector.Cost + selectionCount > category.MaxTraitPoints)
                {
                    selector.Checkbox.Label.FontColorOverride = Color.Red;
                }

                TraitsList.AddChild(selector);
            }
        }
    }

    // WL-StructuredPaper-Start
    private static (ResPath Path, int Size) GetHandwritingPreview(PaperHandwritingStyle style)
    {
        return style switch
        {
            PaperHandwritingStyle.Neat => (new ResPath("/Fonts/_WL/Handwriting/BadScript/BadScript-Regular.ttf"), 15),
            PaperHandwritingStyle.Quick => (new ResPath("/Fonts/_WL/Handwriting/Caveat/Caveat.ttf"), 17),
            PaperHandwritingStyle.Formal => (new ResPath("/Fonts/_WL/Handwriting/MarckScript/MarckScript-Regular.ttf"), 16),
            PaperHandwritingStyle.Heavy => (new ResPath("/Fonts/_WL/Handwriting/Pangolin/Pangolin-Regular.ttf"), 14),
            PaperHandwritingStyle.Messy => (new ResPath("/Fonts/_WL/Handwriting/Neucha/Neucha.ttf"), 16),
            _ => (new ResPath("/Fonts/NotoSans/NotoSans-Regular.ttf"), 12),
        };
    }
    // WL-StructuredPaper-End
}
