reagent-effect-guidebook-split-slime =
    { $chance ->
        [1] Расщепляет слайма
       *[other] расщепить слаймов
    } на низшие формы

reagent-effect-guidebook-create-gas =
    { $chance ->
        [1] Создаёт
       *[other] создают
    } { $moles } моль газа { $gas } с температурой { $temp }C

reagent-effect-guidebook-slime-mutation-prob-change =
    { $chance ->
        [1] Повышает
       *[other] повышает
    } вероятность мутации слайма на { $amount }% с вероятностью { $mutprob }%

reagent-effect-guidebook-knock-down =
    { $chance ->
        [1] Оглушает
       *[other] оглушает
    } существ в небольшом радиусе.

reagent-effect-guidebook-stabilize-generations =
    { $chance ->
        [1] { $increase ->
  [1] Увеличивается
  *[0] Уменьшается
}
       *[other] { $increase ->
  [1] увеличивается
  *[0] уменьшается
}
    } вероятность мутации слайма на { $value }% и эта вероятность устанавливается для { $recursive ->
        [1] всех поколений
       *[other] следующего поколения
    }

reagent-effect-guidebook-spawn-gravity-well =
    { $chance ->
        [1] Создаёт
       *[other] создаёт
    } гравитационный колодец на { $lifetime } секунд

reagent-effect-guidebook-luminescent =
    { $chance ->
        [1] Заставляет
       *[other] заставляет
    } сущность светится цветом { $color }

reagent-effect-guidebook-teleport =
    { $chance ->
        [1] Случайно
       *[other] случайно
    } телепортирует существ вокруг места, где произошла реакция

reagent-effect-guidebook-change-faction =
    { $chance ->
        [1] Меняет
       *[other] меняет
    } фракцию { $radius ->
        [0] выпившей сущности
       *[other] сущностей в радиусе { $radius }м
    } на { $faction }

reagent-effect-guidebook-change-species =
    { $chance ->
        [1] Меняет
       *[other] меняет
    } расу сущности на { $species }

reagent-effect-guidebook-change-sex =
    { $chance ->
        [1] Меняет
       *[other] меняет
    } пол { $radius ->
        [0] выпившей сущности
       *[other] сущностей в радиусе { $radius }м
    } на { $sex ->
        [0] другой
       *[other] { $sex }
    }