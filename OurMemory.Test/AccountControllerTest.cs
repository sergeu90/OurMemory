﻿using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OurMemory;
using OurMemory.Controllers;
using OurMemory.Data;
using OurMemory.Domain.Entities;
using OurMemory.Models;

namespace UnitTestProject1
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public async Task RegisterSuccsess()
        {
            var userStore = new Mock<UserStore<User>>();
            Mock<ApplicationUserManager> userManager = new Mock<ApplicationUserManager>(userStore.Object);

            var registerBindingModel = new RegisterBindingModel()
           {
               Email = "sergeu90@inbox.ru",
               Password = "f12uch12345Q1212z121123",
               ConfirmPassword = "f12uch12345Q1212z121123"
           };

            userManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerBindingModel.Password)).ReturnsAsync(IdentityResult.Success);

            userManager.Setup(x => x.AddToRoleAsync(It.IsAny<string>(), "Guest"));

            var accountController = new AccountController(userManager.Object,null) { Request = new HttpRequestMessage() };

            IHttpActionResult register = accountController.Register(registerBindingModel).Result;

            Assert.IsInstanceOfType(register, typeof(OkResult));
        }



    }
}
