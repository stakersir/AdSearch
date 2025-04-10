namespace TestTask.Models
{
    /// <summary>
    /// Рекламная площадка.
    /// </summary>
    public class AdvertisingSpace
    {
        /// <summary>
        /// Название.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Список локаций площадки.
        /// </summary>
        public List<string> LocationList { get; private set; }

        /// <summary>
        /// Инициализирует рекламную площадку.
        /// </summary>
        /// <param name="name">Название.</param>
        /// <param name="locationList">Список локаций площадки.</param>
        public AdvertisingSpace(string name, List<string> locationList)
        {
            Name = name;
            LocationList = locationList;
        }
    }
}
