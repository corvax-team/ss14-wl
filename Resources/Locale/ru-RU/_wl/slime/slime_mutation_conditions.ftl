slime-transformation-condition-entity-nearby = в радиусе { $radius }м { $white ->
    [0] {""}
    *[other] будет одна из сущностей: { $white }
  } { $black ->
    [0] {""}
    *[other] { $white -> 
    [0] {""}
    *[other] - и
 } не будет сущностей: { $black }
}

slime-transformation-condition-relationship = слайм имеет минимум { $min } очков отношений и максимум { $max } { $all ->
        [0] хотя бы с одной сущностью
       *[1] со всеми сущностями
    } в радиусе { $radius }м

slime-transformation-condition-reagent = минимум { $min }u и максимум { $max }u { $name }
slime-transformation-condition-reagent-inside = слайм содержит в себе { $all ->
        [0] хотя бы один перечисленный реагент
       *[1] все перечисленные реагенты
    }: { $reagents }

slime-transformation-condition-tile-temperature = рядом есть { $gas ->
    *[other] { $gas }
    [0] любой газ
}, находящийся {$state ->
    [bothnull] при любой температуре
    *[both] при температуре в пределах от {$min}K до {$max}K
    [maxnull] при температуре {$min}K и более
    [minnull] при температуре {$max}K и менее
}

slime-transformation-condition-life-stage = стадия жизни слайма ниже {$max}

slime-transformation-condition-job-nearby = в радиусе { $radius }м { $white ->
  [0] {""}
  *[other] { $all ->
  [0] есть
  *[1] все
 } рабочие со следующими должностями: { $white }
}{ $white ->
  [0] {""}
  *[other] { $black ->
  [0] {""}
  *[other] ; и{" "}
 }
}{ $black ->
    [0] {""}
    *[other] нет рабочих со следующими должностями: { $black }
}
