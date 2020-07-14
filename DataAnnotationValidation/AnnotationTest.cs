using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace DataAnnotationValidation
{
    public class AnnotationTest
    {
        [Fact]
        public void Validate_InvalidObject_ReturnErrors()
        {
            // https://asp.mvc-tutorial.com/fr/535/models/manual-model-validation-with-data-annotations/
            var userInfo = new UserInfo
            {
                Email = "invalidAddress.com"
            };

            var ctx = new ValidationContext(userInfo);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(userInfo, ctx, results, validateAllProperties: true);
            Assert.Equal(2, results.Count);
        }
    }

    class UserInfo
    {
        [MinLength(3)]
        [MaxLength(10)]
        public string Name { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
    }
}