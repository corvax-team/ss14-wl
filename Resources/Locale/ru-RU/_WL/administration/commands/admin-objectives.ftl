admin-objective-issuer = Ивентовые задачи

cmd-addadmintask-desc = Добавляет кастомную задачу игроку.
cmd-addadmintask-help = addadmintask <ckey> <Имя задачи> <Описание>

cmd-rmadmintask-desc = Удаляет кастомную задачу у игрока.
cmd-rmadmintask-help = rmadmintask <ckey> <id>

cmd-editadmintask-desc = Редактирует значение у кастомной задачи игрока.
cmd-editadmintask-help = editadmintask <ckey> <id> <name|description|progress> <Значение>

admin-objective-error-player-not-found = Игрок '{$ckey}' не найден.
admin-objective-error-no-mind = '{$ckey}' не обладает активным разумом.
admin-objective-error-task-not-found = Кастомная задача с айди {$id} для игрока {$ckey} не найдена.
admin-objective-error-invalid-field = Неизвестное значение. Доступные значения: name, description, progress.
admin-objective-error-invalid-progress = Прогресс должен быть числом от 0 до 1, либо вы поставили . вместо ,
admin-objective-error-add-failed = Не удалось добавить кастомную задачу.

admin-objective-success-added = Добавлена кастомная задача '{$name}' игроку {$ckey}.
admin-objective-success-removed = Удалена кастомная задача '{$name}' (#{$id}) у игрока {$ckey}.
admin-objective-success-edited = Обновлено поле {$field} у задачи #{$id} для игрока {$ckey}.

admin-objective-default-name = ивентовая задача
admin-objective-default-description = Ивентовая задача, назначенная игровым мастером.
