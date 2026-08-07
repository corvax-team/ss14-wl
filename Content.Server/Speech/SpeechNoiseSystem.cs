using Content.Shared.Chat;
using Content.Shared._WL.Barks; // WL-Changes
using Content.Shared.Speech;
using Robust.Shared.Audio;
using Robust.Shared.Player; // WL-Changes
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Speech
{
    public sealed partial class SpeechSoundSystem : EntitySystem
    {
        [Dependency] private IGameTiming _gameTiming = default!;
        [Dependency] private IRobustRandom _random = default!;
        // WL-Changes: Speech sounds are sent to clients so barks can suppress them locally.

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SpeechComponent, EntitySpokeEvent>(OnEntitySpoke);
        }

        public SoundSpecifier? GetSpeechSound(Entity<SpeechComponent> ent, string message)
        {
            if (ent.Comp.SpeechSounds == null)
                return null;

            // Play speech sound
            SoundSpecifier? contextSound;
            var prototype = ProtoMan.Index<SpeechSoundsPrototype>(ent.Comp.SpeechSounds);

            // Different sounds for ask/exclaim based on last character
            contextSound = message[^1] switch
            {
                '?' => prototype.AskSound,
                '!' => prototype.ExclaimSound,
                _ => prototype.SaySound
            };

            // Use exclaim sound if most characters are uppercase.
            int uppercaseCount = 0;
            for (int i = 0; i < message.Length; i++)
            {
                if (char.IsUpper(message[i]))
                    uppercaseCount++;
            }
            if (uppercaseCount > (message.Length / 2))
            {
                contextSound = prototype.ExclaimSound;
            }

            var scale = (float) _random.NextGaussian(1, prototype.Variation);
            contextSound.Params = ent.Comp.AudioParams.WithPitchScale(scale);
            return contextSound;
        }

        private void OnEntitySpoke(EntityUid uid, SpeechComponent component, EntitySpokeEvent args)
        {
            if (component.SpeechSounds == null)
                return;

            var currentTime = _gameTiming.CurTime;
            var cooldown = TimeSpan.FromSeconds(component.SoundCooldownTime);

            // Ensure more than the cooldown time has passed since last speaking
            if (currentTime - component.LastTimeSoundPlayed < cooldown)
                return;

            var sound = GetSpeechSound((uid, component), args.Message);
            // WL-Changes-Start: Client-side speech sound playback
            if (sound == null)
                return;

            component.LastTimeSoundPlayed = currentTime;
            RaiseNetworkEvent(
                new PlaySpeechSoundEvent(GetNetEntity(uid), sound),
                Filter.Pvs(uid));
            // WL-Changes-End
        }
    }
}
