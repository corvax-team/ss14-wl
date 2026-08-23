structured-template-product-manufacturing-order =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: КОД-КОД[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                    ЗАКАЗ НА ПРОИЗВОДСТВО ПРОДУКТА
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, прошу произвести продукцию отделом %%field:product-manufacturing-order-department%%.
    Перечень необходимых продуктов:
    %%multiline:product-manufacturing-order-products%%

    Причина заказа:
    %%multiline:product-manufacturing-order-reason%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-order-purchase-resources-equipment =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: КОД-СНБ[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
            ЗАКАЗ НА ЗАКУПКУ РЕСУРСОВ, СНАРЯЖЕНИЯ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Перечень товаров для заказа:
    %%multiline:order-purchase-resources-equipment-items%%

    Место доставки товара: %%field:order-purchase-resources-equipment-delivery-place%%

    Причина:
    %%multiline:order-purchase-resources-equipment-reason%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-ordering-special-equipment =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: КОМ-ЦК[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                    ЗАКАЗ СПЕЦИАЛЬНОГО СНАРЯЖЕНИЯ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, прошу предоставить специальное снаряжение станции от Центрального Командования.
    Перечень запрашиваемого снаряжения:
    %%multiline:ordering-special-equipment-items%%

    Причина запроса:
    %%multiline:ordering-special-equipment-reason%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-order-purchase-weapons =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: СБ-СНБ[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                    ЗАКАЗ НА ЗАКУПКУ ВООРУЖЕНИЯ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, произвожу заказ через отдел Снабжения.
    Боевое оружие и (или) боевые приспособления:
    %%multiline:order-purchase-weapons-items%%
    Причина заказа:
    %%multiline:order-purchase-weapons-reason%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-certificate =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: КОМ-ПД[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                                        ГРАМОТА
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    %%field:certificate-recipient-name%%, в должности %%field:certificate-recipient-position%% награждается грамотой за следующие заслуги:
    %%multiline:certificate-merits%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-certificate-advanced-training =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: КОМ[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
    СВИДЕТЕЛЬСТВО О ПОВЫШЕНИИ КВАЛИФИКАЦИИ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, в должности главы отдела %%field:certificate-advanced-training-department%%, свидетельствую, что сотрудник успешно завершил образовательный курс и был аттестован.
    ФИО сотрудника: %%field:certificate-advanced-training-employee-name%%
    Должность сотрудника: %%field:certificate-advanced-training-employee-job%%
    Название курса: %%field:certificate-advanced-training-course%%
    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-certificate-offense =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: ПД-СБ[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                СВИДЕТЕЛЬСТВО О ПРАВОНАРУШЕНИИ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, свидетельствую о правонарушениях/самолично признаюсь в совершении правонарушений, предусмотренных статьями:
    %%multiline:certificate-offense-articles%%
    По данному инциденту могу пояснить следующее.
    Место преступления: %%field:certificate-offense-crime-scene%%
    Мотивы совершения преступления: %%field:certificate-offense-motive%%
    Против кого было совершено преступление: %%field:certificate-offense-victim%%
    Характер и размер вреда, причинённого преступлением: %%field:certificate-offense-harm%%
    Пособники в преступлении: %%field:certificate-offense-accomplices%%
    Полная хронология событий:
    %%multiline:certificate-offense-chronology%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-death-certificate =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: МЕД[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                            СВИДЕТЕЛЬСТВО О СМЕРТИ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    ФИО умершего: %%field:death-certificate-deceased-name%%
    Должность умершего: %%field:death-certificate-deceased-job%%
    Раса: %%field:death-certificate-species%%
    Пол: %%field:death-certificate-sex%%
    Причина смерти:
    %%multiline:death-certificate-cause%%
    Возможность проведения реанимации или клонирования: %%field:death-certificate-revival-possibility%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-marriage-certificate =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: СРВ-ПД[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                СВИДЕТЕЛЬСТВО О ЗАКЛЮЧЕНИИ БРАКА
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, заключаю брак между:
    %%field:marriage-certificate-first-person-name%%, в должности %%field:marriage-certificate-first-person-job%%
    %%field:marriage-certificate-second-person-name%%, в должности %%field:marriage-certificate-second-person-job%%
    После заключения брака брачующимся были присвоены следующие полные имена:
    %%multiline:marriage-certificate-assigned-names%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-divorce-certificate =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: СРВ-ПД[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                СВИДЕТЕЛЬСТВО О РАСТОРЖЕНИИ БРАКА
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, расторгаю брак между:
    %%field:divorce-certificate-first-person-name%%, в должности %%field:divorce-certificate-first-person-job%%
    %%field:divorce-certificate-second-person-name%%, в должности %%field:divorce-certificate-second-person-job%%
    После расторжения брака бывшим супругам были присвоены следующие полные имена:
    %%multiline:divorce-certificate-assigned-names%%

    Разделение имущества было произведено следующим образом:
    %%multiline:divorce-certificate-property-division%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-closing-indictment =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: СБ[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                        ОБВИНИТЕЛЬНОЕ ЗАКЛЮЧЕНИЕ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, разрешаю произвести арест %%field:closing-indictment-suspect-name%%, в должности %%field:closing-indictment-suspect-job%% в связи с подозрением в совершении данным лицом следующих правонарушений:
    %%multiline:closing-indictment-offenses%%

    В ходе предварительного следствия были обнаружены доказательства, указывающие на совершение правонарушения данным лицом.
    Прямые доказательства:
    %%multiline:closing-indictment-direct-evidence%%

    Косвенные доказательства:
    %%multiline:closing-indictment-indirect-evidence%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-sentence =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: СБ[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                                    ПРИГОВОР
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, выношу приговор согласно данным мне полномочиям в отношении %%field:sentence-defendant-name%%, в должности %%field:sentence-defendant-job%%.
    Данное лицо нарушило следующие статьи Корпоративного Закона:
    %%multiline:sentence-articles%%
    С учётом всех смягчающих и отягчающих обстоятельств, правовое наказание данного лица представлено в виде:
    %%multiline:sentence-legal-punishment%%
    Административное наказание:
    %%multiline:sentence-administrative-punishment%%
    Срок заключения под стражу отсчитывается с: %%field:sentence-custody-start-time%%
    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-judgment =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: ЮР[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                    СУДЕБНОЕ РЕШЕНИЕ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, выношу решение по судебному разбирательству в отношении %%field:judgment-defendant-name%%, в должности %%field:judgment-defendant-job%%.
    Предъявляемые правонарушения:
    %%multiline:judgment-alleged-offenses%%

    Решение приговора Службы Безопасности:
    %%multiline:judgment-security-sentence%%

    Проведённое до судебного разбирательства время ареста: %%field:judgment-pretrial-arrest-time%%

    Данное лицо нарушило следующие статьи Корпоративного Закона:
    %%multiline:judgment-articles%%
    С учётом всех смягчающих и отягчающих обстоятельств, правовое наказание данного лица представлено в виде:
    %%multiline:judgment-legal-punishment%%
    Административное наказание:
    %%multiline:judgment-administrative-punishment%%
    Срок заключения под стражу отсчитывается с:
    %%field:judgment-custody-start-time%%
    Моё решение обосновано:
    %%multiline:judgment-reasoning%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-statement-health =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: МЕД-ПД[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
                    ЗАКЛЮЧЕНИЕ О СОСТОЯНИИ ЗДОРОВЬЯ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Пациент был направлен на медицинское обследование. Был произведён полный осмотр, проведены необходимые исследования и анализы.
    ФИО пациента: %%field:statement-health-patient-name%%
    Должность пациента: %%field:statement-health-patient-job%%
    Причина обследования:
    %%multiline:statement-health-examination-reason%%
    Состав врачебной комиссии:
    %%multiline:statement-health-medical-commission%%
    Состояние пациента при поступлении:
    %%multiline:statement-health-admission-condition%%

    Поставленный диагноз:
    %%multiline:statement-health-diagnosis%%

    Психологическое состояние пациента:
    %%multiline:statement-health-psychological-condition%%

    Оказанное лечение в ходе госпитализации:
    %%multiline:statement-health-treatment%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-decision-to-start-trial =
    ⠀[color=#1b487e]███░███░░░░██░░░░[/color]
    ⠀[color=#1b487e]░██░████░░░██░░░░[/color]      [head=3]Бланк документа[/head]
    ⠀[color=#1b487e]░░█░██░██░░██░█░░[/color]               [head=3]NanoTrasen[/head]
    ⠀[color=#1b487e]░░░░██░░██░██░██░[/color] [bold]:СТАНЦИЯ: ЮР[/bold]
    ⠀[color=#1b487e]░░░░██░░░████░███[/color]
    =============================================
            РЕШЕНИЕ О НАЧАЛЕ СУДЕБНОГО ПРОЦЕССА
    =============================================
    Время от начала смены и дата: :ДАТА:
    Составитель документа: :ФИО:
    Должность составителя: %%field:author-position=:ДОЛЖНОСТЬ:%%

    Я, :ФИО:, сообщаю о начале судебного разбирательства по делу %%field:decision-to-start-trial-defendant-name%% в связи со сложностью и неоднозначностью дела.
    Предъявляемые правонарушения:
    %%multiline:decision-to-start-trial-alleged-offenses%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-notice-of-liquidation =
    ⠀[color=#B50F1D] ███░██████░███[/color]
    ⠀[color=#B50F1D] █░░░██░░░░░░░█[/color]    [head=3]Бланк документа[/head]
    ⠀[color=#B50F1D] █░░░░████░░░░█[/color]             [head=3]Syndicate[/head]
    ⠀[color=#B50F1D] █░░░░░░░██░░░█[/color]   [bold]:СТАНЦИЯ: СИН-ПД[/bold]
    ⠀[color=#B50F1D] ███░██████░███[/color]
    =============================================
                        УВЕДОМЛЕНИЕ О ЛИКВИДАЦИИ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Позывной агента: %%field:notice-of-liquidation-agent-call-sign%%

    Уважаемый %%field:notice-of-liquidation-target-name%%, в должности %%field:notice-of-liquidation-target-job%%! Руководством Синдиката принято решение о вашей немедленной ликвидации в ходе данной смены. Просим заранее подготовить завещание и направить его Медицинскому отделу станции. Уничтожение вашего тела будет произведено силами Синдиката.
    Причина ликвидации:
    %%multiline:notice-of-liquidation-reason%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-business-deal =
    ⠀[color=#B50F1D] ███░██████░███[/color]
    ⠀[color=#B50F1D] █░░░██░░░░░░░█[/color]    [head=3]Бланк документа[/head]
    ⠀[color=#B50F1D] █░░░░████░░░░█[/color]             [head=3]Syndicate[/head]
    ⠀[color=#B50F1D] █░░░░░░░██░░░█[/color]   [bold]:СТАНЦИЯ: СИН-КОМ[/bold]
    ⠀[color=#B50F1D] ███░██████░███[/color]
    =============================================
                                ДЕЛОВАЯ СДЕЛКА
    =============================================
    Время от начала смены и дата: :ДАТА:
    Позывной агента: %%field:business-deal-agent-call-sign%%

    Синдикат любезно предлагает заключить сделку между станцией и агентом %%field:business-deal-agent-call-sign-reference%%. Со стороны станции необходимо:
    %%multiline:business-deal-station-obligations%%

    Причина выполнения условий сделки:
    %%multiline:business-deal-reason%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-note-beginning-military-actions =
    ⠀[color=#B50F1D] ███░██████░███[/color]
    ⠀[color=#B50F1D] █░░░██░░░░░░░█[/color]    [head=3]Бланк документа[/head]
    ⠀[color=#B50F1D] █░░░░████░░░░█[/color]             [head=3]Syndicate[/head]
    ⠀[color=#B50F1D] █░░░░░░░██░░░█[/color]   [bold]:СТАНЦИЯ: СИН[/bold]
    ⠀[color=#B50F1D] ███░██████░███[/color]
    =============================================
                    НОТА О НАЧАЛЕ ВОЕННЫХ ДЕЙСТВИЙ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Позывной агента: %%field:note-beginning-military-actions-agent-call-sign%%

    Неуважаемые корпоративные крысы NanoTrasen! Синдикат официально объявляет о начале военных действий с вами, а также о начале операции по вашему истреблению.
    Причина предъявления ноты:
    %%multiline:note-beginning-military-actions-reason%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
structured-template-report-accomplishment-goals =
    ⠀[color=#B50F1D] ███░██████░███[/color]
    ⠀[color=#B50F1D] █░░░██░░░░░░░█[/color]    [head=3]Бланк документа[/head]
    ⠀[color=#B50F1D] █░░░░████░░░░█[/color]             [head=3]Syndicate[/head]
    ⠀[color=#B50F1D] █░░░░░░░██░░░█[/color]   [bold]:СТАНЦИЯ: ПД-СИН[/bold]
    ⠀[color=#B50F1D] ███░██████░███[/color]
    =============================================
                        ОТЧЁТ О ВЫПОЛНЕНИИ ЦЕЛЕЙ
    =============================================
    Время от начала смены и дата: :ДАТА:
    Позывной агента: %%field:report-accomplishment-goals-agent-call-sign%%

    Я, :ФИО:, успешно выполнил поставленные передо мной руководством Синдиката цели. Прошу принять отчёт о выполнении.
    Отчёт:
    %%multiline:report-accomplishment-goals-report%%

    =============================================
    Подпись составителя: %%signature:author-signature%%
    { " " }[italic]Место для печатей[/italic]
