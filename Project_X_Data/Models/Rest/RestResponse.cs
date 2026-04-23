using System.Security.Cryptography.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace Project_X_Data.Models.Rest
{
    public class RestResponse
    {
        public RestStatus Status { get; set; } = new();
        public RestMeta Meta { get; set; } = new();
        public Object? Data { get; set; }
    }



    //        REST(Representational State Transfer) — набір архітектурних вимог/принципів,
    //реалізація яких покращує роботу розподілених систем.*
    //API (Application Program Interface) — інтерфейс взаємодії Програми з своїми
    //Застосунками*
    //у цьому контексті Програма — інформаційний центр, зазвичай бекенд
    //Застосунок — самостійна частина, зазвичай клієнтського призначення,
    //яка взаємодіє з Програмою шляхом обміну даними.
    //(* також Додаток — несамостійна програма — плагін або розширення*)
    //*/
}
