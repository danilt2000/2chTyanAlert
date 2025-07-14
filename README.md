# Фильтр женских анкет с 2ch [/soc/](https://2ch.hk/soc/) → Отправка в [Telegram](https://t.me/SocTyan2chAlert)

<p align="center">
  <img width="1881" height="878" alt="uml diagram" src="https://github.com/user-attachments/assets/81807988-84c2-412f-89b3-8f5832ed425e" />
</p>

<p ><mark><strong>Цель проекта — автоматически просматривать раздел /soc/ на 2ch, вытаскивать тян-анкеты из общей массы постов, оценивать их через LLM и пересылать отобранные анкеты (с изображением и баллами) в Telegram-канал.</strong></mark></p>

<p align="center">
  <img width="575" height="950" alt="telegram example" src="https://github.com/user-attachments/assets/dd998c49-9ac3-4de6-be17-4ac05ffb1662" />
</p>

<p align="center">
  <img  alt="telegram example" src="https://github.com/user-attachments/assets/dbf7be98-41b8-4abf-b047-b80d9d60d48d" />
</p>

## Используемая модель: Gemini 2.5 Flash

**Цены (июль 2025):**

| Тип токенов        | Цена за 1M токенов       |
|--------------------|--------------------------|
| Вход (текст / изображение / видео) | **$0.30** |
| Выход (текст)      | **$2.50**                |

---

### Переменные окружения `.env`

- CatchRatePerDay=2
- TelegramBotToken=
- TelegramChatId=
- Gemini__ApiKey=

### Вы можете скачать образ докер через
```
docker pull ghcr.io/danilt2000/2chtyanalert:master
```

Yml конфиг 
```
2chtyanalert:
    image: ghcr.io/danilt2000/2chtyanalert:master
    restart: unless-stopped
    environment:
      - RUNNING_IN_DOCKER=true
      - CatchRatePerDay=2
      - TelegramBotToken=ВашТелеграмБотТокен
      - TelegramChatId=ВашЧатАЙДИ
      - Gemini__ApiKey=ВашGeminiКлючи
```
---

## Ссылки

* **Telegram-канал** — [подписаться](https://t.me/SocTyan2chAlert)
* **YouTube** — [смотреть](https://www.youtube.com/@hepatica42)
* **Twitch** — [смотреть](https://www.twitch.tv/hepatir)



