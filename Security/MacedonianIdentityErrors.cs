using Microsoft.AspNetCore.Identity;

namespace BauFlow.Security
{
    public class MacedonianIdentityErrors : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
            => new() { Description = "Се појави непозната грешка." };

        public override IdentityError ConcurrencyFailure()
            => new() { Description = "Податоците се променети. Обиди се повторно." };

        public override IdentityError PasswordMismatch()
            => new() { Description = "Лозинката не е точна." };

        public override IdentityError InvalidToken()
            => new() { Description = "Невалиден токен." };

        public override IdentityError LoginAlreadyAssociated()
            => new() { Description = "Овој логин веќе е поврзан со друг профил." };

        public override IdentityError InvalidUserName(string userName)
            => new() { Description = $"Корисничкото име '{userName}' не е валидно." };

        public override IdentityError InvalidEmail(string email)
            => new() { Description = $"Е-поштата '{email}' не е валидна." };

        public override IdentityError DuplicateUserName(string userName)
            => new() { Description = $"Корисничкото име '{userName}' веќе постои." };

        public override IdentityError DuplicateEmail(string email)
            => new() { Description = $"Е-поштата '{email}' веќе се користи." };

        public override IdentityError InvalidRoleName(string role)
            => new() { Description = $"Улогата '{role}' не е валидна." };

        public override IdentityError DuplicateRoleName(string role)
            => new() { Description = $"Улогата '{role}' веќе постои." };

        public override IdentityError UserAlreadyHasPassword()
            => new() { Description = "Корисникот веќе има лозинка." };

        public override IdentityError UserLockoutNotEnabled()
            => new() { Description = "Заклучувањето не е овозможено." };

        public override IdentityError UserAlreadyInRole(string role)
            => new() { Description = $"Корисникот веќе е во улогата '{role}'." };

        public override IdentityError UserNotInRole(string role)
            => new() { Description = $"Корисникот не е во улогата '{role}'." };

        public override IdentityError PasswordTooShort(int length)
            => new() { Description = $"Лозинката мора да има најмалку {length} карактери." };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new() { Description = "Лозинката мора да содржи специјален знак." };

        public override IdentityError PasswordRequiresDigit()
            => new() { Description = "Лозинката мора да содржи број." };

        public override IdentityError PasswordRequiresLower()
            => new() { Description = "Лозинката мора да содржи мала буква." };

        public override IdentityError PasswordRequiresUpper()
            => new() { Description = "Лозинката мора да содржи голема буква." };
    }
}