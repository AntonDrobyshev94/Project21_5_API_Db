using Project21API_Db.ContextFolder;
using Project21API_Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Project21API_Db.AuthContactApp;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Project21API_Db.Controllers;

namespace Project21API_Db.Data
{
    public class ContactData
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext context;
        public IConfiguration _configuration { get; }

        public ContactData(DataContext context, UserManager<User> userManager,
                                RoleManager<IdentityRole> roleManager,
                                IConfiguration configuration)
        {
            this.context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Метод добавления контакта, посредством
        /// обращения к методу Add datacontext ContactData.
        /// По окончанию производится сохранение методом
        /// SaveChanges.
        /// </summary>
        /// <param name="contact"></param>
        public void AddContacts(Contact contact)
        {
            context.Contacts.Add(contact);
            context.SaveChanges();
        }

        /// <summary>
        /// Метод, возаращающий последовательность коллекции объектов, 
        /// реализующих интерфейс IContact с помощью интерфейса 
        /// IEnumberable
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IContact> GetContacts()
        {
            return this.context.Contacts;
        }

        /// <summary>
        /// Асинхронный метод предоставления текущих ролей пользователя,
        /// принимающий строковый параметр userName и возвращающий 
        /// строковую коллекцию IList. В данном методе происходит 
        /// создание экземпляра работника, с последующим перебором 
        /// пользователей в существующей базе данных на предмет 
        /// соответствия принимаемому имени userName с помощью метода 
        /// FirstOrDefaultAsync. Id полученного юзера сохраняется в 
        /// переменную строкового типа idUser. Далее создается коллекция 
        /// IList roleIdCol с типом string и запускается цикл foreach, 
        /// в котором происходит перебор таблицы базы данных UserRoles, 
        /// с условием совпадения id элемента таблицы с ранее сохраненным 
        /// idUser. При совпадении происходит добавление id роли в 
        /// коллекцию IList. Далее, создается вторая коллекция IList 
        /// roleNameCol с типом string и запускается цикл foreach, в 
        /// котором происходит перебор ранее созданной коллекции IList 
        /// roleIdCol. В цикле создается экземпляр IdentityRole, в 
        /// который записывается результат поиска совпадающих Id ролей 
        /// (в таблице ролей базы данных) с помощью метода 
        /// FirstOrDefaultAsync. При условии, что экземпляр не равен 
        /// нулю происходит добавление имени роли в коллекцию roleNameCol. 
        /// По окончанию происходит возврат коллекции roleNameCol с 
        /// помощью ключевого слова return.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<IList<string>> GetCurrentRoles(string userName)
        {
            User? currentUser = await context.Users.FirstOrDefaultAsync(p => p.UserName == userName);
            string idUser = currentUser.Id;
            IList<string> roleIdCol = new List<string>();

            foreach (var item in context.UserRoles)
            {
                if (item.UserId == idUser)
                {
                    roleIdCol.Add(item.RoleId);
                }
            }
            IList<string> roleNameCol = new List<string>();
            foreach (var item in roleIdCol)
            {
                IdentityRole<string> nextRole = await context.Roles.FirstOrDefaultAsync(p => p.Id == item);
                if (nextRole != null)
                {
                    roleNameCol.Add(nextRole.Name);
                }
            }
            return roleNameCol;
        }

        /// <summary>
        /// Асинхронный метод предоставления всех зарегистрированных 
        /// пользователей, возвращающий строковую коллекцию IList.
        /// В данном методе происходит создание экземпляра строковой
        /// коллекции IList, далее в цикле foreach происходит перебор
        /// таблицы Users базы данных с последующим добавлением всех
        /// найденных пользователей в созданную коллекцию с помощью
        /// метода Add. По окончанию происходит возвращение коллекции
        /// IList с помощью ключевого слова return.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<string>> GetAllUsers()
        {
            IList<string> userNameCol= new List<string>();
            foreach (var item in context.Users)
            {
                userNameCol.Add(item.UserName);
            }
            return userNameCol;
        }

        /// <summary>
        /// Асинхронный метод предоставления всех зарегистрированных 
        /// пользователей с ролью администратора, возвращающий 
        /// строковую коллекцию IList. В методе происходит создание
        /// экземпляра коллекции IList adminsId, далее в цикле 
        /// foreach происходит перебор таблицы UserRoles базы данных 
        /// с условием, что значение RoleId текущего элемента равно 
        /// string "1" (т.е. Id роли администратора. При совпадении 
        /// происходит добавление Id текущего пользователя в 
        /// коллекцию adminsId. Далее проиходит создание очередной
        /// коллекции IList adminNameCol и в цикле foreach происходит
        /// перебор ранее созданной коллекции adminsId. В цикле 
        /// создается экземпляр User, в который записывается результат 
        /// поиска совпадающих Id пользователей (в таблице пользователей
        /// базы данных) с помощью метода FirstOrDefaultAsync. При 
        /// условии, что экземпляр не равен 
        /// нулю происходит добавление имени пользователя в коллекцию
        /// adminNameCol. По окончанию происходит возврат коллекции 
        /// adminNameCol с помощью ключевого слова return.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<string>> GetAllAdmins()
        {
            IList<string> adminsId= new List<string>();
            foreach (var item in context.UserRoles)
            {
                if (item.RoleId == "1")
                {
                    adminsId.Add(item.UserId);
                }
            }
            IList<string> adminNameCol = new List<string>();
            foreach (var item in adminsId)
            {
                User? adminUser = await context.Users.FirstOrDefaultAsync(p => p.Id == item);
                if(adminUser != null)
                {
                    adminNameCol.Add(adminUser.UserName);
                }
            }
            return adminNameCol;
        }

        /// <summary>
        /// Асинхронный метод удаления контакта невозвращаемого
        /// типа, который принимает в себя int значение Id 
        /// удаляемого пользователя. В методе используется
        /// директива using для определения границ текущего
        /// контекста для избежания ошибки ObjectDisposedException.
        /// Происходит создание экземпляра Contact, в которую
        /// записывается результат перебора таблицы Contacts
        /// базы данных на предмет совпадения принимаемого id
        /// с id контакта с помощью метода FirstOrDefaultAsync.
        /// При условии, что экземпляр contact не равен нулю
        /// из таблицы Contacts происходит удаление полученного
        /// экземпляра contact с помощью метода Remove с последующим
        /// сохранением базы данных методом SaveChangesAsync.
        /// </summary>
        /// <param name="id"></param>
        public async void DeleteContact(int id)
        {
            using (var context = new DataContext())
            {
                Contact contact = await context.Contacts.FirstOrDefaultAsync(x => x.ID == id);
                if (contact != null)
                {
                    context.Contacts.Remove(contact);
                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Асинхронный метод поиска контакта, принимающий int id
        /// контакта и возвращающий экземпляр контакта интерфейса
        /// IContact. Метод представлен лямбда выражением, в котором 
        /// происходит перебор таблицы Contacts текущей базы данных
        /// на предмет совпадения Id контакта с принимаемым Id с 
        /// помощью метода FirstOrDefaultAsync. В итоге происходит
        /// возвращение полученного экземпляра IContact.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Contact> GetContactByID(int id) => await context.Contacts.FirstOrDefaultAsync(x => x.ID == id);

        /// <summary>
        /// Асинхронный метод изменения контакта невозвращаемого типа,
        /// который принимает в себя id контакта и экземпляр контакта.
        /// В методе используется директива using для определения границ 
        /// текущего контекста для избежания ошибки ObjectDisposedException.
        /// Создается экземпляр Contact, в который записывается результат
        /// перебора таблицы Contacts текущей базы данных на предмет
        /// совпадения Id контакта с помощью метода FirstOrDefaultAsync.
        /// Далее происходит изменение параметров полученного контакта
        /// параметрами, которые были получены в принимаемом экземпляре
        /// Contact. Результат изменений сохраняется с помощью метода
        /// SaveChangesAsync.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contact"></param>
        public async void ChangeContact(int id, Contact contact)
        {
            using (var context = new DataContext())
            {
                Contact concreteContact = await context.Contacts.FirstOrDefaultAsync(x => x.ID == id);
                concreteContact.Name = contact.Name;
                concreteContact.Surname = contact.Surname;
                concreteContact.FatherName = contact.FatherName;
                concreteContact.TelephoneNumber = contact.TelephoneNumber;
                concreteContact.ResidenceAdress = contact.ResidenceAdress;
                concreteContact.Description = contact.Description;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Асинхронный метод создания новой роли, принимающий модель 
        /// роли RoleModel, описанную отдельным классом, возвращающий 
        /// переменную строкового типа. В методе происходит создание
        /// переменной строкового типа createResponse, далее в блоке
        /// try catch происходит проверка таблицы ролей на отсутствие
        /// роли, совпадающей с ролью принимаемой модели. При отсутствии
        /// таких ролей происходит добавление нового экземпляра
        /// IdentityRole с помощью метода CreateAsync. В переменную
        /// строкового типа createResponse записывается сообщение 
        /// об успешном добавленеии. В противном случае, записывается
        /// сообщение, что роль уже существует. По окончанию происходит
        /// возвращение переменной строкового типа ключевым словом
        /// return.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> CreateRole(RoleModel model)
        {
            string createResponse = string.Empty;
            try
            {
                if (!await _roleManager.RoleExistsAsync(model.roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole()
                    {
                        Name = model.roleName,
                        NormalizedName = model.roleName
                    });
                    createResponse = "Роль успешно добавлена";
                }
                else
                {
                    createResponse = "Роль уже существует";
                }
            }
            catch (Exception)
            {
                createResponse = "Ошибка выполнения";
            }
            return createResponse;
        }

        /// <summary>
        /// Асинхронный метод добавления роли указанному пользователю,
        /// принимающий модель роли RoleModel, описанную отдельным 
        /// классом, возвращающий переменную строкового типа. В методе 
        /// происходит создание переменной строкового типа addRoleResponse, 
        /// далее в блоке try catch происходит создание переменной user,
        /// в которую с помощью метода FindByNameAsync записывается 
        /// экземпляр пользователя с именем, который совпадает с именем
        /// принимаемой модели. Далее, при условии, что роль существует
        /// (проверяется с помощью метода RoleExistAsync и названием
        /// роли принимаемой модели) происходит запись сообщения о
        /// доступности роли для добавления в переменную addRoleResponse,
        /// и при условии, что экземпляр user не равен нулю происходит
        /// добавление роли указанному пользователю с помощью метода 
        /// AddToRoleAsync. При этом, в переменную addRoleResponse 
        /// добавляются записи о том, что пользователь указан верно и
        /// что роль успешно добавлена. В противном случае, в переменную
        /// addRoleResponse записывается сообщение, что пользователь
        /// отсутствует или что указанная роль не существует. По 
        /// окончанию происходит возвращение переменной строкового типа 
        /// ключевым словом return.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> AddRoleToUser(RoleModel model)
        {
            string addRoleResponse = string.Empty;
            try
            {
                var user = await _userManager.FindByNameAsync(model.userName);
                if (await _roleManager.RoleExistsAsync(model.roleName))
                {
                    addRoleResponse += "Роль доступна для добавления";
                    if (user != null)
                    {
                        await _userManager.AddToRoleAsync(user, model.roleName);
                        addRoleResponse += "Пользователь указан верно";
                        addRoleResponse += "Роль успешно добавлена";
                    }
                    else
                    {
                        addRoleResponse += "Пользователь отсутствует";
                    }
                }
                else
                {
                    addRoleResponse += "Ошибка: указанная роль не существует";
                    if (user != null)
                    {
                        addRoleResponse += "Пользователь указан верно";
                    }
                    else
                    {
                        addRoleResponse += "Пользователь отсутствует";
                    }
                }
            }
            catch (Exception)
            {
                addRoleResponse += "Ошибка выполнения";
            }
            return addRoleResponse;
        }

        /// <summary>
        /// Асинхронный метод удаления роли у пользователя, принимающий 
        /// модель роли RoleModel, описанную отдельным классом, 
        /// возвращающий переменную строкового типа. В методе происходит 
        /// создание переменной строкового типа removeResponse, далее в 
        /// блоке try catch происходит создание переменной user,в которую 
        /// с помощью метода FindByNameAsync записывается экземпляр 
        /// пользователя с именем, который совпадает с именем принимаемой 
        /// модели. Далее, при условии, что роль существует
        /// (проверяется с помощью метода RoleExistAsync и названием
        /// роли принимаемой модели) происходит запись сообщения о
        /// доступности роли для удаления в переменную removeResponse и
        /// создается экземпляр IdentityRole, в который записывается
        /// результат перебора таблицы Roles базы данных на предмет
        /// совпадения имени роли с именем роли принимаемой модели с
        /// помощью метода FirstOrDefaultAsync.Далее, при условии,
        /// что экземпляр User не равен нулю, в переменную removeResponse
        /// происходит запись о том, что пользователь указан верно и 
        /// создается экземпляр IdentityUserRole параметаризированный 
        /// строкой, в который записывается результат перебора таблицы 
        /// UserRoles базы данных на предмет совпадения Id пользователя 
        /// экземпляра IdentityUserRole с Id ранее найденного экземпляра 
        /// user и Id роли экземпляра IdentityUserRole с ранее найденным 
        /// экземпляром IdentityRole. Если полученный экземпляр не равен 
        /// нулю, то происходит удаление указанной роли у указанного 
        /// пользователя с помощью метода RemoveFromRoleAsync, после чего
        /// происходит запись об успешном удалении роли в переменную
        /// removeResponse. В обратном случае, происходит запись о том,
        /// что роль отсутствует у указанного пользователя. По окончанию
        /// происходит возврат переменной removeResponse с помощью
        /// ключевого слова return.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> RemoveUserRole(RoleModel model)
        {
            string removeResponse = string.Empty;
            try
            {
                var user = await _userManager.FindByNameAsync(model.userName);
                if (await _roleManager.RoleExistsAsync(model.roleName))
                {
                    removeResponse += "Роль доступна для удаления";
                    IdentityRole? concreteRole = await context.Roles.FirstOrDefaultAsync(p => p.Name == model.roleName);

                    if (user != null)
                    {
                        removeResponse += "Пользователь указан верно";
                        IdentityUserRole<string>? someUserRole = await context.UserRoles.FirstOrDefaultAsync(p => p.UserId == user.Id && p.RoleId == concreteRole.Id);
                        if (someUserRole != null)
                        {
                            await _userManager.RemoveFromRoleAsync(user, model.roleName);
                            removeResponse += "Роль успешно удалена";
                        }
                        else
                        {
                            removeResponse += "Роль отсутствует у указанного пользователя";
                        }
                    }
                    else
                    {
                        removeResponse += "Пользователь отсутствует";
                    }
                }
                else
                {
                    removeResponse += "Ошибка: указанная роль не существует";
                    if (user != null)
                    {
                        removeResponse += "Пользователь указан верно";
                    }
                    else
                    {
                        removeResponse += "Пользователь отсутствует";
                    }
                }
            }
            catch (Exception)
            {
                removeResponse += "Ошибка выполнения";
            }
            return removeResponse;

        }

        /// <summary>
        /// Асинхронный метод удаления роли у пользователя, принимающий 
        /// модель роли RoleModel, описанную отдельным классом, 
        /// возвращающий переменную строкового типа. В методе происходит 
        /// создание переменной строкового типа removeResponse, далее в 
        /// блоке try catch происходит создание экземпляра User,
        /// в который записывается результат перебора таблицы Users 
        /// базы данных на совпадение имени пользователя с именем,
        /// принимаемым в модели. При условии, что полученный экземпляр
        /// не равен нулю происходит удаление указанного экземпляра с
        /// помощью метода DeleteAsync. В переменную removeResponse 
        /// записывается сообщение об успешном удалении. В противном
        /// случае, если пользователь не найден (экземпляр равен нулю),
        /// в переменную removeResponse записывается сообщение о том,
        /// что пользователь отсутствует. По окончанию происходит 
        /// возврат экземпляра removeResponse с помощью ключевого слова
        /// return.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> RemoveUser(RoleModel model)
        {
            string removeResponse = string.Empty;
            try
            {
                var user = await _userManager.FindByNameAsync(model.userName);
                if(user!= null)
                {
                    await _userManager.DeleteAsync(user);
                    removeResponse = "Пользователь успешно удален";
                }
                else
                {
                    removeResponse = "Пользователь отсутствует";
                }
            }
            catch
            {
                removeResponse = "Ошибка";
            }
            return removeResponse;
        }

        /// <summary>
        /// Асинхронный метод входа в учетную запись, который принимает
        /// UserLoginProp модель, описанную отдельным классом и возвращает
        /// TokenResponseModel, описанную отдельным классом. В данном методе
        /// происходит создание экземпляра пользователя User newUser, в 
        /// параметр UserName которого записывается UserName принимаемой модели.
        /// Далее создается экземпляр User "user" в который записывается 
        /// результат перебора таблицы Users базы данных на предмет совпадения
        /// имени пользователя с именем пользователя принимаемой модели.
        /// Если полученный экземпляр равен нулю, то возвращается null,
        /// приводящий к завершению метода.
        /// Если экземпляр не равен нулю, то создается экземпляр PasswordHasher
        /// параметаризированный строкой. Далее создается новый обобщенный
        /// экземпляр, который является результатом работы метода 
        /// VerifyHashedPassword на экземпляре passwordHasher. Данный метод
        /// принимает значение пароля из полученной модели и переводит сверяет
        /// его с паролем текущего экземпляра пользователя в виде Хэш пароля.
        /// Далее, с помощью конструкции switch происходит проверка полученного
        /// экземпляра passwordVerificationResult. При результате 
        /// PasswordVerificationResult.Failed происходит возвращение null с 
        /// помощью ключевого слова return и завершение метода.
        /// Если результат не ошибочный, то метод продолжается и происходит
        /// создание переменной строкового типа id, в которую записывается
        /// значение Id текущего экземпляра User. Далее происходит создание
        /// экземпляра коллекции List строкового типа и в цикле foreach
        /// происходит перебор таблицы UserRoles базы данных и с условием,
        /// что параметр UserId текущей таблицы равен Id пользователя происходит
        /// запись Id роли в коллекцию с помощью метода Add. Далее, создается
        /// строковая переменная roleId для записи в неё id роли. В следующем 
        /// цикле foreach происходит перебор полученной ранее коллекции List на
        /// предмет равенства id единице (т.е., означает что пользователь - 
        /// администратор). Если найден id, равный 1, то происходит завершение
        /// цикла с записью id в переменную roleId. В противном случае результат
        /// в любом случае записывается в roleId, но он уже не будет равен 1.
        /// Далее происходит создание переменной строкового типа jwt, в которую
        /// будет записан результат выполнения метода GenerateJwtToken,
        /// который принимает в себя имя текущего экземпляра пользователя и id
        /// полученной роли roleId, после чего создается модель TokenResponseModel,
        /// описанная отдельным классом, с параметрами токена и имени пользователя.
        /// В параметры созданной модели записываются полученные ранее параметры
        /// токена и имени пользователя и происходит возврат модели с помощью 
        /// метода return
        /// </summary>
        /// <param name="loginData"></param>
        /// <returns></returns>
        public async Task<TokenResponseModel> Login(UserLoginProp loginData)
        {
            User newUser = new User()
            {
                UserName = loginData.UserName
            };

            User? user = await context.Users.FirstOrDefaultAsync(p => p.UserName == newUser.UserName);

            if (user is null)
            {
                return null;
            }
            else
            {
                var passwordHasher = new PasswordHasher<string>();
                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginData.Password);
                switch (passwordVerificationResult)
                {
                    case PasswordVerificationResult.Failed:
                        return null;
                }
            }
            string id = user.Id;
            List<string> idCol = new List<string>();
            foreach (var item in context.UserRoles)
            {
                if (item.UserId == id)
                {
                    idCol.Add(item.RoleId);
                }
            }
            string roleId = string.Empty;
            foreach(var item in idCol)
            {
                if (item == "1")
                {
                    roleId = item;
                    break;
                }
                else
                {
                    roleId = item;
                }
            }
            if (roleId != null)
            {
                string jwt = GenerateJwtToken(user.UserName, roleId);

                var response = new TokenResponseModel
                {
                    access_token = jwt,
                    username = user.UserName
                };
                return (response);
            }
            else
            {
                return null;
            }
        }

        private string GenerateJwtToken(string userName, string roleId)
        {
            if (roleId == "1")
            {
                var claims = new List<Claim> {
                            new Claim(ClaimTypes.Name, userName),
                            new Claim(ClaimTypes.Role, "Admin")
                        };
                var jwt = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"]!,
                audience: _configuration["Jwt:Audience"]!,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!)
                    ),
                    SecurityAlgorithms.HmacSha256));
                return new JwtSecurityTokenHandler().WriteToken(jwt);
            }
            else
            {
                var claims = new List<Claim> {
                            new Claim(ClaimTypes.Name, userName),
                            new Claim(ClaimTypes.Role, "User")
                        };
                var jwt = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"]!,
                audience: _configuration["Jwt:Audience"]!,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!)
                    ),
                    SecurityAlgorithms.HmacSha256));
                return new JwtSecurityTokenHandler().WriteToken(jwt);
            }
        }

        /// </summary>
        /// <param name="loginData"></param>
        /// <returns></returns>
        public async Task<TokenResponseModel> Register(UserRegistration registrData)
        {
            var user = new User { UserName = registrData.LoginProp };
            var createResult = await _userManager.CreateAsync(user, registrData.Password);
            var addRoleResult = await _userManager.AddToRoleAsync(user, "User");
            var jwt = GenerateJwtToken(user.UserName, "2");

            var response = new TokenResponseModel
            {
                access_token = jwt,
                username = user.UserName
            };

            if (createResult.Succeeded && addRoleResult.Succeeded)
            {
                return response;
            }
            else
            {
                return null;
            }
        }

        public async Task<TokenResponseModel> AdminRegister(UserRegistration registrData)
        {
            var user = new User { UserName = registrData.LoginProp };
            var createResult = await _userManager.CreateAsync(user, registrData.Password);
            var addRoleResult = await _userManager.AddToRoleAsync(user, "User");
            var jwt = GenerateJwtToken(user.UserName, "2");

            var response = new TokenResponseModel
            {
                access_token = jwt,
                username = user.UserName
            };
            if (createResult.Succeeded && addRoleResult.Succeeded)
            {
                return response;
            }
            else
            {
                return null;
            }
        }
    }
}
 
