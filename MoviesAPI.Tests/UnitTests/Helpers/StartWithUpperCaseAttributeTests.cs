using MoviesAPI.Helpers;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Tests.UnitTests.Helpers
{
    [TestClass]
    public class StartWithUpperCaseAttributeTests
    {
        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow(null)]
        public void IsValid_ReturnSuccess_IfValueIsEmpty(string value)
        {
            // Arrange
            StartWithUpperCaseAttribute validation = new StartWithUpperCaseAttribute();
            ValidationContext validationContext = new ValidationContext(new object());

            // Act
            ValidationResult? result = validation.GetValidationResult(value, validationContext);

            // Assert
            Assert.AreEqual(expected: ValidationResult.Success, actual: result);
        }

        [TestMethod]
        [DataRow("Upper")]
        [DataRow(" Upper")]
        public void IsValid_ReturnSuccess_IfValueIsUpperCase(string value)
        {
            // Arrange
            StartWithUpperCaseAttribute validation = new StartWithUpperCaseAttribute();
            ValidationContext validationContext = new ValidationContext(new object());

            // Act
            ValidationResult? result = validation.GetValidationResult(value, validationContext);

            // Assert
            Assert.AreEqual(expected: ValidationResult.Success, actual: result);
        }

        [TestMethod]
        [DataRow("lower")]
        [DataRow(" lower")]
        public void IsValid_ReturnError_IfValueDoenstStratInUpperCase(string value)
        {
            // Arrange
            StartWithUpperCaseAttribute validation = new StartWithUpperCaseAttribute();
            ValidationContext validationContext = new ValidationContext(new object());

            // Act
            ValidationResult? result = validation.GetValidationResult(value, validationContext);

            // Assert
            Assert.AreEqual(expected: "La primera letra debe ser mayúscula.", actual: result.ErrorMessage);
        }
    }
}

