namespace eMeetup.Modules.Users.Domain.Activities;

// Типы активностей
public enum ActivityType
{
    // Спорт
    Football,
    Basketball,
    Volleyball,
    Tennis,
    Running,
    Cycling,
    Swimming,
    GymWorkout,
    Yoga,
    MartialArts,

    // Активный отдых
    Hiking,
    Camping,
    Climbing,
    Skiing,
    Snowboarding,
    Surfing,
    Kayaking,

    // Социальные
    Restaurants,
    Cafes,
    Bars,
    BoardGames,
    Concerts,
    Movies,
    Theater,
    Museums,
    ArtExhibition,

    // Культурные
    LanguagePractice,
    BookClub,
    Cooking,
    Photography,
    MusicJam,
    Dance,

    // Путешествия
    Travel,
    Tourism,
    CityWalk,

    // Образование
    Workshop,
    Lecture,
    Seminar,
    Networking
}

// Уровни сложности/активности
public enum ActivityLevel
{
    VeryEasy,    // Прогулка, кафе
    Easy,        // Йога, кино
    Medium,      // Поход, ресторан
    Hard,        // Бег, теннис
    VeryHard     // Маунтинбайк, скалолазание
}

// Время суток
public enum TimeOfDay
{
    Morning,     // 6:00 - 12:00
    Afternoon,   // 12:00 - 17:00
    Evening,     // 17:00 - 21:00
    Night        // 21:00 - 6:00
}
