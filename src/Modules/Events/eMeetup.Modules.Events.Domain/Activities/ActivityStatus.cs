using System;
using System.Collections.Generic;
using System.Text;

namespace eMeetup.Modules.Events.Domain.Activities;

public enum ActivityStatus
{
    Scheduled = 1,   // Запланирована
    InProgress = 2,  // В процессе
    Completed = 3,   // Завершена
    Cancelled = 4,   // Отменена
    Postponed = 5    // Отложена
}
