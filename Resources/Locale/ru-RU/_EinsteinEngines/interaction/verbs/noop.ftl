interaction-LookAt-name = Уставиться
interaction-LookAt-description = Загляни в пустоту, и она заглянет в тебя.
interaction-LookAt-success-self-popup = Вы уставились на { $target }.
interaction-LookAt-success-target-popup = Вы краем глаза заметили, что { $user } пялится на вас...
interaction-LookAt-success-others-popup = { $user } уставился на { $target }.

interaction-Hug-name = Обнять
interaction-Hug-description = Одно объятие в день подавляет неподвластные твоему пониманию ужасы.
interaction-Hug-success-self-popup = Вы обнимаете { $target }.
interaction-Hug-success-target-popup = { $user } обнимает вас.
interaction-Hug-success-others-popup = { $user } обнимает { $target }.

interaction-Pet-name = Погладить
interaction-Pet-description = Погладьте коллегу для снятия стресса.
interaction-Pet-success-self-popup = Вы гладите { $target } по { POSS-ADJ($target) } голове.
interaction-Pet-success-target-popup = { $user } гладит по вашей голове.
interaction-Pet-success-others-popup = { $user } гладит { $target }.

interaction-KnockOn-name = Постучать
interaction-KnockOn-description = Постучите для привлечения внимания.
interaction-KnockOn-success-self-popup = Вы стучите по { $target }.
interaction-KnockOn-success-target-popup = { $user } стучит по вам.
interaction-KnockOn-success-others-popup = { $user } стучит по { $target }.

interaction-Rattle-name = Побряцать
interaction-Rattle-success-self-popup = Вы дребезжите { $target }.
interaction-Rattle-success-target-popup = { $user } дребезжит вами.
interaction-Rattle-success-others-popup = { $user } дребезжит { $target }.

#  below includes conditionals for if  user is holding an item
interaction-WaveAt-name = Помахать
interaction-WaveAt-description = Помашите цели. Если вы держите предмет, вы помашите им.
interaction-WaveAt-success-self-popup = Вы машете {$hasUsed ->
    [false] { $target }.
    *[true] { $target } держа в руках {$used}.
}
interaction-WaveAt-success-target-popup = { $user } машет {$hasUsed ->
    [false] вам.
    *[true] {POSS-PRONOUN($user)} вам, держа в руках {$used}.
}
interaction-WaveAt-success-others-popup = { $user } машет {$hasUsed ->
    [false] { $target }.
    *[true] {POSS-PRONOUN($user)} {$used} { $target }.
}
