construction-presenter-to-node-to-node = Чтобы сделать это, тебе нужно:

construction-examine-status-effect-should-have = У цели должно быть { $effect }.
construction-examine-status-effect-should-not-have = У цели не должно быть { $effect }.
construction-step-condition-status-effect-should-have = У цели должно быть { $effect }.
construction-step-condition-status-effect-should-not-have = У цели не должно быть { $effect }.

construction-examine-heart-damage-range = { $min ->
    [2147483648] Цель должна иметь минимум { NATURALFIXED($min, 2) } повреждений сердца.
    *[other] { $max ->
                [0] Цель должна иметь не более { NATURALFIXED($max, 2) } повреждений сердца.
                *[other] Сердце цели должно быть повреждено между { NATURALFIXED($min, 2) } и { NATURALFIXED($max, 2) }.
             }
}

construction-step-heart-damage-range = { $min ->
    [2147483648] Цель должна иметь минимум { NATURALFIXED($min, 2) } повреждений сердца.
    *[other] { $max ->
                [0] Цель должна иметь не более { NATURALFIXED($max, 2) } повреждений сердца.
                *[other] Сердце цели должно быть повреждено между { NATURALFIXED($min, 2) } и { NATURALFIXED($max, 2) }.
             }
}

construction-examine-lung-damage-range = { $min ->
    [2147483648] Цель должна иметь минимум { NATURALFIXED($min, 2) } повреждений лёгких.
    *[other] { $max ->
                [0] Цель должна иметь не более { NATURALFIXED($max, 2) } повреждений лёгких.
                *[other] Лёгкие цели должны быть повреждены между { NATURALFIXED($min, 2) } и { NATURALFIXED($max, 2) }.
             }
}

construction-step-lung-damage-range = { $min ->
    [2147483648] Цель должна иметь минимум { NATURALFIXED($min, 2) } повреждений лёгких.
    *[other] { $max ->
                [0] Цель должна иметь не более { NATURALFIXED($max, 2) } повреждений лёгких.
                *[other] Лёгкие цели должны быть повреждены между { NATURALFIXED($min, 2) } и { NATURALFIXED($max, 2) }.
             }
}

construction-component-to-perform-header = Чтобы выполнить { $targetName }...
