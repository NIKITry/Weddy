using System.Text;

namespace Weddy.UI.Invitation.Pages;

public static class InvitationPage
{
    public static string GetHtml(string token, string baseUrl)
    {
        var version = DateTime.UtcNow.Ticks;
        var fontVersion = DateTime.UtcNow.Ticks;
        return $@"<!DOCTYPE html>
<html lang=""ru"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""Cache-Control"" content=""no-cache, no-store, must-revalidate"">
    <meta http-equiv=""Pragma"" content=""no-cache"">
    <meta http-equiv=""Expires"" content=""0"">
    <title>Свадебное приглашение</title>
    <script src=""https://cdn.tailwindcss.com?v={version}""></script>
    <script defer src=""https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js?v={version}""></script>
    <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
    <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
    <link rel=""dns-prefetch"" href=""https://fonts.googleapis.com"">
    <link rel=""dns-prefetch"" href=""https://fonts.gstatic.com"">
    <link href=""https://fonts.googleapis.com/css2?family=Great+Vibes&family=Allura&family=Cormorant+Garamond:ital,wght@0,300;0,400;0,500;0,600;0,700;1,300;1,400;1,500;1,600&display=swap&v={fontVersion}"" rel=""stylesheet"" media=""print"" onload=""this.media='all'"">
    <noscript><link href=""https://fonts.googleapis.com/css2?family=Great+Vibes&family=Allura&family=Cormorant+Garamond:ital,wght@0,300;0,400;0,500;0,600;0,700;1,300;1,400;1,500;1,600&display=swap&v={fontVersion}"" rel=""stylesheet""></noscript>
    <style>
        .title-font {{ font-family: 'Great Vibes', cursive !important; }}
        .names-font {{ font-family: 'Great Vibes', cursive !important; }}
        .date-font {{ font-family: 'Cormorant Garamond', serif !important; font-variant: small-caps; }}
        .text-font {{ font-family: 'Cormorant Garamond', serif !important; }}
        .plan-font {{ font-family: 'Cormorant Garamond', serif !important; }}
        .button-font {{ font-family: 'Cormorant Garamond', serif !important; }}
        html, body {{ 
            height: 100%; 
            margin: 0; 
            padding: 0; 
            background: #faf8f5;
        }}
        .animated-background {{
            position: fixed;
            inset: 0;
            width: 100%;
            height: 100%;
            background-image: url('/images/backgrounds/wedding-background.jpg');
            background-size: cover;
            background-position: center center;
            background-repeat: no-repeat;
            opacity: 0.7;
            z-index: 0;
            animation: slowMove 60s ease-in-out infinite;
            pointer-events: none;
            will-change: transform;
        }}
        .invitation-container {{
            height: 100vh;
            width: 100%;
            overflow-y: scroll;
            overflow-x: hidden;
            scroll-snap-type: y mandatory;
            scroll-behavior: smooth;
            position: relative;
            z-index: 1;
            background: transparent;
        }}
        @keyframes slowMove {{
            0%, 100% {{
                transform: translate(0, 0) scale(1);
            }}
            25% {{
                transform: translate(-2%, -1%) scale(1.02);
            }}
            50% {{
                transform: translate(-1%, -2%) scale(1.01);
            }}
            75% {{
                transform: translate(-2%, -1%) scale(1.02);
            }}
        }}
        .invitation-container::-webkit-scrollbar {{
            display: none;
        }}
        .invitation-container {{
            -ms-overflow-style: none;
            scrollbar-width: none;
        }}
        .invitation-screen {{
            min-height: 100vh;
            scroll-snap-align: start;
            scroll-snap-stop: always;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            padding: 40px 30px;
            position: relative;
            z-index: 1;
        }}
        @keyframes fadeInUp {{
            from {{
                opacity: 0;
                transform: translateY(30px);
            }}
            to {{
                opacity: 1;
                transform: translateY(0);
            }}
        }}
        @keyframes fadeIn {{
            from {{
                opacity: 0;
            }}
            to {{
                opacity: 1;
            }}
        }}
        .invitation-card {{ 
            position: relative;
            background: rgba(254, 252, 248, 0.75); 
            border: 1px solid rgba(232, 224, 214, 0.9); 
            box-shadow: 0 8px 32px rgba(0,0,0,0.12);
            overflow: hidden;
            width: 100%;
            max-width: 600px;
            backdrop-filter: blur(2px);
        }}
        .invitation-content {{ 
            position: relative; 
            z-index: 2; 
        }}
        .text-sage {{ color: #5a6b5a; }}
        .text-sage-dark {{ color: #3d4a3d; }}
        .btn-yes-active {{ background: linear-gradient(135deg, #d4c4b4 0%, #c4b4a4 100%); border: 2px solid #b4a494; color: #2d3a2d; }}
        .btn-yes-inactive {{ background: #fefcf8; border: 2px solid #d4c4b4; color: #2d3a2d; }}
        .btn-no-active {{ background: linear-gradient(135deg, #d4c4b4 0%, #c4b4a4 100%); border: 2px solid #b4a494; color: #2d3a2d; }}
        .btn-no-inactive {{ background: #fefcf8; border: 2px solid #d4c4b4; color: #2d3a2d; }}
        
        /* Адаптивность для мобильных устройств */
        @media (max-width: 768px) {{
            .invitation-screen {{
                padding: 20px 15px;
            }}
            .invitation-card {{
                max-width: 100%;
                margin: 0 10px;
            }}
            /* Адаптивные размеры шрифтов */
            .mobile-text-large {{ font-size: 2.5rem !important; }}
            .mobile-text-title {{ font-size: 2rem !important; letter-spacing: 1px !important; }}
            .mobile-text-medium {{ font-size: 1.5rem !important; }}
            .mobile-text-normal {{ font-size: 1rem !important; }}
            .mobile-text-small {{ font-size: 0.9rem !important; }}
            /* Адаптивные отступы */
            .mobile-padding {{ padding: 30px 20px !important; }}
            .mobile-padding-small {{ padding: 20px 15px !important; }}
            /* Адаптивные отступы для текста */
            .px-8 {{ padding-left: 1rem !important; padding-right: 1rem !important; }}
            .py-8 {{ padding-top: 1rem !important; padding-bottom: 1rem !important; }}
            .mb-10 {{ margin-bottom: 2rem !important; }}
        }}
        @media (max-width: 480px) {{
            .invitation-screen {{
                padding: 15px 10px;
            }}
            .invitation-card {{
                margin: 0 5px;
            }}
            .mobile-text-large {{ font-size: 2rem !important; }}
            .mobile-text-title {{ font-size: 1.75rem !important; letter-spacing: 0.5px !important; }}
            .mobile-text-medium {{ font-size: 1.25rem !important; }}
            .mobile-text-normal {{ font-size: 0.95rem !important; }}
            .mobile-text-small {{ font-size: 0.85rem !important; }}
            .mobile-padding {{ padding: 25px 15px !important; }}
            .mobile-padding-small {{ padding: 15px 10px !important; }}
            /* Адаптивные отступы для текста на маленьких экранах */
            .px-8 {{ padding-left: 0.75rem !important; padding-right: 0.75rem !important; }}
            .py-8 {{ padding-top: 0.75rem !important; padding-bottom: 0.75rem !important; }}
            .mb-10 {{ margin-bottom: 1.5rem !important; }}
            /* Уменьшаем gap для кнопок */
            .grid {{ gap: 0.5rem !important; }}
        }}
    </style>
</head>
<body style=""background: #faf8f5; padding: 0; margin: 0; height: 100vh;"">
    <div class=""animated-background""></div>
    <div class=""invitation-container"" x-data=""invitationApp()"" x-init=""loadInvitation()"">
        <!-- Loading -->
        <div x-show=""loading"" class=""invitation-screen"" style=""background: transparent;"">
            <div class=""text-center"">
                <div class=""inline-block animate-spin rounded-full h-12 w-12 border-b-2"" style=""border-color: #8fa68f;""></div>
                <p class=""mt-4 text-font text-sage"" style=""font-size: 1.125rem;"">Загрузка приглашения...</p>
            </div>
        </div>

        <!-- Error -->
        <div x-show=""error"" class=""invitation-screen"" style=""background: transparent;"">
            <div class=""invitation-card"" style=""padding: 60px 40px;"">
                <div class=""invitation-content"">
                    <div class=""text-center"">
                        <p class=""text-font text-sage-dark"" style=""font-size: 1.125rem; font-weight: 400;"" x-text=""error""></p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Screen 1: Names (кому приглашение) -->
        <div x-show=""!loading && !error && invitation"" class=""invitation-screen"">
            <div class=""invitation-card mobile-padding"" style=""padding: 60px 40px; animation: fadeInUp 1s ease-out;"">
                <div class=""invitation-content"">
                    <div class=""text-center"">
                        <div class=""names-font mobile-text-large"" style=""color: #2d3a2d; font-weight: 200; line-height: 1.2; animation: fadeInUp 0.8s ease-out; font-size: 5rem; word-wrap: break-word; overflow-wrap: break-word;"" x-text=""invitation && invitation.displayName ? invitation.displayName : ''""></div>
                    </div>
                </div>
            </div>
            <div class=""mt-8 text-center"" style=""animation: fadeIn 1s ease-out 0.5s both;"">
                <div class=""inline-block"" style=""width: 30px; height: 4px; background: #8fa68f; border-radius: 2px; opacity: 0.6;""></div>
            </div>
        </div>

        <!-- Screen 2: Date and Event Plan -->
        <div x-show=""!loading && !error && invitation"" class=""invitation-screen"">
            <div class=""invitation-card mobile-padding"" style=""padding: 60px 40px; animation: fadeInUp 1s ease-out;"">
                <div class=""invitation-content"">
                    <!-- Date -->
                    <div x-show=""invitation && invitation.event && invitation.event.eventDate"" class=""text-center mb-10"" style=""animation: fadeInUp 0.8s ease-out;"">
                        <div class=""title-font text-3xl mobile-text-medium"" style=""color: #2d3a2d; letter-spacing: 1px; font-weight: 300; font-size: calc(1.875rem + 4pt);"" x-text=""invitation && invitation.event && invitation.event.eventDate ? formatDate(invitation.event.eventDate) : ''""></div>
                    </div>

                    <!-- Invitation Text -->
                    <div x-show=""invitation && invitation.invitationText && invitation.invitationText.trim()"" 
                         class=""px-8 py-8 mb-10"" 
                         style=""animation: fadeInUp 0.8s ease-out 0.2s both; padding-left: 2rem; padding-right: 2rem; padding-top: 2rem; padding-bottom: 2rem;"">
                        <div class=""text-font leading-relaxed whitespace-pre-line text-center mobile-text-normal"" 
                             style=""color: #2d3a2d; font-size: 1.25rem; line-height: 1.8; letter-spacing: 0.3px; font-weight: 400; word-wrap: break-word; overflow-wrap: break-word;"" 
                             x-text=""invitation.invitationText""></div>
                    </div>

                     <!-- Event Plan -->
                     <div x-show=""invitation.event && invitation.event.eventPlan && invitation.event.eventPlan.length > 0"" 
                          class=""px-8 py-8"" 
                          style=""animation: fadeInUp 0.8s ease-out 0.4s both; padding-left: 1rem; padding-right: 1rem; padding-top: 1rem; padding-bottom: 1rem;"">
                         <div class=""text-center mb-6"" style=""animation: fadeInUp 0.8s ease-out 0.4s both;"">
                             <h3 class=""title-font mobile-text-medium"" style=""color: #2d3a2d; font-weight: 300; letter-spacing: 1px; font-size: 2.5rem; margin-bottom: 1.5rem;"">План мероприятия</h3>
                         </div>
                         <div class=""plan-font leading-relaxed text-center"" 
                              style=""color: #2d3a2d; font-size: 1.125rem; letter-spacing: 0.2px; font-weight: 400;"">
                             <template x-for=""(item, index) in invitation.event.eventPlan"" :key=""index"">
                                <div class=""mb-6"" :style=""'animation: fadeInUp 0.6s ease-out ' + (0.6 + index * 0.1) + 's both;'"">
                                    <div class=""mb-2 mobile-text-normal"" style=""font-weight: 600; font-size: 1.125rem;"" x-text=""item.time""></div>
                                    <div class=""mb-1 mobile-text-normal"" style=""font-size: 1.125rem;"" x-text=""item.title""></div>
                                    <div class=""opacity-80 mobile-text-small"" style=""font-size: 1rem; word-wrap: break-word; overflow-wrap: break-word;"" x-text=""item.location""></div>
                                </div>
                            </template>
                        </div>
                    </div>
                </div>
            </div>
            <div class=""mt-8 text-center"" style=""animation: fadeIn 1s ease-out 0.5s both;"">
                <div class=""inline-block"" style=""width: 30px; height: 4px; background: #8fa68f; border-radius: 2px; opacity: 0.6;""></div>
            </div>
        </div>

        <!-- Screen 3: RSVP Form -->
        <div x-show=""!loading && !error && invitation"" class=""invitation-screen"">
            <div class=""invitation-card mobile-padding"" style=""padding: 60px 40px; animation: fadeInUp 1s ease-out;"">
                <div class=""invitation-content"">

            <!-- RSVP Section -->
            <div class=""px-6 pb-8"" style=""animation: fadeInUp 0.8s ease-out; padding-left: 1.5rem; padding-right: 1.5rem; padding-bottom: 2rem;"">
                <h2 class=""title-font text-4xl mb-10 text-center mobile-text-medium"" style=""color: #2d3a2d; font-weight: 300; letter-spacing: 1px; margin-bottom: 2.5rem;"">Ваш ответ</h2>
                
                <!-- RSVP Status Summary (shown when answer exists and was saved) -->
                <div x-show=""invitation && invitation.status && String(invitation.status).toLowerCase() !== 'none' && hasSavedResponse && !editingResponse"" 
                     class=""space-y-6 max-w-md mx-auto"">
                    <div class=""px-6 py-6 text-center text-font"" 
                         style=""font-size: 1.125rem; background-color: #fefcf8; border: 2px solid #e8e0d6; color: #2d3a2d; font-weight: 400; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.05);"">
                        <div class=""mb-4"">
                            <div class=""title-font text-2xl"" style=""color: #2d3a2d; font-weight: 300; letter-spacing: 0.5px;"" x-text=""invitation && invitation.status ? getStatusText(invitation.status) : ''""></div>
                        </div>
                        <div x-show=""invitation && invitation.note && invitation.note.trim()"" class=""mt-4 pt-4"" style=""border-top: 1px solid #e8e0d6;"">
                            <div class=""text-font text-sm opacity-70 mb-1"" style=""font-weight: 400; letter-spacing: 0.2px;"">Примечание:</div>
                            <div class=""text-font text-base"" style=""font-weight: 400; letter-spacing: 0.2px;"" x-text=""invitation && invitation.note ? invitation.note : ''""></div>
                        </div>
                        <button 
                            @click=""editingResponse = true""
                            class=""mt-6 px-6 py-3 button-font text-base transition-all tracking-wide hover:opacity-90""
                            style=""background: linear-gradient(135deg, #d4c4b4 0%, #c4b4a4 100%); border: 2px solid #b4a494; color: #2d3a2d; font-weight: 600; letter-spacing: 0.3px; border-radius: 6px;"">
                            Изменить ответ
                        </button>
                    </div>
                </div>

                <!-- RSVP Form (shown when no answer saved or editing) -->
                <div x-show=""!invitation || !invitation.status || (invitation.status && String(invitation.status).toLowerCase() === 'none') || !hasSavedResponse || editingResponse"" 
                     class=""space-y-6 max-w-md mx-auto"">
                    <!-- Status Buttons -->
                    <div class=""grid grid-cols-2 gap-3"" style=""gap: 0.75rem;"">
                        <button 
                            @click=""setStatus('yes')""
                            :class=""invitation && String(invitation.status).toLowerCase() === 'yes' ? 'btn-yes-active' : 'btn-yes-inactive'""
                            class=""py-4 px-3 button-font transition-all hover:opacity-90 mobile-text-normal"" 
                            style=""font-size: 1.125rem; letter-spacing: 0.3px; font-weight: 600; padding-top: 1rem; padding-bottom: 1rem; padding-left: 0.75rem; padding-right: 0.75rem;"">
                            Приду
                        </button>
                        <button 
                            @click=""setStatus('no')""
                            :class=""invitation && String(invitation.status).toLowerCase() === 'no' ? 'btn-no-active' : 'btn-no-inactive'""
                            class=""py-4 px-3 button-font transition-all hover:opacity-90 mobile-text-normal""
                            style=""font-size: 1.125rem; letter-spacing: 0.3px; font-weight: 600; padding-top: 1rem; padding-bottom: 1rem; padding-left: 0.75rem; padding-right: 0.75rem;"">
                            Не смогу
                        </button>
                    </div>

                    <!-- Note -->
                    <div>
                        <label class=""block text-font mb-2 text-center"" style=""color: #2d3a2d; font-size: 1rem; font-weight: 400; letter-spacing: 0.2px;"">
                            Примечание (необязательно)
                        </label>
                        <textarea 
                            x-model=""note""
                            class=""w-full p-4 text-font focus:ring-2 focus:ring-sage-300 mobile-text-normal""
                            rows=""3""
                            placeholder=""Например: Придём к 16:30, с детьми""
                            style=""font-size: 1.125rem; resize: none; border: 2px solid #d4c4b4; background-color: #fefcf8; color: #2d3a2d; font-weight: 400; cursor: text; padding: 1rem; word-wrap: break-word; overflow-wrap: break-word;"" 
                            tabindex=""0""></textarea>
                    </div>

                    <!-- Submit Button -->
                    <button 
                        @click=""submitRSVP()""
                        :disabled=""submitting || !invitation || !invitation.status || String(invitation.status).toLowerCase() === 'none'""
                        class=""w-full button-font py-4 px-6 disabled:opacity-50 disabled:cursor-not-allowed transition-all tracking-wide mobile-text-normal""
                        style=""font-size: 1.25rem; background: linear-gradient(135deg, #d4c4b4 0%, #c4b4a4 100%); border: 2px solid #b4a494; color: #2d3a2d; font-weight: 600; letter-spacing: 0.5px; padding-top: 1rem; padding-bottom: 1rem; padding-left: 1.5rem; padding-right: 1.5rem;"">
                        <span x-show=""!submitting"">Отправить ответ</span>
                        <span x-show=""submitting"">Отправка...</span>
                    </button>

                    <!-- Cancel Edit Button -->
                    <button 
                        x-show=""editingResponse""
                        @click=""editingResponse = false""
                        class=""w-full button-font py-3 px-6 transition-all tracking-wide""
                        style=""font-size: 1rem; background: #fefcf8; border: 2px solid #d4c4b4; color: #2d3a2d; font-weight: 600; letter-spacing: 0.3px; border-radius: 6px;"">
                        Отмена
                    </button>

                    <!-- Success Message -->
                    <div x-show=""success"" class=""px-6 py-4 text-center text-font"" style=""font-size: 1.125rem; background-color: #f0f5f0; border: 1px solid #c8d8c8; color: #5a6b5a; font-weight: 400;"">
                        Ваш ответ сохранен
                    </div>
                </div>
            </div>
            
            <!-- Couple Display Name -->
            <div x-show=""invitation && invitation.event && invitation.event.coupleDisplayName && String(invitation.event.coupleDisplayName).trim().length > 0"" 
                 class=""px-6 pt-8 mt-8"" 
                 style=""animation: fadeInUp 0.8s ease-out 0.5s both; padding-left: 1.5rem; padding-right: 1.5rem; padding-top: 2rem; margin-top: 2rem; border-top: 1px solid #e8e0d6;"">
                <div class=""title-font text-center mobile-text-medium"" 
                     style=""color: #2d3a2d; font-size: 2rem; line-height: 1.4; letter-spacing: 0.5px; font-weight: 300; word-wrap: break-word; overflow-wrap: break-word;"">
                    <div x-text=""invitation && invitation.event && invitation.event.coupleDisplayName ? 'Ждем вас!' : ''""></div>
                    <div x-text=""invitation && invitation.event && invitation.event.coupleDisplayName ? ('Ваши ' + String(invitation.event.coupleDisplayName)) : ''""></div>
                </div>
            </div>
            
            <!-- Footer Note -->
            <div x-show=""invitation && invitation.event && invitation.event.footerNote && String(invitation.event.footerNote).trim().length > 0"" 
                 class=""px-6 pt-8 mt-8"" 
                 style=""animation: fadeInUp 0.8s ease-out 0.6s both; padding-left: 1.5rem; padding-right: 1.5rem; padding-top: 2rem; margin-top: 2rem; border-top: 1px solid #e8e0d6;"">
                <div class=""text-font text-center mobile-text-small"" 
                     style=""color: #2d3a2d; font-size: 0.95rem; line-height: 1.6; letter-spacing: 0.2px; font-weight: 400; opacity: 0.8; word-wrap: break-word; overflow-wrap: break-word;"" 
                     x-text=""invitation && invitation.event && invitation.event.footerNote ? String(invitation.event.footerNote) : ''""></div>
            </div>
                </div>
            </div>
        </div>
        </div>

    <script>
        function invitationApp() {{
            return {{
                invitation: null,
                note: '',
                loading: true,
                error: null,
                submitting: false,
                success: false,
                editingResponse: false,
                hasSavedResponse: false, // Флаг, что ответ был сохранен на сервере
                token: '{token}',

                async loadInvitation() {{
                    try {{
                        const apiUrl = `/api/public/invitations/${{this.token}}`;
                        const response = await fetch(apiUrl);
                        if (!response.ok) {{
                            if (response.status === 404) {{
                                this.error = 'Приглашение не найдено или было удалено';
                            }} else if (response.status === 500) {{
                                this.error = 'Ошибка сервера. Пожалуйста, попробуйте позже';
                            }} else {{
                                this.error = `Ошибка загрузки приглашения (код: ${{response.status}})`;
                            }}
                            this.loading = false;
                            return;
                        }}
                        const data = await response.json();
                        this.invitation = data;
                        this.note = data.note || '';
                        // Если уже есть ответ, не показываем форму редактирования сразу
                        this.editingResponse = false;
                        // Устанавливаем флаг, что ответ был сохранен, если статус не 'none'
                        this.hasSavedResponse = data.status && String(data.status).toLowerCase() !== 'none';
                        this.loading = false;
                    }} catch (error) {{
                        console.error('Error loading invitation:', error);
                        this.error = 'Ошибка подключения к серверу. Проверьте подключение к интернету';
                        this.loading = false;
                    }}
                }},

                formatDate(dateString) {{
                    if (!dateString) return '';
                    const date = new Date(dateString);
                    const day = String(date.getDate()).padStart(2, '0');
                    const month = String(date.getMonth() + 1).padStart(2, '0');
                    const year = date.getFullYear();
                    return `${{day}}/${{month}}/${{year}}`;
                }},

                formatEventPlan(plan) {{
                    if (!plan) return '';
                    // Форматируем план мероприятия: время делаем полужирным
                    return plan.split('\\n').map(line => {{
                        // Если строка начинается с времени (формат: 16:00 или 18:00 -)
                        const timeMatch = line.match(/^(\\d{{1,2}}:\\d{{2}})/);
                        if (timeMatch) {{
                            const time = timeMatch[1];
                            const rest = line.substring(timeMatch[0].length);
                            return `<span style=""font-weight: 600;"">${{time}}</span>${{rest}}`;
                        }}
                        return line;
                    }}).join('<br>');
                }},

                getStatusText(status) {{
                    if (!status) return 'Нет ответа';
                    const statusLower = String(status).toLowerCase();
                    const statuses = {{
                        'yes': 'Вы придёте',
                        'no': 'Вы не сможете прийти',
                        'none': 'Нет ответа'
                    }};
                    return statuses[statusLower] || 'Нет ответа';
                }},

                setStatus(status) {{
                    if (!this.invitation) return;
                    this.invitation.status = status;
                    this.success = false;
                    // Сбрасываем флаг сохранения, так как статус изменился локально
                    this.hasSavedResponse = false;
                }},

                async submitRSVP() {{
                    if (!this.invitation || !this.invitation.status || String(this.invitation.status).toLowerCase() === 'none') {{
                        alert('Пожалуйста, выберите статус');
                        return;
                    }}
                    this.submitting = true;
                    this.success = false;
                    try {{
                        const apiUrl = `/api/public/invitations/${{this.token}}/rsvp`;
                        const response = await fetch(apiUrl, {{
                            method: 'PUT',
                            headers: {{
                                'Content-Type': 'application/json'
                            }},
                            body: JSON.stringify({{
                                status: this.invitation.status,
                                note: this.note || null
                            }})
                        }});
                        if (response.ok) {{
                            this.success = true;
                            this.editingResponse = false;
                            // Устанавливаем флаг, что ответ был сохранен
                            this.hasSavedResponse = true;
                            // Обновляем данные приглашения
                            await this.loadInvitation();
                            setTimeout(() => this.success = false, 3000);
                        }} else {{
                            throw new Error('Ошибка сохранения');
                        }}
                    }} catch (error) {{
                        alert('Ошибка отправки ответа. Попробуйте еще раз.');
                    }} finally {{
                        this.submitting = false;
                    }}
                }}
            }}
        }}
    </script>
</body>
</html>";
    }
}

