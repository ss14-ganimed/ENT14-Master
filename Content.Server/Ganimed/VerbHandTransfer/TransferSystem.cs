using Content.Shared.Hands.Components;
using Content.Shared.Strip.Components;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Players;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.Ganimed.VerbHandTransfer
{
    public sealed class VerbHandTransfer : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entitymanager = default!;
        [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<HandsComponent, GetVerbsEvent<EquipmentVerb>>(AddTransferVerb);
        }

        private void AddTransferVerb(EntityUid uid, HandsComponent component, GetVerbsEvent<EquipmentVerb> args)
        {
            if (!args.CanInteract || args.Hands == null || args.Hands.ActiveHandEntity == null
                || args.Target == args.User || !FindFreeHand(component, out var freeHand))
                return;

            EquipmentVerb verb = new EquipmentVerb()
            {
                Text = Loc.GetString("action-transfer-name"),
                Act = () => TransferItemInHands(uid, args.User, freeHand)
            };

            args.Verbs.Add(verb);
        }

        private void TransferItemInHands(EntityUid target, EntityUid user, string freeHand)
        {
            if (_playerManager.TryGetSessionByEntity(user, out var session) && session is ICommonSession playerSession)
                RaiseLocalEvent<StrippingSlotButtonPressed>(target, new StrippingSlotButtonPressed(freeHand, true) { Session = playerSession });
        }

        private bool FindFreeHand(HandsComponent component, [NotNullWhen(true)] out string? freeHand)
        {
            return (freeHand = component.GetFreeHandNames().Any() ? component.GetFreeHandNames().First() : null) != null;
        }
    }
}