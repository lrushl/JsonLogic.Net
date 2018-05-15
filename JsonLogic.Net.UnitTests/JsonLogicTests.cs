using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JsonLogic.Net.UnitTests
{
    public class JsonLogicTests
    {
        public static object data = Dynamic(d => {
            d.name = "John Doe";
            d.address = Dynamic(a => {
                a.street = "123 Main";
                a.city = "Gotham";
                a.zip = "33333";
            });
            d.luckyNumbers= new int[]{ 3, 5, 7 };
        });

        [Theory]
        [InlineData("{`==`: [1, 1]}", true)]

        [InlineData("{`+`: [0,0]}", 0d)]
        [InlineData("{`+`: [1,1]}", 2d)]
        [InlineData("{`+`: [-1, 1]}", 0d)]
        [InlineData("{`+`: [0.5, 0.5, 0.3]}", 1.3d)]
        [InlineData("{`+`: [1, 1.2]}", 2.2d)]
        [InlineData("{`+`: [3, `musketeers`]}", "3musketeers")]
        [InlineData("{`+`: [`musketeers`, 3]}", "musketeers3")]
        [InlineData("{`+`: 2}", 2d)]
        [InlineData("{`+`: -2}", -2d)]

        [InlineData("{`-`: [0,0]}", 0d)]
        [InlineData("{`-`: [0,1]}", -1d)]
        [InlineData("{`-`: [0,1,2]}", -3d)]
        [InlineData("{`-`: [1.5, 0.3]}", 1.2d)]
        [InlineData("{`-`: 2}", -2d)]
        [InlineData("{`-`: -2}", 2d)]
        
        [InlineData("{`*`: [0,0]}", 0d)]
        [InlineData("{`*`: [-1,1]}", -1d)]
        [InlineData("{`*`: [-2,1.5]}", -3d)]
        [InlineData("{`*`: [-2,-1.5]}", 3d)]
        
        [InlineData("{`/`: [-3,-1.5]}", 2d)]
        [InlineData("{`/`: [-3,1.5]}", -2d)]
        [InlineData("{`/`: [2,0]}", double.PositiveInfinity)]
        [InlineData("{`/`: [-2,0]}", double.NegativeInfinity)]

        [InlineData("{`%`: [3, 2]}", 1d)]
        [InlineData("{`%`: [3.3, 2]}", 3.3 % 2)]
        [InlineData("{`%`: [3.8, 2]}", 3.8 % 2)]
        [InlineData("{`%`: [3, 0]}", double.NaN)]

        [InlineData("{`max`: [1, 2, 3]}", 3d)]
        [InlineData("{`min`: [1, 2, 3]}", 1d)]

        [InlineData("{`<`: [1, 2, 3]}", true)]
        [InlineData("{`<`: [1, 1, 3]}", false)]
        [InlineData("{`<`: [1, 4, 3]}", false)]
        [InlineData("{`<=`: [1, 2, 3]}", true)]
        [InlineData("{`<=`: [1, 1, 3]}", true)]
        [InlineData("{`<=`: [1, 4, 3]}", false)]
        [InlineData("{`>`: [3, 2, 1]}", true)]
        [InlineData("{`>`: [3, 1, 1]}", false)]
        [InlineData("{`>`: [3, 4, 1]}", false)]
        [InlineData("{`>=`: [3, 2, 1]}", true)]
        [InlineData("{`>=`: [3, 1, 1]}", true)]
        [InlineData("{`>=`: [3, 4, 1]}", false)]

        [InlineData("{`var`: `name`}", "John Doe")]
        [InlineData("{`var`: `address.zip`}", "33333")]
        [InlineData("{`var`: [`nonexistent`, `default-value`]}", "default-value")]
        [InlineData("{`var`: `luckyNumbers.1`}", 5)]

        [InlineData("{`and`: [true, true]}", true)]
        [InlineData("{`and`: [true, false]}", false)]
        [InlineData("{`and`: [false, true]}", false)]
        [InlineData("{`and`: [false, false]}", false)]
        [InlineData("{`and`: [{`==`: [5,5]}, {`<`: [3,5]}]}", true)]
        
        [InlineData("{`or`: [true, true]}", true)]
        [InlineData("{`or`: [false, true]}", true)]
        [InlineData("{`or`: [true, false]}", true)]
        [InlineData("{`or`: [false, false]}", false)]
        public void Apply(string argsJson, object expectedResult) 
        {
            // Arrange
            var rules = JsonFrom( argsJson );
            var jsonLogic = new JsonLogicEvaluator();
            
            // Act
            var result = jsonLogic.Apply(rules, data);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("{`-`: [2,`something`]}", typeof(FormatException))]
        public void ApplyThrowsException(string rulesJson, Type exceptionType)
        {
            // Arrange
            var rules = JsonFrom(rulesJson);
            var jsonLogic = new JsonLogicEvaluator();
            object result = null;
            
            // Act & Assert
            try {
                result = jsonLogic.Apply(rules, data);
            }
            catch (Exception e) {
                Assert.True(exceptionType.IsAssignableFrom(e.GetType()));
            }
            finally {
                Assert.Equal(null, result);
            }
        }
        public static JObject JsonFrom(string input) {
            return JObject.Parse(input.Replace('`', '"'));
        }

        public static object Dynamic(Action<dynamic> ctor) 
        {
            var value = new System.Dynamic.ExpandoObject();
            ctor(value);
            return value;
        }
    }
    
}