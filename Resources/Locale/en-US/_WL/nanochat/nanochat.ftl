nanochat-program-name = NanoChat
nanochat-cartridge-name = NanoChat cartridge
nanochat-cartridge-desc = A program for exchanging short messages between PDAs.
nanochat-title = NanoChat
nanochat-own-number = #{ $number }
nanochat-unknown-contact = Unknown user
nanochat-no-chats = No active chats
nanochat-select-chat = Select a chat to begin
nanochat-directory-title = User directory
nanochat-directory-empty = No other NanoChat users found
nanochat-no-messages = No messages yet
nanochat-message-placeholder = Type a message…
nanochat-send = Send
nanochat-notification-title = NanoChat · [color=#999999]{ $sender }[/color]
nanochat-toggle-list-number = List your number in directory
nanochat-message-too-long = Message too long ({ $current }/{ $max })
nanochat-delivery-failed = failed to deliver
nanochat-card-examine-no-number = It doesn't seem to have a NanoChat number assigned.
nanochat-card-examine-number = Its NanoChat number is [color=lightblue]#{ $number }[/color].
nanochat-number-placeholder = Enter a number…
nanochat-start-chat = Start chat
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
nanochat-welcome-title = Start a new conversation
nanochat-welcome-details = Find a user by number or in the NanoChat directory.
nanochat-select-chat-details = Choose a conversation on the left or find a new contact.
nanochat-message-status-failed = { $time } · failed to deliver
nanochat-contact-not-found = User #{ $number } was not found.
nanochat-contact-limit-reached = The active chat limit has been reached.
nanochat-message-preview-own = Me: { $message }

log-probe-nanochat-header = NanoChat number: #{ $number }
log-probe-nanochat-header-no-number = NanoChat: no number assigned
log-probe-nanochat-contact = { $name } #{ $number } ({ $count ->
    [one] { $count } message
   *[other] { $count } messages
})
log-probe-nanochat-message = { $time } · { $direction }: { $message }
log-probe-nanochat-direction-outgoing = outgoing
log-probe-nanochat-direction-incoming = incoming
