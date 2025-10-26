reagent-guidebook-status-effect = Вызывает { $effect } при метаболизации{ $conditionCount ->
        [0] .
        *[other] {" "}когда { $conditions }.
    }

reagent-effect-guidebook-status-effect-remove = { $chance ->
        [1] Убирает { LOC($key) }
   *[other] убирает { LOC($key) }
}

reagent-effect-guidebook-modify-brain-damage-heals = { $chance ->
        [1] Излечивает { $amount } повреждений мозга
   *[other] излечивает { $amount } повреждений мозга
}
reagent-effect-guidebook-modify-brain-damage-deals = { $chance ->
        [1] Наносит { $amount } повреждений мозга
   *[other] наносит { $amount } повреждений мозга
}
reagent-effect-guidebook-modify-heart-damage-heals = { $chance ->
        [1] Излечивает { $amount } повреждений сердца
   *[other] излечивает { $amount } повреждений сердца
}
reagent-effect-guidebook-modify-heart-damage-deals = { $chance ->
        [1] Наносит { $amount } повреждений сердца
   *[other] наносит { $amount } повреждений сердца
}
reagent-effect-guidebook-modify-lung-damage-heals = { $chance ->
        [1] Излечивает { $amount } повреждений лёгких
   *[other] излечивает { $amount } повреждений лёгких
}
reagent-effect-guidebook-modify-lung-damage-deals = { $chance ->
        [1] Наносит { $amount } повреждений лёгких
   *[other] Наносит { $amount } повреждений лёгких
}
reagent-effect-guidebook-clamp-wounds = { $probability ->
        [1] Останавливает кровотечение с шансом { NATURALPERCENT($chance, 2) } на рану
   *[other] останавливает кровотечение с шансом { NATURALPERCENT($chance, 2) } на рану
}
reagent-effect-condition-guidebook-heart-damage = { $min ->
    [2147483648] если есть как минимум {NATURALFIXED($min, 2)} повреждений сердца
    *[other] { $max ->
                [0] если есть не более {NATURALFIXED($max, 2)} повреждений сердца
                *[other] если повреждения сердца между {NATURALFIXED($min, 2)} и {NATURALFIXED($max, 2)}
             }
}
reagent-effect-condition-guidebook-lung-damage = { $min ->
    [2147483648] если есть как минимум {NATURALFIXED($min, 2)} повреждений лёгких
    *[other] { $max ->
                [0] если есть не более {NATURALFIXED($max, 2)} повреждений лёгких
                *[other] если повреждения лёгких между {NATURALFIXED($min, 2)} и {NATURALFIXED($max, 2)}
             }
}
reagent-effect-condition-guidebook-brain-damage = { $min ->
    [2147483648] если есть как минимум {NATURALFIXED($min, 2)} повреждений мозга
    *[other] { $max ->
                [0] если есть не более {NATURALFIXED($max, 2)} повреждений мозга
                *[other] если повреждения мозга между {NATURALFIXED($min, 2)} и {NATURALFIXED($max, 2)}
             }
}
reagent-effect-condition-guidebook-total-group-damage = { $min ->
    [2147483648] если есть как минимум {NATURALFIXED($min, 2)} урона
    *[other] { $max ->
                [0] если есть не более {NATURALFIXED($max, 2)} урона
                *[other] если урон между {NATURALFIXED($min, 2)} и {NATURALFIXED($max, 2)}
             }
}
reagent-effect-guidebook-modify-brain-oxygen-heals = { $chance ->
        [1] Восполняет { $amount } оксигенации мозга
   *[other] восполняет { $amount } оксигенации мозга
}
reagent-effect-guidebook-modify-brain-oxygen-deals = { $chance ->
        [1] Уменьшает оксигенацию мозга на { $amount }
   *[other] меньшает оксигенацию мозга на { $amount }
}

reagent-effect-guidebook-start-heart = { $chance ->
        [1] Перезапускает сердце цели
   *[other] перезапускает сердце цели
}
reagent-effect-guidebook-zombify = { $chance ->
        [1] Зомбифицирует цель
   *[other] зомбифицирует цель
}

reagent-effect-condition-guidebook-total-dosage-threshold =
    { $min ->
        [2147483648] общая доза {$reagent} не менее {NATURALFIXED($min, 2)}u
        *[other] { $max ->
                    [0] общая доза {$reagent} не более {NATURALFIXED($max, 2)}u
                    *[other] общая доза {$reagent} между {NATURALFIXED($min, 2)}u и {NATURALFIXED($max, 2)}u
                 }
    }

reagent-effect-condition-guidebook-metabolite-threshold =
    { $min ->
        [2147483648] есть по крайней мере {NATURALFIXED($min, 2)}u метаболитов {$reagent}
        *[other] { $max ->
                    [0] есть не более {NATURALFIXED($max, 2)}u метаболитов {$reagent}
                    *[other] метаболиты {$reagent} находяться между {NATURALFIXED($min, 2)}u и {NATURALFIXED($max, 2)}u
                 }
    }

reagent-effect-condition-guidebook-is-zombie-immune =
    цель { $invert ->
                    [true] не имеет иммунита к зомби вирусу
                   *[false] имеет иммунитет к зомби вирусу
                }

reagent-effect-condition-guidebook-is-zombie =
    цель { $invert ->
                    [true] не зомбу
                   *[false] зомбу
                }

reagent-effect-condition-guidebook-this-metabolite = эти реагенты

reagent-effect-guidebook-adjust-reagent-gaussian =
    { $chance ->
        [1] { $deltasign ->
                [1] Обычно добавляет
                *[-1] Обычно удаляет
            }
        *[other]
            { $deltasign ->
                [1] обычно добавляет
                *[-1] обычно удаляет
            }
    } {NATURALFIXED($mu, 2)}u {$reagent} от { $deltasign ->
        [1] до
        *[-1] из
    } из смеси, при этом обычная сумма варьируется около {NATURALFIXED($sigma, 2)}u
