nanochat-program-name = NanoChat
nanochat-cartridge-name = NanoChat cartridge
nanochat-cartridge-desc = A program for exchanging short messages between PDAs.
nanochat-title = NanoChat
nanochat-own-number = #{ $number }
nanochat-unknown-contact = Unknown user
nanochat-no-chats = No active chats
nanochat-select-chat = Select a chat to begin
nanochat-directory-empty = No other NanoChat users found
nanochat-no-messages = No messages yet
nanochat-message-placeholder = Type a message…
nanochat-send = Send
nanochat-notification-title = NanoChat · [color=#999999]{ $sender }[/color]
nanochat-message-too-long = Message too long ({ $current }/{ $max })
nanochat-card-examine-no-number = It doesn't seem to have a NanoChat number assigned.
nanochat-card-examine-number = Its NanoChat number is [color=lightblue]#{ $number }[/color].
nanochat-number-placeholder = ####
nanochat-delete-chat = Delete chat
nanochat-delete-chat-confirm = Press again to delete this chat
nanochat-search-contacts = Search contacts
nanochat-return-to-chats = Return to chats
nanochat-search = Search
nanochat-return = Chats
nanochat-disable-notifications = Disable notifications
nanochat-enable-notifications = Enable notifications
nanochat-show-in-search = Allow other users to find me in NanoChat.
nanochat-hide-from-search = Hide my contact from NanoChat search.
nanochat-welcome-title = Welcome to NanoChat!
nanochat-welcome-details = Find the contact you need and start chatting.
nanochat-welcome-find-contact = Find a contact
nanochat-select-chat-details = Choose a conversation on the left or find a new contact.
nanochat-message-status-failed = { $time } · failed to deliver
nanochat-contact-not-found = User #{ $number } was not found.
nanochat-contact-limit-reached = The active chat limit has been reached.
nanochat-message-preview-own = Me: { $message }
nanochat-directory-search-placeholder = Search by name or job…
nanochat-message-status-partial = { $time } · delivered { $delivered }/{ $total }
nanochat-block-contact = Block
nanochat-unblock-contact = Unblock
nanochat-group-create = Create
nanochat-group-new = Group
nanochat-group-name-placeholder = Group name…
nanochat-group-rename = Rename
nanochat-group-mute = Mute notifications
nanochat-group-unmute = Enable notifications
nanochat-group-leave = Leave
nanochat-group-delete = Delete group
nanochat-group-delete-confirm = Delete for everyone?
nanochat-group-add-member = Add member
nanochat-group-make-admin = Make admin
nanochat-group-remove-admin = Remove admin
nanochat-group-remove-member = Remove member

log-probe-nanochat-header = NanoChat number: #{ $number }
log-probe-nanochat-header-no-number = NanoChat: no number assigned
log-probe-nanochat-contact = { $name } #{ $number } { $blocked } ({ $count ->
    [one] { $count } message
   *[other] { $count } messages
})
log-probe-nanochat-message = { $time } · { $direction }: { $message }
log-probe-nanochat-direction-outgoing = outgoing
log-probe-nanochat-direction-incoming = incoming
log-probe-nanochat-group = Group “{ $name }” ({ $count ->
    [one] { $count } message
   *[other] { $count } messages
})
log-probe-nanochat-group-message = { $time } · { $sender }: { $message }
log-probe-section-nanochat = NanoChat correspondence
log-probe-section-direct-chats = Direct chats
log-probe-section-group-chats = Group chats
log-probe-section-access-logs = Access history
log-probe-nanochat-blocked-suffix = · blocked
log-probe-nanochat-blocked-list-ui = Blocked: { $contacts }
log-probe-nanochat-preview-truncated = Showing the latest { $shown } of { $total } messages.

log-probe-nanochat-header-formatted = [bold]NanoChat number:[/bold] #{ $number }
log-probe-nanochat-header-no-number-formatted = [bold]NanoChat number:[/bold] not assigned
log-probe-nanochat-blocked-list = [bold]Blocked contacts:[/bold] { $numbers }
log-probe-nanochat-contact-formatted = [bold]{ $name }[/bold] [color=#55555D]#{ $number } { $blocked } · { $count ->
    [one] { $count } message
   *[other] { $count } messages
}[/color]
log-probe-nanochat-message-formatted = [color=#77777D]{ $time } · { $direction }[/color]  { $message }
log-probe-nanochat-group-formatted = [bold]Group “{ $name }”[/bold] [color=#55555D]· { $count ->
    [one] { $count } message
   *[other] { $count } messages
}[/color]
log-probe-nanochat-group-message-formatted = [color=#77777D]{ $time }[/color]  [bold]{ $sender }:[/bold] { $message }
