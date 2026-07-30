// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._WL.Tajaran;

[RegisterComponent]
public sealed partial class CoughingUpHairballComponent : Component
{
    [DataField]
    public float Accumulator;

    [DataField]
    public TimeSpan CoughUpTime = TimeSpan.FromSeconds(2.15);
}
