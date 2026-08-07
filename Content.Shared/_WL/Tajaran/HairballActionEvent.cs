// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._WL.Tajaran;

public sealed partial class HairballActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class HairballDoAfterEvent : SimpleDoAfterEvent;
