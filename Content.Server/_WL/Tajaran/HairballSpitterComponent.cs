// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._WL.Tajaran;

[RegisterComponent]
public sealed partial class HairballSpitterComponent : Component
{
    [DataField]
    public TimeSpan CoughUpTime = TimeSpan.FromSeconds(2.15);

    [DataField]
    public EntProtoId HairballPrototype = "TajaranHairball";

    [DataField]
    public EntProtoId HairballActionPrototype = "ActionTajaranHairball";

    [DataField]
    public EntityUid? HairballActionEntity;
}
