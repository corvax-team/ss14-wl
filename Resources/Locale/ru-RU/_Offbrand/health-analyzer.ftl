-health-analyzer-rating = { $rating ->
    [good] ([color=#00D3B8]хорошо[/color])
    [okay] ([color=#30CC19]удовлетворительно[/color])
    [poor] ([color=#bdcc00]неудовлетворительно[/color])
    [bad] ([color=#E8CB2D]плохо[/color])
    [awful] ([color=#EF973C]ужасно[/color])
    [dangerous] ([color=#FF6C7F]опасно[/color])
   *[other] (неизвестно)
    }

health-analyzer-window-entity-brain-health-text = Мозговая Активность:
health-analyzer-window-entity-blood-pressure-text = Кровяное Давление:
health-analyzer-window-entity-heart-rate-text = Сердцебиение:
health-analyzer-window-entity-heart-health-text = Здоровье Сердца:
health-analyzer-window-entity-lung-health-text = Здоровье Лёгких:
health-analyzer-window-entity-spo2-text = { $spo2 }:
health-analyzer-window-entity-etco2-text = { $etco2 }:
health-analyzer-window-entity-respiratory-rate-text = Частота Дыхания:

health-analyzer-window-entity-brain-health-value = {$value}% {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-heart-health-value = {$value}% {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-lung-health-value = {$value}% {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-heart-rate-value = {$value} уд/мин {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-blood-pressure-value = {$systolic}/{$diastolic} {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-respiratory-rate-value = {$value} вдох/мин
health-analyzer-window-entity-spo2-value = {$value}% {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-etco2-value = {$value} мм рт. ст. {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-non-medical-reagents = [color=yellow]Пациент имеет немедицинские вещества в крови.[/color]

wound-bone-death = [color=red]У пациента системная костная смерть.[/color]
wound-internal-fracture = [color=red]У пациента закрытый перелом.[/color]
wound-incision = [color=red]У пациента вскрыта кожа.[/color]
wound-clamped = [color=red]У пациента пережаты артерии.[/color]
wound-retracted = [color=red]У пациента раздвинута кожа.[/color]
wound-ribcage-open = [color=red]У пациента вскрыта грудная клетка.[/color]
wound-arterial-bleeding = [color=red]У пациента артериальное кровотечение.[/color]
wound-husking = [color=red]У пациента тяжёлые ожоги четвёртой степени.[/color]

etco2-carbon-dioxide = EtCO2
etco2-ammonia = EtNH3
etco2-nitrous-oxide = EtN2O

spo2-oxygen = SpO2
spo2-nitrogen = SpN2

health-analyzer-window-no-patient-damages = У пациента нет повреждений.

health-analyzer-status-tooltip =
    {"[bold]"}Жив[/bold]: Пациент жив и в сознании.
    {"[bold]"}Критическое состояние[/bold]: Пациент без сознания и умрёт без помощи.
    {"[bold]"}Мёртв[/bold]: Пациент мёртв и скоро начнёт гнить.

health-analyzer-blood-pressure-tooltip =
    Показатель того, насколько хорошо кровь циркулирует по телу.

    Капельницы можно использовать для восполнения объёма крови.

    Связанные показатели:
    {"[color=#7af396]"}Объём крови[/color]:
        Низкий объём крови может привести к снижению давления.

    {"[color=#7af396]"}Активность мозга[/color]:
        Низкая активность мозга может привести к снижению давления.

    {"[color=#7af396]"}Пульс и состояние сердца[/color]:
        Повреждение сердца или его остановка могут привести к снижению давления.

health-analyzer-spo2-tooltip =
    Показатель того, насколько { $gas }{"а"} хватает органам.

    Влияющие показатели:
    {"[color=#7af396]"}Метаболизм[/color]:
        Физические травмы и боль могут вызвать большую потребность в { $gas }{"е"}.

    {"[color=#7af396]"}Кровяное давление[/color]:
        Низкое давление может может снижать уровень { $spo2 }.

    {"[color=#7af396]"}Здоровье лёгких[/color]:
        Плохое состояние лёгких может снижать уровень { $spo2 }.

    {"[color=#7af396]"}Асфиксия[/color]:
        Асфиксия может снижать уровень { $spo2 }.

    {"[color=#7af396]"}Частота дыхания[/color]:
        Гипервентиляция может привести к тому, что пациент будет вдыхать меньше воздуха за один вдох.

health-analyzer-heart-rate-tooltip =
    То, как часто бьётся сердце пациента.

    Сердцебиение увеличивается в ответ на недостаточный уровень { $spo2 }.

health-analyzer-respiratory-rate-tooltip =
    То, как быстро пациент дышит.

    Слишком частое дыхание может привести к уменьшению объема воздуха за вдох, вызывая удушье.

    {"[color=#731024]"}Инапровалин[/color] может быть использован для нормализации дыхания.

    Влияющие показатели:
    {"[color=#7af396]"}{ $spo2 }[/color]:
        Недостаточное поступление { $spo2gas }{"а"} может привести к учащению дыхания.

    {"[color=#7af396]"}Metabolic Rate[/color]:
        Физическая травма и боль могут вызывать учащение дыхания.

health-analyzer-etco2-tooltip =
    Показатель вывода { $gas } через дыхания.

    Низкий уровень { $etco2 } может привести к накоплению { $gas } и отравлению.

    Влияющие показатели:
    {"[color=#7af396]"}Частота дыхания[/color]:
        Неравномерное дыхание может привести к неполному выдоху всего { $gas }..

    {"[color=#7af396]"}Кровяное давление[/color]:
        Низкое кровяное давление может привести к задержке большего количества { $gas } в организме.

health-analyzer-heart-health-tooltip =
    Здоровье сердца.

    Падает из-за чрезмерно высокого пульса.

    Влияющие показатели:
    {"[color=#7af396]"}Пульс[/color]: {$heartrate}уд/мин

health-analyzer-plain-temperature-tooltip =
    Температура тела пациента.

health-analyzer-cryostasis-temperature-tooltip =
    Температура тела пациента.

    Имеет криостазисный коэффициент {$factor}%.

health-analyzer-lung-health-tooltip =
    Здоровье лёгких пациента.

    Чем ниже, тем труднее ему дышать.

    Если здоровье лёгких низкое, стоит перевести пациента на баллоны с повышенным давлением.

health-analyzer-blood-tooltip =
    Объём крови пациента.

health-analyzer-damage-tooltip =
    Общие повреждения пациента.

health-analyzer-brain-health-tooltip = { $dead ->
    [true] {-health-analyzer-brain-health-tooltip-dead}
   *[false] {-health-analyzer-brain-health-tooltip-alive(spo2: $spo2)}
}

-health-analyzer-brain-health-tooltip-alive =
    {"[color=#fedb79]"}Маннитол[/color] может быть использован для восстановления мозга, если [color=#7af396]SpO2[/color] позволяет.

    Влияющие показатели:
    {"[color=#7af396]"}SpO2[/color]: {$spo2}%

-health-analyzer-brain-health-tooltip-dead =
    Мозг пациента не проявляет активности. Он мёртв.
