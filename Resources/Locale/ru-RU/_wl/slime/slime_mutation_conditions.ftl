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

slime-transformation-condition-tile-temperature = { $separator }рядом есть { $gas ->
        [0] любой газ
       *[other] { $gas }
    } { $state ->
        *[bothnull] { $separator }
        [other] с температурой 
        } { $min ->
        [0] { $separator }
       *[other] больше { $min }K
    } { $state ->
        [both] и
       *[other] { $separator } 
        } { $max ->
        [0] { $separator }
       *[other] меньше { $max }K
    }

slime-transformation-condition-life-stage = стадия жизни слайма { $both ->
  [0] находится в пределах с { $min } до { $max }
  *[1] - { $min }
}

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