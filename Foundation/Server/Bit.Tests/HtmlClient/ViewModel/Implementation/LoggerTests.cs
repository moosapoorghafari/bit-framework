﻿using System.Linq;
using Bit.Api.ApiControllers;
using Bit.Model.Dtos;
using Bit.Test;
using Bit.Test.Core.Implementations;
using Bit.Test.Server;
using FakeItEasy;
using IdentityModel.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Bit.Tests.HtmlClient.ViewModel.Implementation
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        [TestCategory("HtmlClient"), TestCategory("Logging")]
        [Ignore]
        public virtual void LogException()
        {
            using (BitOwinTestEnvironment testEnvironment = new BitOwinTestEnvironment(new TestEnvironmentArgs { UseRealServer = true }))
            {
                TokenResponse token = testEnvironment.Server.Login("ValidUserName", "ValidPassword", clientName: "TestResOwner");

                try
                {
                    using (RemoteWebDriver driver = testEnvironment.Server.GetWebDriver(new RemoteWebDriverOptions { Token = token }))
                    {
                        driver.ExecuteTest("LoggerTests.logException");
                    }

                    Assert.Fail();
                }
                catch
                {
                    ClientsLogsController clientsLogsController = TestDependencyManager.CurrentTestDependencyManager
                        .Objects.OfType<ClientsLogsController>().Single();

                    A.CallTo(() => clientsLogsController.Create(A<ClientLogDto>.That.Matches(cl => cl.ErrorName == "TypeError")))
                        .MustHaveHappened(Repeated.Exactly.Once);
                }
            }
        }
    }
}