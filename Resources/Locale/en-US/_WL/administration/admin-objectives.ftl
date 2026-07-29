admin-objective-issuer = Event objectives

cmd-addadmintask-desc = Adds a custom objective to a player.
cmd-addadmintask-help = addadmintask <ckey> <name> <description>

cmd-rmadmintask-desc = Removes a custom objective from a player.
cmd-rmadmintask-help = rmadmintask <ckey> <id>

cmd-editadmintask-desc = Edits a field of a player's custom objective.
cmd-editadmintask-help = editadmintask <ckey> <id> <name|description|progress> <value>

admin-objective-error-player-not-found = Player '{$ckey}' not found or offline.
admin-objective-error-no-mind = '{$ckey}' has no active mind.
admin-objective-error-task-not-found = No task with id {$id} found for {$ckey}.
admin-objective-error-invalid-field = Unknown field. Valid: name, description, progress.
admin-objective-error-invalid-progress = Progress must be a number between 0 and 1, or you've used . instead of ,
admin-objective-error-add-failed = Failed to add task.

admin-objective-success-added = Added task '{$name}' to {$ckey}.
admin-objective-success-removed = Removed task '{$name}' (#{$id}) from {$ckey}.
admin-objective-success-edited = Updated {$field} on task #{$id} for {$ckey}.

admin-objective-default-name = Admin objective
admin-objective-default-description = A task assigned by an administrator.
