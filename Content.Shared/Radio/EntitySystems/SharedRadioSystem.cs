using JetBrains.Annotations;
using Content.Shared._WL.Languages; // WL-Changes
using Robust.Shared.Prototypes;

namespace Content.Shared.Radio.EntitySystems;

/// <summary>
/// This system handles intrinsic radios and the general process of converting radio messages into chat messages.
/// </summary>
public abstract partial class SharedRadioSystem : EntitySystem
{
    /// <summary>
    /// Send radio message to all active radio listeners.
    /// </summary>
    [PublicAPI]
    public void SendRadioMessage(EntityUid messageSource,
        string message,
        ProtoId<RadioChannelPrototype> channel,
        EntityUid radioSource,
        bool escapeMarkup = true,
        LanguageMessageContext? languageContext = null) // WL-Changes
    {
        SendRadioMessage(messageSource, message, ProtoMan.Index(channel), radioSource, escapeMarkup: escapeMarkup, languageContext: languageContext); // WL-Changes
    }

    /// <summary>
    /// Sends a radio message to all active radio listeners.
    /// </summary>
    /// <param name="messageSource">Entity that spoke the message.</param>
    /// <param name="message">Message to send over the radio.</param>
    /// <param name="channel">Radio channel to send the message on.</param>
    /// <param name="radioSource">Entity transmitting the message.</param>
    /// <param name="escapeMarkup">Whether markup in the message should be escaped.</param>
    /// <param name="languageContext">Shared comprehension results for the original utterance.</param>
    [PublicAPI]
    public virtual void SendRadioMessage(EntityUid messageSource,
        string message,
        RadioChannelPrototype channel,
        EntityUid radioSource,
        bool escapeMarkup = true,
        LanguageMessageContext? languageContext = null) // WL-Changes
    {

    }
}
