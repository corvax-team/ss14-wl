fax-machine-paper-empty = В факсе не осталось бумаги!
fax-machine-examine-papers-remaining = Листов в хранилище: [color=yellow]{$count}[/color].
fax-machine-examine-is-storage-open = Хранилище бумаги: {$isopen ->
    [true] [color=green]открыто[/color]
    *[false] [color=red]закрыто[/color]
}.
fax-machine-ui-open-storage-button = Хранилище
fax-machine-ui-turn-notify-button = Оповещение
fax-machine-ui-open-storage-button-state = {$open ->
    [true] [color=green]Открыто[/color]
    *[false] [color=red]Закрыто[/color]
}
fax-machine-ui-turn-notify-button-state = {$enabled ->
    [true] [color=green]Включено[/color]
    *[false] [color=red]Выключено[/color]
}
fax-machine-ui-papers-in-storage-count = {$count ->
    [0] [color=red]0[/color]
    *[other] [color=green]{$count}[/color]
}
fax-machine-ui-papers-in-storage-label-name = Листы в хранилище:
