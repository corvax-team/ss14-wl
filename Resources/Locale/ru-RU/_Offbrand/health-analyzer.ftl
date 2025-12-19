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
health-analyzer-window-entity-spo2-text = {LOC($spo2)}:
health-analyzer-window-entity-etco2-text = {LOC($etco2)}:
health-analyzer-window-entity-respiratory-rate-text = Частота Дыхания:

health-analyzer-window-entity-brain-health-value = {$value}% {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-heart-health-value = {$value}% {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-lung-health-value = {$value}% {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-heart-rate-value = {$value}уд/мин {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-blood-pressure-value = {$systolic}/{$diastolic} {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-respiratory-rate-value = {$value}вдохов/в минуту
health-analyzer-window-entity-spo2-value = {$value}% {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-etco2-value = {$value}мм рт. ст. {-health-analyzer-rating(rating: $rating)}
health-analyzer-window-entity-non-medical-reagents = [color=yellow]Пациент имеет немедицинские реагенты в крови.[/color]

wound-bone-death = [color=red]Пациует имеет системную костную смерть.[/color]
wound-internal-fracture = [color=red]Пациент имеет внутренний перелом.[/color]
wound-incision = [color=red]Пациент имеет открытый разрез.[/color]
wound-clamped = [color=red]У пациента пережаты артерии.[/color]
wound-retracted = [color=red]У пациента раздвинута кожа.[/color]
wound-ribcage-open = [color=red]У пациента открыта грудная клетка.[/color]
wound-arterial-bleeding = [color=red]Пациент имеет артериальное кровотечение.[/color]
wound-husking = [color=red]Пациент имеет ожоги четвёртой степени.[/color]

etco2-carbon-dioxide = EtCO2
etco2-ammonia = EtNH3
etco2-nitrous-oxide = EtN2O

spo2-oxygen = SpO2
spo2-nitrogen = SpN2

health-analyzer-window-no-patient-damages = Пациент полностью здоров.

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
    Показатель того, насколько количество {LOC($gas)}{"а"} поступающего в организм, соответствует потребностям.

    Влияющие показатели:
    {"[color=#7af396]"}Метаболизм[/color]:
        Физические травмы и боль могут вызвать большую потребность в {LOC($gas)}{"е"}.

    {"[color=#7af396]"}Кровяное давление[/color]:
        Низкое кровяное давление может может привести к снижению уровня {LOC($spo2)}.

    {"[color=#7af396]"}Здоровье лёгких[/color]:
        Низкое здоровье лёгких может привести к снижению уровня {LOC($spo2)}.

    {"[color=#7af396]"}Асфиксия[/color]:
        Асфиксия может привести к снижению уровня {LOC($spo2)}.

    {"[color=#7af396]"}Частота дыхания[/color]:
        Гипервентиляция может привести к тому, что пациент будет вдыхать меньше воздуха за один вдох.

health-analyzer-heart-rate-tooltip =
    Частота сокращений сердца

    Частота сердечных сокращений увеличивается в ответ на недостаточный уровень {LOC($spo2)} для компенсации.

health-analyzer-respiratory-rate-tooltip =
    Частота дыхания

    Слишком частое дыхание может привести к уменьшению объема воздуха за вдох, вызывая удушье.

    {"[color=#731024]"}Инапровалин[/color] может быть использован для нормализации дыхания.

    Влияющие показатели:
    {"[color=#7af396]"}{LOC($spo2)}[/color]:
        Недостаточное поступление {LOC($spo2gas)}{"а"} может привести к учащению дыхания.

    {"[color=#7af396]"}Metabolic Rate[/color]:
        Физическая травма и боль могут вызывать учащение дыхания.

health-analyzer-etco2-tooltip =
    Показатель количества {LOC($gas)} с каждым выдохом.

    Низкий уровень {LOC($etco2)} может привести к накоплению токсичного {LOC($gas)}.

    Влияющие показатели:
    {"[color=#7af396]"}Частота дыхания[/color]:
        Неравномерное дыхание может привести к неполному выдоху всего {LOC($gas)}..

    {"[color=#7af396]"}Кровяное давление[/color]:
        Низкое кровяное давление может привести к задержке большего количества {LOC($gas)} в организме.

health-analyzer-heart-health-tooltip =
    Показатель здоровья сердца.

    Уменьшается из-за чрезмерно высокого пульса.

    Влияющие показатели:
    {"[color=#7af396]"}Пульс[/color]: {$heartrate}уд/мин

health-analyzer-plain-temperature-tooltip =
    Температура тела пациента.

health-analyzer-cryostasis-temperature-tooltip =
    Температура тела пациента.

    Имеет криостазисный коэффициент {$factor}%.

health-analyzer-lung-health-tooltip =
    Здоровье лёгких пациента.

    Чем ниже это число, тем труднее ему дышать.

    Если здоровье лёгких низкое, подумайте о переводе пациента на баллоны с повышенным давлением.

health-analyzer-blood-tooltip =
    Объём крови пациента.

health-analyzer-damage-tooltip =
    Суммарные повреждения пациента.

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
