using Crystal.Core.Abstractions;
﻿namespace Crystal.Core.Services.EmailSender;

public enum EmailType
{
    Confirmation = 0,
    PasswordReset = 1,
}