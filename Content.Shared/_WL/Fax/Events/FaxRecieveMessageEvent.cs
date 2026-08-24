using Content.Shared.Fax.Components;

namespace Content.Shared._WL.Fax.Events
{
    public sealed partial class FaxRecieveMessageEvent(FaxPrintout msg, EntityUid? sender, Entity<FaxMachineComponent> reciever) : EntityEventArgs
    {
        public readonly FaxPrintout Message = msg;
        public readonly EntityUid? Sender = sender;
        public readonly Entity<FaxMachineComponent> Reciever = reciever;
    }
}
