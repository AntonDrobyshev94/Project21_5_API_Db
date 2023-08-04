using Microsoft.AspNetCore.Mvc;
using Project21API_Db.AuthContactApp;
using Project21API_Db.Data;
using Project21API_Db.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Azure;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Project21API_Db.ContextFolder;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Project21API_Db.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ContactData repositoryData;

        public IConfiguration Configuration { get; }

        public ValuesController(ContactData repositoryData)
        {
            this.repositoryData = repositoryData;
        }

        /// <summary>
        /// GET метод, возвращающий информацию о контактах
        /// </summary>
        /// <returns></returns>
        // GET api/values
        [HttpGet]
        public IEnumerable<IContact> Get()
        {
            return repositoryData.GetContacts();
        }

        /// <summary>
        /// Get Асинхронный метод, предоставляющий информацию о
        /// выбранном контакте
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/values/5
        [HttpGet]
        [Route("Details/{id}")]
        public async Task<Contact> ContactDetails(int id)
        {
            return await repositoryData.GetContactByID(id);
        }

        /// <summary>
        /// Get метод с атрибутом авторизации по роли администратора,
        /// Асинхронный метод, который возвращает текущие роли
        /// пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("CurrentRoles")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CurrentRoles()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;
                IList<string> collection = await repositoryData.GetCurrentRoles(userName);
                string json = JsonConvert.SerializeObject(collection);
                return Ok(json);
            }
            return null;
        }

        /// <summary>
        /// Get метод, возвращающий bool переменную с ответом
        /// о валидности токена. Метод служит для проверки
        /// токена на валидность.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("CheckToken")]
        public async Task<bool> CheckTokenMethod()
        {
            if (User.Identity.IsAuthenticated)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get запрос с атрибутом авторизации по роли администратора,
        /// Асинхронный метод, возвращающий список всех пользователей
        /// пользователей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("GetUsers")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetUsers()
        {
            IList<string> collection = await repositoryData.GetAllUsers();
            string json = JsonConvert.SerializeObject(collection);
            return Ok(json);
        }

        /// <summary>
        /// Get запрос с атрибутом авторизации по роли администратора,
        /// Асинхронный метод, возвращающий список всех
        /// пользователей с ролью администратора
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAdmins")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAdmins()
        {
            IList<string> collection = await repositoryData.GetAllAdmins();
            string json = JsonConvert.SerializeObject(collection);
            return Ok(json);
        }

        /// <summary>
        /// POST запрос с атрибутом авторизации.
        /// Метод добавления нового контакта в базу данных
        /// </summary>
        /// <param name="value"></param>
        // POST api/values
        [HttpPost]
        [Authorize]
        public void Post([FromBody] Contact value)
        {
            repositoryData.AddContacts(value);
        }

        /// <summary>
        /// POST запрос с атрибутом авторизации с ролью администратора
        /// Метод удаления контакта из базы данных
        /// </summary>
        /// <param name="id"></param>
        // DELETE api/values/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public void Delete(int id)
        {
            repositoryData.DeleteContact(id);
        }

        /// <summary>
        /// POST запрос с атрибутом авторизации с ролью администратора
        /// Метод для изменения контакта по указанному ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contact"></param>
        [HttpPost]
        [Route("ChangeContactById/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public void ChangeContactById(int id, Contact contact)
        {
            repositoryData.ChangeContact(id, contact);
        }

        /// <summary>
        /// POST запрос с атрибутом авторизации с ролью администратора
        /// Метод создания новой роли
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RoleCreate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<string> RoleCreate(RoleModel model)
        {
            return await repositoryData.CreateRole(model);
        }

        /// <summary>
        /// POST запрос с атрибутом авторизации с ролью администратора
        /// Метод добавления роли указанному пользователю
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddRoleToUser")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<string> AddRoleUser(RoleModel model)
        {
            return await repositoryData.AddRoleToUser(model);
        }

        /// <summary>
        /// POST запрос с атрибутом авторизации с ролью администратора
        /// Метод удаления роли у указанного пользователя
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveUserRole")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<string> RemoveRoleUser(RoleModel model)
        {
            return await repositoryData.RemoveUserRole(model);
        }

        /// <summary>
        /// POST запрос с атрибутом авторизации с ролью администратора
        /// Метод удаления указанного пользователя
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveUser")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<string> RemoveUser(RoleModel model)
        {
            return await repositoryData.RemoveUser(model);
        }

        /// <summary>
        /// POST запрос для регистрации нового пользователя по
        /// указанным в модели данным.
        /// </summary>
        /// <param name="regData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Registration([FromBody] UserRegistration regData)
        {
            TokenResponseModel loginResponse = await repositoryData.Register(regData);
            if (loginResponse == null)
            {
                return Unauthorized();
            }
            return Ok(loginResponse);
        }

        /// <summary>
        /// POST запрос на аутентификацию пользователя по указанным
        /// аутентификационным данным
        /// </summary>
        /// <param name="loginData"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] UserLoginProp loginData)
        {
            TokenResponseModel loginResponse = await repositoryData.Login(loginData);
            if (loginResponse == null)
            {
                return Unauthorized();
            }
            return Ok(loginResponse);
        }
    }
}

