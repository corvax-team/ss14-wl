-crew-monitor-vitals-rating = { $rating ->
    [good] [color=#00D3B8]{$text}[/color]
    [okay] [color=#30CC19]{$text}[/color]
    [poor] [color=#bdcc00]{$text}[/color]
    [bad] [color=#E8CB2D]{$text}[/color]
    [awful] [color=#EF973C]{$text}[/color]
    [dangerous] [color=#FF6C7F]{$text}[/color]
   *[other] неизвестно
    }

offbrand-crew-monitoring-heart-rate = [color=white]{ $rate }[/color]уд/мин
offbrand-crew-monitoring-blood-pressure = [color=white]{ $systolic }[/color]/[color=white]{ $diastolic }[/color]
offbrand-crew-monitoring-spo2 = [color=white]{ $value }[/color]% { LOC($spo2) }
