cpr-target-needs-cpr = [color=red]{ CAPITALIZE(SUBJECT($target)) } { CONJUGATE-HAVE($target) } не имеет пульса и задыхается![/color]

fracture-examine = [color=red]{ CAPITALIZE(SUBJECT($target)) } { CONJUGATE-BASIC($target, "выглядит", "выглядит") } будто что-то неправильной формы застряло { POSS-ADJ($target) } под кожей![/color]
arterial-bleeding-examine = [color=red]{ CAPITALIZE(SUBJECT($target)) } фонтанирует кровью![/color]
bone-death-examine = [color=red]{ CAPITALIZE(SUBJECT($target)) } искалечен![/color]

wound-bleeding-modifier = [color=red]кровоточащие {$wound}[/color]
wound-tended-modifier = ухоженные {$wound}
wound-bandaged-modifier = перевязанные {$wound}
wound-salved-modifier = помазанные {$wound}

tourniquet-applied-examine = { CAPITALIZE(SUBJECT($target)) } { CONJUGATE-HAVE($target) } применяет турникет на { OBJECT($target) }.
splints-applied-examine = { CAPITALIZE(SUBJECT($target)) } { CONJUGATE-HAVE($target) } накладывает шину { OBJECT($target) }.

wound-count-modifier =
    { CAPITALIZE(SUBJECT($target)) } { CONJUGATE-HAVE($target) } { $count ->
        [1] { $wound }
        [2] два { $wound }
        [3] несколько { $wound }
        [4] несколько { $wound }
        [5] несколько { $wound }
        [6] много { $wound }
        [7] много { $wound }
        [8] много { $wound }
       *[other] очень много { $wound }
    }.

-wound-plural-modifier-s = { $count ->
    [one]{""}
   *[other]{"ы"}
}

-wound-plural-modifier-es = { $count ->
    [one]{""}
   *[other]{"ы"}
}

-wound-plural-modifier-patch = { $count ->
    [one]{"область"}
   *[other]{"области"}
}

wound-incision-examine = [color=yellow]открытые разрезы[/color]
wound-ribcage-open-examine = [color=yellow]открытую грудную клетку[/color]

wound-bruise-80 = [color=crimson]ужасные гематомы[/color]
wound-bruise-50 = [color=crimson]большие гематомы[color]
wound-bruise-30 = [color=red]серьёзные ушибы[/color]
wound-bruise-20 = [color=red]значительныe ушибы[/color]
wound-bruise-10 = [color=orange]слабыe ушибы[/color]
wound-bruise-5 = [color=yellow]несколько синяков[/color]

wound-cut-massive-45 = [color=crimson]глубокие резаные раны[/color]
wound-cut-massive-35 = [color=crimson]резаные раны[/color]
wound-cut-massive-25 = [color=crimson]серьёзные порезы[/color]
wound-cut-massive-10 = [color=red]объёмные кровавые корки[/color]
wound-cut-massive-0 = [color=orange]массивные шрамы[/color]

wound-cut-severe-25 = [color=crimson]резаные раны[/color]
wound-cut-severe-15 = [color=red]значительные порезы[/color]
wound-cut-severe-10 = [color=red]заживающие порезы[color]
wound-cut-severe-5 = [color=orange]кровавые корки[color]
wound-cut-severe-0 = [color=yellow]шрамы[/color]

wound-cut-moderate-15 = [color=red]разрезы[color]
wound-cut-moderate-10 = [color=orange]умеренные порезы[/color]
wound-cut-moderate-5 = [color=yellow]порезы[/color]
wound-cut-moderate-0 = [color=yellow]бледнеющие шрамы[/color]

wound-cut-small-7 = [color=orange]лёгкие разрезы[color]
wound-cut-small-3 = [color=yellow]слабые порезы[/color]
wound-cut-small-0 = [color=yellow]бледный шрам[/color]

wound-puncture-massive-45 = [color=crimson]рваные, зияющие дыры[color]
wound-puncture-massive-35 = [color=crimson]рваные, глубокие проколы[/color]
wound-puncture-massive-25 = [color=crimson]массивные проколы[/color]
wound-puncture-massive-10 = [color=red]круглые кровавые корки[/color]
wound-puncture-massive-0 = [color=orange]массивные круглые шрамы[/color]

wound-puncture-severe-25 = [color=crimson]рваные проколы[/color]
wound-puncture-severe-15 = [color=red]значительные проколы[/color]
wound-puncture-severe-10 = [color=red]заживающие проколы[/color]
wound-puncture-severe-5 = [color=orange]круглые кровавые корки[/color]
wound-puncture-severe-0 = [color=yellow]круглые шрамы[/color]

wound-puncture-moderate-15 = [color=red]глубокие колотые ранения[/color]
wound-puncture-moderate-10 = [color=orange]колотые ранения[/color]
wound-puncture-moderate-5 = [color=yellow]круглые кровавые корки[color]
wound-puncture-moderate-0 = [color=yellow]бледнеющие круглые шрамы[/color]

wound-puncture-small-7 = [color=orange]слабые рваные проколы[/color]
wound-puncture-small-3 = [color=yellow]слабые уколы[/color]
wound-puncture-small-0 = [color=yellow]бледные круглые шрамы[/color]

wound-heat-carbonized-40 = [color=crimson]серьёзные обугленные ожоги[/color]
wound-heat-carbonized-25 = [color=crimson]обугленные ожоги[/color]
wound-heat-carbonized-10 = [color=red]заживающие обугленные ожоги[/color]
wound-heat-carbonized-0 = [color=red]серьёзные послеожоговые шрамы[/color]

wound-heat-severe-25 = [color=crimson]ужасные ожоги[/color]
wound-heat-severe-15 = [color=red]серьёзные ожоги[/color]
wound-heat-severe-10 = [color=red]заживающие серьёзные ожоги[/color]
wound-heat-severe-5 = [color=orange]заживающие ожоги[/color]
wound-heat-severe-0 = [color=orange]послеожоговые шрамы[/color]

wound-heat-moderate-15 = [color=red]серьёзные ожоги[/color]
wound-heat-moderate-10 = [color=red]ожоги[/color]
wound-heat-moderate-5 = [color=orange]заживающие ожоги[/color]
wound-heat-moderate-0 = [color=orange]бледнеющие послеожоговые шрамы[/color]

wound-heat-small-7 = [color=orange]свежие слабые ожоги[/color]
wound-heat-small-3 = [color=orange]слабые ожоги[/color]
wound-heat-small-0 = [color=yellow]бледые послеожоговые шрамы[/color]

wound-cold-petrified-45 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} некротических, заледенелых обморожений[/color]
wound-cold-petrified-35 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} тёмных, заледенелых обморожений[/color]
wound-cold-petrified-25 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} заледенелых обморожений[/color]
wound-cold-petrified-10 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} покрасневших, оттаивающих обморожений[/color]
wound-cold-petrified-0 = [color=lightblue]массивные шрамы от обморожения[/color]

wound-cold-severe-25 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} значительных обморожений, покрытых волдырями[/color]
wound-cold-severe-15 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} значительных обморожений[/color]
wound-cold-severe-10 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} заживающих обморожений[/color]
wound-cold-severe-5 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} оттаивающих обморожений[/color]
wound-cold-severe-0 = [color=lightblue]шрамы от обморожения[/color]

wound-cold-moderate-15 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} умеренных обморожений, покрытых волдырями[/color]
wound-cold-moderate-10 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} умеренных обморожений[/color]
wound-cold-moderate-5 = [color=lightblue]{-wound-plural-modifier-patch(count: $count)} оттаивающих обморожений[/color]
wound-cold-moderate-0 = [color=lightblue]бледнеющих красных областей[/color]

wound-cold-small-7 = [color=lightblue]покрасневшие слабые обморожения[/color]
wound-cold-small-3 = [color=lightblue]слабые обморожения[/color]
wound-cold-small-0 = [color=lightblue]замёрзжие области[color]

wound-caustic-sloughing-45 = [color=yellowgreen]некротические, плавяющиеся химические ожоги[/color]
wound-caustic-sloughing-35 = [color=yellowgreen]плавящиеся химические ожоги, покрытые волдырями[/color]
wound-caustic-sloughing-25 = [color=yellowgreen]шелушащиеся химические ожоги[/color]
wound-caustic-sloughing-10 = [color=yellowgreen]отслаивающиеся химические ожоги[/color]
wound-caustic-sloughing-0 = [color=yellowgreen]химическе шрамы, покрытые волдырями[/color]

wound-caustic-severe-25 = [color=yellowgreen]blistering, серьёзные химические ожоги[/color]
wound-caustic-severe-15 = [color=yellowgreen]серьёзные химические ожоги[/color]
wound-caustic-severe-10 = [color=yellowgreen]заживающие химические ожоги[/color]
wound-caustic-severe-5 = [color=yellowgreen]{-wound-plural-modifier-patch(count: $count)} обесцвеченной воспалённой кожи[/color]
wound-caustic-severe-0 = [color=yellowgreen]шрамы с волдырями[/color]

wound-caustic-moderate-15 = [color=yellowgreen]умеренные химические ожоги, покрытые волдырями[/color]
wound-caustic-moderate-10 = [color=yellowgreen]умеренные химические ожоги[/color]
wound-caustic-moderate-5 = [color=yellowgreen]{-wound-plural-modifier-patch(count: $count)} воспалённой кожи[/color]
wound-caustic-moderate-0 = [color=yellowgreen]раздражённые шрамы[/color]

wound-caustic-small-7 = [color=yellowgreen]blistered химические ожоги[/color]
wound-caustic-small-3 = [color=yellowgreen]слабые химические ожоги[/color]
wound-caustic-small-0 = [color=yellowgreen]обесцвеченные ожоги[/color]

wound-shock-exploded-45 = [color=lightgoldenrodyellow]обугленные взрывные электрические ожоги[/color]
wound-shock-exploded-35 = [color=lightgoldenrodyellow]обугленные взрывные электрические ожоги[/color]
wound-shock-exploded-25 = [color=lightgoldenrodyellow]взрывные электрические ожоги[/color]
wound-shock-exploded-10 = [color=lightgoldenrodyellow]заживающие электрические ожоги[/color]
wound-shock-exploded-0 = [color=lightgoldenrodyellow]массивные электрические шрамы[/color]

wound-shock-severe-25 = [color=lightgoldenrodyellow]тлеющие серьёзные электрические ожоги[/color]
wound-shock-severe-15 = [color=lightgoldenrodyellow]серьёзные электрические ожоги[/color]
wound-shock-severe-10 = [color=lightgoldenrodyellow]заживающие электрические ожоги[/color]
wound-shock-severe-5 = [color=lightgoldenrodyellow]бледнеющие электрические ожоги[/color]
wound-shock-severe-0 = [color=lightgoldenrodyellow]электрические шрамы[/color]

wound-shock-moderate-15 = [color=lightgoldenrodyellow]mildly обугленные электрические ожоги[/color]
wound-shock-moderate-10 = [color=lightgoldenrodyellow]умеренные электрические ожоги[/color]
wound-shock-moderate-5 = [color=lightgoldenrodyellow]бледнеющие электрические ожоги[/color]
wound-shock-moderate-0 = [color=lightgoldenrodyellow]небольшие волдыри[/color]

wound-shock-small-7 = [color=lightgoldenrodyellow]электрические ожоги[/color]
wound-shock-small-3 = [color=lightgoldenrodyellow]слабые электрические ожоги[/color]
wound-shock-small-0 = [color=lightgoldenrodyellow]бледнеющие электрические ожоги[/color]
