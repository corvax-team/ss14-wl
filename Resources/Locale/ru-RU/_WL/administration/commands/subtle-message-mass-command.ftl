# Command: massmsg
cmd-massmsg-desc = Отправляет скрытое сообщение всем указанным игрокам
cmd-massmsg-help = Использование: {$command} <сообщение> <всплывающее сообщение> <user1 [user2 [user3]] | all | radius:<float>>
cmd-massmsg-player-unable = Не удалось найти игрока с таким никнеймом.
cmd-massmsg-invalid-radius = Радиус указан неверно
cmd-massmsg-invalid-radius-mode = Вы должны быть привязаны к объекту, чтобы использовать режим radius.
cmd-massmsg-command-hint = username/all/radius:<int>
cmd-massmsg-command-hint-one-args = message
cmd-massmsg-command-hint-second-args = popupMessage
cmd-massmsg-success = Сообщено отправлено { $count } { $count ->
        [one] игроку в радиусе { $radius } единиц
       *[other] игрокам в радисе { $radius } единиц
    }
