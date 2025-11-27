using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.ObjectModel;

namespace CloudQA.AutomationTests
{
    [TestFixture]
    public class RobustFormTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private const string Url = "https://app.cloudqa.io/home/AutomationPracticeForm";

        [SetUp]
        public void Setup()
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                
                _driver = new ChromeDriver(options);
                _driver.Manage().Window.Maximize();
                
                _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                
                Console.WriteLine($"[Setup] Navigating to {Url}");
                _driver.Navigate().GoToUrl(Url);
                Console.WriteLine("[Setup] Navigation complete");
                
                _wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                Console.WriteLine("[Setup] Page ready");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Setup] Error: {ex.Message}");
                _driver?.Quit();
                _driver?.Dispose();
                throw;
            }
        }

        [Test]
        public void Test_Fill_Form_Using_Robust_Locators()
        {
 
            FillInputByLabel("First Name", "Rajnish");
            System.Threading.Thread.Sleep(500); 
            SelectRadioByLabel("Gender", "Male");
            System.Threading.Thread.Sleep(500); 
            FillInputByLabel("Mobile #", "9876543210");
            
            var firstNameValue = GetInputValueByLabel("First Name");
            Assert.That(firstNameValue, Is.EqualTo("Rajnish"), "First Name should match input.");

            var mobileValue = GetInputValueByLabel("Mobile #");
            Assert.That(mobileValue, Is.EqualTo("9876543210"), "Mobile number should match input.");
            
            
            System.Threading.Thread.Sleep(2000); 
        }

        [TearDown]
        public void Cleanup()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

    
        private void FillInputByLabel(string labelText, string valueToType)
        {

            string xpath = $"//label[normalize-space()='{labelText}']/following::input[1]";

            try
            {
                Console.WriteLine($"[FillInputByLabel] Looking for input after label '{labelText}'");
                IWebElement element = _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
                Console.WriteLine($"[FillInputByLabel] Found element, clearing and sending keys '{valueToType}'");
                
                element.Clear();
                element.SendKeys(valueToType);
                Console.WriteLine($"[FillInputByLabel] Successfully filled '{labelText}' with '{valueToType}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FillInputByLabel] Error filling '{labelText}': {ex.Message}");
                throw;
            }
        }

        private string GetInputValueByLabel(string labelText)
        {
            string xpath = $"//label[normalize-space()='{labelText}']/following::input[1]";
            IWebElement element = _driver.FindElement(By.XPath(xpath));
            return element.GetAttribute("value");
        }

        private void SelectRadioByLabel(string groupLabel, string optionText)
        {
            string optionXpath = $"//label[normalize-space()='{groupLabel}']/following::label[normalize-space()='{optionText}']/preceding-sibling::input[@type='radio']";

            try 
            {
                Console.WriteLine($"[SelectRadioByLabel] Looking for radio '{optionText}' under group '{groupLabel}'");
                var radio = _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(optionXpath)));
                if (!radio.Selected)
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", radio);
                    System.Threading.Thread.Sleep(200);
                    radio.Click();
                    Console.WriteLine($"[SelectRadioByLabel] Clicked radio '{optionText}'");
                }
                else
                {
                    Console.WriteLine($"[SelectRadioByLabel] Radio '{optionText}' already selected");
                }
            }
            catch (WebDriverTimeoutException ex)
            {
               Console.WriteLine($"[SelectRadioByLabel] Timeout with primary XPath, trying fallback: {ex.Message}");

               string fallbackXpath = $"//label[normalize-space()='{groupLabel}']/following::input[@value='{optionText}']";
               var radio = _driver.FindElement(By.XPath(fallbackXpath));
               ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", radio);
               System.Threading.Thread.Sleep(200);
               radio.Click();
               Console.WriteLine($"[SelectRadioByLabel] Clicked radio using fallback XPath");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SelectRadioByLabel] Error: {ex.Message}");
                throw;
            }
        }
    }
}