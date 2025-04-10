using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using TestTask.Models;

namespace TestTask.Controllers
{
    /// <summary>
    /// Контроллер для работы с рекламными данными.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MarketingController : Controller
    {
        /// <summary>
        /// Сообщение об ошибке при загрузке файла.
        /// </summary>
        private const string WrongFileMessage = "Ошибка при получении данных с загружаемого файла.";

        /// <summary>
        /// Сообщение о передаче пустой строки.
        /// </summary>
        private const string EmptyStringMessage = "Передана пустая строка";

        /// <summary>
        /// Расширение загружаемого файла.
        /// </summary>
        private const string AllowedFileExtention = ".txt";

        /// <summary>
        /// Площадки для заданной локации не найдены.
        /// </summary>
        private const string SpacesNotFoundMessage = "Площадки для заданной локации не найдены";

        /// <summary>
        /// Файл ещё не загружен на сервер.
        /// </summary>
        private const string FileNotLoadedMessage = "Файл ещё не загружен.";

        /// <summary>
        /// Сообщение об успешной загрузке файла.
        /// </summary>
        private const string FileAddedMessage = "Файл загружен";

        /// <summary>
        /// Кэш.
        /// </summary>
        private IMemoryCache _cache;

        /// <summary>
        /// Инициализирует контроллер/
        /// </summary>
        /// <param name="cache">Кэш.</param>
        public MarketingController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Загрузка данных текстового файла в кэш.
        /// </summary>
        /// <param name="file">Загружаемый файл.</param>
        /// <returns>Ответ сервера.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadFile(IFormFile file)
        {
            if (file == null || file.Length == 0 || Path.GetExtension(file.FileName) != AllowedFileExtention)
            {
                return BadRequest(WrongFileMessage);
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                var content = ConvertMarketingData(Encoding.UTF8.GetString(memoryStream.ToArray()).Split(Environment.NewLine));

                _cache.Set("LoadedAdvertisingData", content);

                return Ok(FileAddedMessage);
            }
        }

        /// <summary>
        /// Обработка и вывод найденных рекламных площадок.
        /// </summary>
        /// <param name="localtion">Локация.</param>
        /// <returns>Список рекламных площадок.</returns>
        [HttpGet]
        public IActionResult GetSpaceList(string localtion)
        {
            if (!_cache.TryGetValue("LoadedAdvertisingData", out List<AdvertisingSpace> data))
            {
                return NotFound(FileNotLoadedMessage);
            }

            if (string.IsNullOrEmpty(localtion))
            {
                return BadRequest(EmptyStringMessage);
            }

            var locations = localtion.Split('/');

            var allLocations = new List<string>();

            for (var i = locations.Length; i > 0; i--)
            {
                allLocations.Add(string.Join('/', locations.Take(i)));
            }

            var seletedData = data
                .Where(company => company.LocationList.Any(location => allLocations.Contains(location)))
                .Select(company => company.Name);

            if (seletedData.Count() == 0)
            {
                return NotFound(SpacesNotFoundMessage);
            }

            return Json(seletedData);
        }

        /// <summary>
        /// Обработка загруженного файла.
        /// </summary>
        /// <param name="content">Строки из загруженного файла.</param>
        /// <returns>Обработанный список сущностей.</returns>
        protected List<AdvertisingSpace> ConvertMarketingData(string[] content)
        {
            var marketSpaces = new List<AdvertisingSpace>();

            foreach (var item in content)
            {
                var marketSpace = item.Split(':');

                marketSpaces.Add(
                    new AdvertisingSpace(marketSpace.First(), marketSpace.Last().Split(',').ToList()));
            }

            return marketSpaces;
        }
    }   
}
