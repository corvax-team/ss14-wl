nanochat-program-name = NanoChat
nanochat-cartridge-name = картридж NanoChat
nanochat-cartridge-desc = Программа для обмена короткими сообщениями между КПК.
nanochat-title = NanoChat
nanochat-own-number = #{ $number }
nanochat-unknown-contact = Неизвестный пользователь
nanochat-no-chats = Нет активных чатов
nanochat-select-chat = Выберите чат, чтобы начать
nanochat-directory-empty = Другие пользователи NanoChat не найдены
nanochat-no-messages = Сообщений пока нет
nanochat-message-placeholder = Введите сообщение…
nanochat-send = Отправить
nanochat-notification-title = NanoChat · [color=#999999]{ $sender }[/color]
nanochat-message-too-long = Слишком длинное сообщение ({ $current }/{ $max })
nanochat-card-examine-no-number = Похоже, ей не присвоен номер NanoChat.
nanochat-card-examine-number = Её номер NanoChat: [color=lightblue]#{ $number }[/color].
nanochat-number-placeholder = ####
nanochat-delete-chat = Удалить чат
nanochat-delete-chat-confirm = Нажмите ещё раз, чтобы удалить чат
nanochat-search-contacts = Найти контакты
nanochat-return-to-chats = Вернуться к чатам
nanochat-search = Поиск
nanochat-return = Чаты
nanochat-disable-notifications = Отключить уведомления
nanochat-enable-notifications = Включить уведомления
nanochat-show-in-search = Разрешить другим пользователям находить меня в NanoChat.
nanochat-hide-from-search = Скрыть мой контакт из поиска NanoChat.
nanochat-welcome-title = Добро пожаловать в NanoChat!
nanochat-welcome-details = Найдите нужный контакт и начните общение.
nanochat-welcome-find-contact = Найти контакт
nanochat-select-chat-details = Выберите существующий диалог слева или найдите новый контакт.
nanochat-message-status-failed = { $time } · не доставлено
nanochat-contact-not-found = Пользователь с номером #{ $number } не найден.
nanochat-contact-limit-reached = Достигнут лимит активных чатов.
nanochat-message-preview-own = Я: { $message }
nanochat-directory-search-placeholder = Поиск по имени или должности…
nanochat-message-status-partial = { $time } · доставлено { $delivered }/{ $total }
nanochat-block-contact = Заблокировать
nanochat-unblock-contact = Разблокировать
nanochat-group-create = Создать
nanochat-group-new = Группа
nanochat-group-name-placeholder = Название группы…
nanochat-group-rename = Изменить
nanochat-group-mute = Отключить уведомления
nanochat-group-unmute = Включить уведомления
nanochat-group-leave = Выйти
nanochat-group-delete = Удалить группу
nanochat-group-delete-confirm = Точно удалить?
nanochat-group-add-member = Добавить участника
nanochat-group-make-admin = Сделать админом
nanochat-group-remove-admin = Снять админа
nanochat-group-remove-member = Исключить участника

log-probe-nanochat-header = Номер NanoChat: #{ $number }
log-probe-nanochat-header-no-number = NanoChat: номер не присвоен
log-probe-nanochat-contact = { $name } #{ $number } { $blocked } ({ $count } сообщ.)
log-probe-nanochat-message = { $time } · { $direction }: { $message }
log-probe-nanochat-direction-outgoing = исходящее
log-probe-nanochat-direction-incoming = входящее
log-probe-nanochat-group = Группа «{ $name }» ({ $count } сообщ.)
log-probe-nanochat-group-message = { $time } · { $sender }: { $message }
log-probe-section-nanochat = Переписка NanoChat
log-probe-section-direct-chats = Личные диалоги
log-probe-section-group-chats = Групповые чаты
log-probe-section-access-logs = История доступов
log-probe-nanochat-blocked-suffix = · заблокирован
log-probe-nanochat-blocked-list-ui = Заблокированы: { $contacts }
log-probe-nanochat-preview-truncated = Показаны последние { $shown } из { $total } сообщений.

log-probe-nanochat-header-formatted = [bold]Номер NanoChat:[/bold] #{ $number }
log-probe-nanochat-header-no-number-formatted = [bold]Номер NanoChat:[/bold] не присвоен
log-probe-nanochat-blocked-list = [bold]Заблокированные контакты:[/bold] { $numbers }
log-probe-nanochat-contact-formatted = [bold]{ $name }[/bold] [color=#55555D]#{ $number } { $blocked } · { $count } сообщ.[/color]
log-probe-nanochat-message-formatted = [color=#77777D]{ $time } · { $direction }[/color]  { $message }
log-probe-nanochat-group-formatted = [bold]Группа «{ $name }»[/bold] [color=#55555D]· { $count } сообщ.[/color]
log-probe-nanochat-group-message-formatted = [color=#77777D]{ $time }[/color]  [bold]{ $sender }:[/bold] { $message }
