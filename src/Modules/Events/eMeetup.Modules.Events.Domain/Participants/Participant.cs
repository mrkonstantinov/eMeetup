using System;
using System.Collections.Generic;
using System.Text;
using eMeetup.Common.Domain;

namespace eMeetup.Modules.Events.Domain.Participants;

public class Participant : Entity
{
    private Participant()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string UserName { get; private set; }

    public DateTime DateOfBirth { get; private set; }

    public Gender Gender { get; private set; }

    public DateTime SyncedAt { get; private set; }

    public static Participant Create(Guid id, string email, string userName, DateTime dateOfBirth, Gender gender, DateTime syncedAt)
    {
        return new Participant
        {
            Id = id,
            Email = email,
            UserName = userName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            SyncedAt = syncedAt
        };
    }

    public void Update(string userName, DateTime syncedAt)
    {
        UserName = userName;
        SyncedAt = syncedAt;
    }
}


public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3,
    PreferNotToSay = 4
}
