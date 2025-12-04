-health-analyzer-rating = { $rating ->
    [good] ([color=#00D3B8]хорошо[/color])
    [okay] ([color=#30CC19]удовлетворительно[/color])
    [poor] ([color=#bdcc00]неудовлетворительно[/color])
    [bad] ([color=#E8CB2D]плохо[/color])
    [awful] ([color=#EF973C]ужасно[/color])
    [dangerous] ([color=#FF6C7F]опасно[/color])
   *[other] (неизвестно)
    }

health-analyzer-window-entity-brain-health-text = Мозговая активность:
health-analyzer-window-entity-blood-pressure-text = Кровяное давление:
health-analyzer-window-entity-blood-oxygenation-text = Сатурация крови:
health-analyzer-window-entity-blood-flow-text = Кровоток:
health-analyzer-window-entity-heart-rate-text = Пульс:
health-analyzer-window-entity-heart-health-text = Здоровье сердца:
health-analyzer-window-entity-lung-health-text = Здоровье лёгких:

health-analyzer-window-entity-brain-health-value = {$value}% { -health-analyzer-rating(rating: $rating) }
health-analyzer-window-entity-heart-health-value = {$value}% { -health-analyzer-rating(rating: $rating) }
health-analyzer-window-entity-lung-health-value = {$value}% { -health-analyzer-rating(rating: $rating) }
health-analyzer-window-entity-heart-rate-value = {$value}bpm { -health-analyzer-rating(rating: $rating) }
health-analyzer-window-entity-blood-oxygenation-value = {$value}% { -health-analyzer-rating(rating: $rating) }
health-analyzer-window-entity-blood-pressure-value = {$systolic}/{$diastolic} { -health-analyzer-rating(rating: $rating) }
health-analyzer-window-entity-blood-flow-value = {$value}% { -health-analyzer-rating(rating: $rating) }
health-analyzer-window-entity-non-medical-reagents = [color=yellow]Пациент имеет немедицинские вещества в крови.[/color]

wound-husking = [color=red]Пациент имеет ожоги четвёртой степени.[/color]
wound-bone-death = [color=red]Пациент имеет системную костную смерть.[/color]
wound-internal-fracture = [color=red]Пациент имеет внутренние переломы.[/color]
wound-incision = [color=red]Пациент имеет открытый разрез.[/color]
wound-clamped = [color=red]Пациент имеет пережатые сосуды.[/color]
wound-retracted = [color=red]У пациента раздвинута кожа.[/color]
wound-ribcage-open = [color=red]У пациента открыта грудная клетка.[/color]
wound-arterial-bleeding = [color=red]У пациента артериальное кровотечение.[/color]

health-analyzer-window-no-patient-damages = Пациент не имеет телесных повреждений.

health-analyzer-status-tooltip =
    {"[bold]"}Жив[/bold]: Пациент жив и в сознании.
    {"[bold]"}Критическое состояние[/bold]: Пациент без сознания и умрёт без вмешательства.
    {"[bold]"}Мёртв[/bold]: Пациент мёртв и сгниёт без вмешательства.

health-analyzer-blood-saturation-tooltip =
    То, сколько кислорода (или азота и т.д.) получает мозг пациента.

    { $rating ->
    [good] Мозг пациента в безопасности.
    [okay] Мозг пациента может слегка повредиться.
    [poor] Мозг пациента может повредиться.
    [bad] Мозг пациента может серьёзно повредиться.
    [awful] Мозг пациента имеет [color=red]серьёзный риск[/color] фатальных повреждений.
    [dangerous] Мозг пациента имеет [color=red]смертельно опасный[/color] фатальных повреждений.
   *[other] Твой пациент загадочник. Чтобы разгадать загадку загадочника, обратись к разработчикам.
    }

    Влияющие показатели:
    {"[color=#7af396]"}Кровяное давление[/color]: {$systolic}/{$diastolic}
    {"[color=#7af396]"}Асфиксия[/color]: {$asphyxiation}

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

health-analyzer-blood-volume-tooltip =
    Количество крове в теле пациента.

    Если [color=#7af396]Кровоток[/color] высокий, но [color=#7af396]Кровяное давление[/color] нет, убедись, что пациент имеет достаточный [color=#7af396]Объём крови[/color].

    Влияющие показатели:
    {"[color=#7af396]"}Кровоток[/color]: {$flow}%

health-analyzer-blood-flow-tooltip =
    Количество крови, циркулирующее по телу пациента.

    Напрямую зависит от сердца пациента и его пульса.
    СЛР может помочь, если сердце не прокачивает достаточно крови.

    Влияющие показатели:
    {"[color=#7af396]"}Пульс[/color]: {$heartrate}bpm
    {"[color=#7af396]"}Здоровье сердца[/color]: {$health}%

health-analyzer-heart-rate-tooltip =
    Скорость сердцебиения пациента.

    Падение может привести к асфиксии и боли.

    Оно может остановиться от сильной боли, недостатока крови или серьёзных повреждений мозга.

    {"[color=#731024]"}Инапровалин[/color] может быть использован для стабилизации пульса.

    Влияющие показатели:
    {"[color=#7af396]"}Асфиксия[/color]: {$asphyxiation}

health-analyzer-heart-health-tooltip =
    Показатель здоровья сердца.

    Уменьшается из-за чрезмерно высокого пульса.

    Влияющиеу показатели:
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
   *[false] {-health-analyzer-brain-health-tooltip-alive(rating: $rating, saturation: $saturation)}
    }

-health-analyzer-brain-health-tooltip-alive =
    { $rating ->
    [good] Пациент в порядке и не требует никаких дополнительных мер.
    [okay] Пациент имеет лёгкие повреждения мозга, но они скоро восстановятся.
    [poor] Пациент имеет повреждения мозга.
    [bad] Пациент имеет значительные повреждения мозга. Используйте [color=#731024]Инапровалин[/color] для стабилизации мозга пациента перед последующими манипуляциями.
    [awful] Пациент имеет значительные повреждения мозга. [bold]Используйту [color=#731024]Инапровалин[/color] для стабилизаци мозга немедленно.[/bold] Применение криокапсул или криостазисных кроватей может быть хорошей идеей.
    [dangerous] Пациент в [color=red]смертельной опасности[/color]. [bold]Используйте [color=#731024]Инапровалин[/color] и перемести пациента на криостазисную кровать или в криокапсулу, если нет плана лучше.[/bold]
   *[other] Твой пациент загадочник. Чтобы разгадать загадку загадочника, обратись к разработчикам.
    }

    {"[color=#fedb79]"}Маннитол[/color] может быть использован для восстановления мозга, если [color=#7af396]Сатурация[/color] позволяет.

    Влияющие показатели:
    {"[color=#7af396]"}Сатурация[/color]: {$saturation}%

-health-analyzer-brain-health-tooltip-dead =
    Мозг пациента не проявляет активности. Он мёртв.
