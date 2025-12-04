entity-guidebook-status-effect = Вызывает { $effect } при метаболизации{ $conditionCount ->
        [0] .
        *[other] {" "}когда { $conditions }.
    }

entity-effect-guidebook-status-effect-remove = { $chance ->
        [1] Убирает { LOC($key) }
   *[other] убирает { LOC($key) }
}

entity-effect-guidebook-modify-brain-damage-heals = { $chance ->
        [1] Излечивает { $amount } повреждений мозга
   *[other] излечивает { $amount } повреждений мозга
}
entity-effect-guidebook-modify-brain-damage-deals = { $chance ->
        [1] Наносит { $amount } повреждений мозга
   *[other] наносит { $amount } повреждений мозга
}
entity-effect-guidebook-modify-heart-damage-heals = { $chance ->
        [1] Излечивает { $amount } повреждений сердца
   *[other] излечивает { $amount } повреждений сердца
}
entity-effect-guidebook-modify-heart-damage-deals = { $chance ->
        [1] Наносит { $amount } повреждений сердца
   *[other] наносит { $amount } повреждений сердца
}
entity-effect-guidebook-modify-lung-damage-heals = { $chance ->
        [1] Излечивает { $amount } повреждений лёгких
   *[other] излечивает { $amount } повреждений лёгких
}
entity-effect-guidebook-modify-lung-damage-deals = { $chance ->
        [1] Наносит { $amount } повреждений лёгких
   *[other] Наносит { $amount } повреждений лёгких
}
entity-effect-guidebook-clamp-wounds = { $probability ->
        [1] Останавливает кровотечение с шансом { NATURALPERCENT($chance, 2) } на рану
   *[other] останавливает кровотечение с шансом { NATURALPERCENT($chance, 2) } на рану
}
entity-condition-guidebook-heart-damage = { $min ->
    [2147483648] если есть как минимум {NATURALFIXED($min, 2)} повреждений сердца
    *[other] { $max ->
                [0] если есть не более {NATURALFIXED($max, 2)} повреждений сердца
                *[other] если повреждения сердца между {NATURALFIXED($min, 2)} и {NATURALFIXED($max, 2)}
             }
}
entity-condition-guidebook-lung-damage = { $min ->
    [2147483648] если есть как минимум {NATURALFIXED($min, 2)} повреждений лёгких
    *[other] { $max ->
                [0] если есть не более {NATURALFIXED($max, 2)} повреждений лёгких
                *[other] если повреждения лёгких между {NATURALFIXED($min, 2)} и {NATURALFIXED($max, 2)}
             }
}
entity-condition-guidebook-brain-damage = { $min ->
    [2147483648] если есть как минимум {NATURALFIXED($min, 2)} повреждений мозга
    *[other] { $max ->
                [0] если есть не более {NATURALFIXED($max, 2)} повреждений мозга
                *[other] если повреждения мозга между {NATURALFIXED($min, 2)} и {NATURALFIXED($max, 2)}
             }
}
entity-condition-guidebook-total-group-damage = { $min ->
    [2147483648] если есть как минимум {NATURALFIXED($min, 2)} урона
    *[other] { $max ->
                [0] если есть не более {NATURALFIXED($max, 2)} урона
                *[other] если урон между {NATURALFIXED($min, 2)} и {NATURALFIXED($max, 2)}
             }
}
entity-effect-guidebook-modify-brain-oxygen-heals = { $chance ->
        [1] Восполняет { $amount } оксигенации мозга
   *[other] восполняет { $amount } оксигенации мозга
}
entity-effect-guidebook-modify-brain-oxygen-deals = { $chance ->
        [1] Уменьшает оксигенацию мозга на { $amount }
   *[other] меньшает оксигенацию мозга на { $amount }
}

entity-effect-guidebook-start-heart = { $chance ->
        [1] Перезапускает сердце цели
   *[other] перезапускает сердце цели
}
entity-effect-guidebook-zombify = { $chance ->
        [1] Зомбифицирует цель
   *[other] зомбифицирует цель
}

entity-condition-guidebook-total-dosage-threshold =
    { $min ->
        [2147483648] общая доза {$reagent} не менее {NATURALFIXED($min, 2)}u
        *[other] { $max ->
                    [0] общая доза {$reagent} не более {NATURALFIXED($max, 2)}u
                    *[other] общая доза {$reagent} между {NATURALFIXED($min, 2)}u и {NATURALFIXED($max, 2)}u
                 }
    }

entity-condition-guidebook-metabolite-threshold =
    { $min ->
        [2147483648] есть по крайней мере {NATURALFIXED($min, 2)}u метаболитов {$reagent}
        *[other] { $max ->
                    [0] есть не более {NATURALFIXED($max, 2)}u метаболитов {$reagent}
                    *[other] метаболиты {$reagent} находяться между {NATURALFIXED($min, 2)}u и {NATURALFIXED($max, 2)}u
                 }
    }

entity-condition-guidebook-is-zombie-immune =
    цель { $invert ->
                    [true] не имеет иммунита к зомби вирусу
                   *[false] имеет иммунитет к зомби вирусу
                }

entity-condition-guidebook-is-zombie =
    цель { $invert ->
                    [true] не зомбу
                   *[false] зомбу
                }

entity-condition-guidebook-this-metabolite = эти реагенты

entity-effect-guidebook-adjust-reagent-gaussian =
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
