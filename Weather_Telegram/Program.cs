//Стешкин Георгий, ИП-19-3 (Прогноз погоды в телеграмм-боте)
using Newtonsoft.Json;
using Weather;
using Weather_Telegram;

var APIkey_OpenWeather = "0c5866bb0a95c5760159e404f48499cc";
var TelegaToken = "5966234609:AAFxDnHalGX22-zNIZ2beR3EK26qgD4-JpE";
var Offset = 0;
var client = new HttpClient();

var Message_telegramBot = "";
var ID_telegramBot = 0;
var ResponseToUSer = "";

while(true)
{
    var response_Telegram = await client.GetAsync($"https://api.telegram.org/bot{TelegaToken}/getUpdates?offset={Offset}");

    var result = await response_Telegram.Content.ReadAsStringAsync();
    var model = JsonConvert.DeserializeObject<telega_bot>(result);

    if (model.result.Length > 0)
    {
        Offset = model.result[^1].update_id + 1;
    }

    if (response_Telegram.IsSuccessStatusCode)
    {
        foreach (var m in model.result)
        {
            Message_telegramBot = m.message.text;
            ID_telegramBot = m.message.chat.id;

            if(Message_telegramBot == "/start")
            {
                ResponseToUSer = $"Привет, напиши команду /weather, чтобы узнать подробности.";

                var responseToUser = await client.GetAsync($"https://api.telegram.org/bot{TelegaToken}/sendMessage?chat_id={m.message.chat.id}&text={ResponseToUSer}");
            }
            else
            {
                if(Message_telegramBot == "/weather")
                {
                    ResponseToUSer = "Введите название города для того, чтобы получить данные о погоде в настоящее время.";

                    var responseToUser = await client.GetAsync($"https://api.telegram.org/bot{TelegaToken}/sendMessage?chat_id={m.message.chat.id}&text={ResponseToUSer}");
                }
                else
                {
                    var response_OpenWeatherMap = await client.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={Message_telegramBot}&appid={APIkey_OpenWeather}&lang=ru&units=metric");
                    if (response_OpenWeatherMap.IsSuccessStatusCode)
                    {
                        var resultOpenWeather = await response_OpenWeatherMap.Content.ReadAsStringAsync();
                        var modelOpenWeather = JsonConvert.DeserializeObject<WeatherJson>(resultOpenWeather);

                        ResponseToUSer = $"Прогноз погоды на {DateTime.Now} для города {modelOpenWeather.name}:" +
                        $"\nТекущая температура {modelOpenWeather.main.temp}°, {modelOpenWeather.weather[0].description}, ощущается как {modelOpenWeather.main.feels_like}°" +
                        $"\nСкорость ветра {modelOpenWeather.wind.speed} м/с, {WeatherDegToString(modelOpenWeather.wind.deg)}, влажность {modelOpenWeather.main.humidity}%, давление {modelOpenWeather.main.pressure} мм рт. ст.";

                        var responseToUser = await client.GetAsync($"https://api.telegram.org/bot{TelegaToken}/sendMessage?chat_id={m.message.chat.id}&text={ResponseToUSer}");
                    }
                    else
                    {
                        ResponseToUSer = "Извини, название города указано неверно или такого города не существует, попробуй еще раз.";
                        var responseToUser = await client.GetAsync($"https://api.telegram.org/bot{TelegaToken}/sendMessage?chat_id={m.message.chat.id}&text={ResponseToUSer}");
                    }
                }
            }
        }
    }
}
string WeatherDegToString(int WindDeg)
{
    string str = "";
    if (WindDeg <= 15 && WindDeg >= 345)
    {
        str = "С";
    }
    if (WindDeg > 15 && WindDeg < 75)
    {
        str = "СВ";
    }
    if (WindDeg >= 75 && WindDeg <= 105)
    {
        str = "В";
    }
    if (WindDeg > 105 && WindDeg < 165)
    {
        str = "ЮВ";
    }
    if (WindDeg >= 165 && WindDeg <= 195)
    {
        str = "Ю";
    }
    if (WindDeg > 195 && WindDeg < 255)
    {
        str = "ЮЗ";
    }
    if (WindDeg >= 255 && WindDeg <= 285)
    {
        str = "З";
    }
    if (WindDeg > 285 && WindDeg < 345)
    {
        str = "СЗ";
    }
    return str;
}
