using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.WebApi.Formatter;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter
{
    [TestClass]
    public class SirenBuilderActionsTest : SirenBuilderTestBase
    {
        private const string CustomMediaType = "custom/mediatype";

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            ClassInitBase();
        }

        [TestInitialize]
        public void TestInit()
        {
            TestInitBase();
        }

        [TestMethod]
        public void ActionsTest()
        {
            var routeName = nameof(ActionsHypermediaObject) + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(ActionsHypermediaObject), routeName, HttpMethods.Get);

            var routeNameHypermediaActionNotExecutable = nameof(HypermediaActionNotExecutable) + "_Route";
            RouteRegister.AddActionRoute(typeof(HypermediaActionNotExecutable), routeNameHypermediaActionNotExecutable, HttpMethods.Post);

            var routeNameHypermediaActionNoArgument = nameof(HypermediaActionNoArgument) + "_Route";
            RouteRegister.AddActionRoute(typeof(HypermediaActionNoArgument), routeNameHypermediaActionNoArgument, HttpMethods.Post);

            var routeNameHypermediaActionWithArgument = nameof(HypermediaActionWithArgument) + "_Route";
            
            RouteRegister.AddActionRoute(typeof(HypermediaActionWithArgument), routeNameHypermediaActionWithArgument, HttpMethods.Post, CustomMediaType);

            var routeNameRegisteredActionParameter = nameof(RegisteredActionParameter) + "_Route";
            RouteRegister.AddParameterTypeRoute(typeof(RegisteredActionParameter), routeNameRegisteredActionParameter, HttpMethods.Get);
            
            var routeNameFileUpload = nameof(FileUploadAction) + "_Route";
            RouteRegister.AddActionRoute(typeof(FileUploadAction), routeNameFileUpload, HttpMethods.Post, DefaultMediaTypes.MultipartFormData);

            var routeNameFileUploadWithParameter = nameof(FileUploadWithParameterAction) + "_Route";
            RouteRegister.AddActionRoute(typeof(FileUploadWithParameterAction), routeNameFileUploadWithParameter, HttpMethods.Post, DefaultMediaTypes.MultipartFormData);
            
            // for dynamic actions
            // parameter type route
            var routeNameDynamicParameter = nameof(DynamicParameter) + "_Route";
            RouteRegister.AddParameterTypeRoute(typeof(DynamicParameter), routeNameDynamicParameter, HttpMethods.Get);
            
            // ReSharper disable InconsistentNaming
            var routeNameDynamicAction = nameof(DynamicAction) + "_Route";
            RouteRegister.AddActionRoute(typeof(DynamicAction), routeNameDynamicAction, HttpMethods.Post);
            
            var ho = new ActionsHypermediaObject();

            var siren = SirenConverter.ConvertToJson(ho);

            AssertClassName(siren, nameof(ActionsHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertHasNoLinks(siren);

            var actions = siren["actions"]
                .Should().NotBeNull()
                    .And.HaveCount(11)
                    .And.AllBeAssignableTo<JObject>()
                    .Which.ToList();
            
            AssertActionBasic(actions[0], "RenamedAction", "POST", routeNameHypermediaActionNoArgument, 5,  ActionClasses.ParameterLessActionClass, "A Title");
            AssertActionBasic(actions[1], "ActionNoArgument", "POST", routeNameHypermediaActionNoArgument, 4,  ActionClasses.ParameterLessActionClass);

            AssertActionBasic(actions[2], "ActionWithArgument", "POST", routeNameHypermediaActionWithArgument, 6,  ActionClasses.ParameterActionClass);
            AssertActionArgument(actions[2], CustomMediaType,  nameof(ActionParameter), nameof(ActionParameter),hasDefaultValues:true);
            AssertDefaultValues(actions[2], ActionsHypermediaObject.ActionWithArgumentDefaultValues);
            
            AssertActionBasic(actions[3], nameof(ExternalActionNoArgument), "POST", "ExternalActionRoute", 4,  ActionClasses.ParameterLessActionClass);
            
            AssertActionBasic(actions[4], nameof(ExternalActionWithArgument), "DELETE", "ExternalActionRoute", 6,  ActionClasses.ParameterActionClass);
            AssertActionArgument(actions[4], CustomMediaType, nameof(ActionParameter),  nameof(ActionParameter),hasDefaultValues:true);
            AssertDefaultValues(actions[4], ActionsHypermediaObject.ExternalActionWithArgumentDefaultValues);
            
            AssertActionBasic(actions[5], nameof(FileUploadAction), "POST", routeNameFileUpload, 6,  ActionClasses.FileUploadActionClass);
            AssertFileUploadAction(actions[5], ho.FileUploadAction.FileUploadConfiguration, DefaultMediaTypes.MultipartFormData, hasParameter: false);
            
            AssertActionBasic(actions[6], nameof(FileUploadWithParameterAction), "POST", routeNameFileUploadWithParameter, 6,  ActionClasses.FileUploadActionWithParameterClass);
            AssertFileUploadAction(actions[6], ho.FileUploadWithParameterAction.FileUploadConfiguration, DefaultMediaTypes.MultipartFormData, hasParameter: true);
            AssertFileUploadActionArgument(actions[6], DefaultMediaTypes.ApplicationJson, nameof(ActionParameter), nameof(ActionParameter), hasDefaultValues: false);
            
            AssertActionBasic(actions[7], nameof(ExternalFileUploadAction), "POST", "ExternalActionRoute", 6,  ActionClasses.FileUploadActionClass);
            AssertFileUploadAction(actions[7], ho.FileUploadAction.FileUploadConfiguration, CustomMediaType, hasParameter: false);
            
            AssertActionBasic(actions[8], nameof(ActionsHypermediaObject.DynamicActionPrefilledParameter_None), "POST", routeNameDynamicAction, 4,  ActionClasses.ParameterLessActionClass);
            
            AssertActionBasic(actions[9], nameof(ActionsHypermediaObject.DynamicActionPrefilledParameter_String), "POST", routeNameDynamicAction, 6,  ActionClasses.ParameterActionClass);
            AssertActionArgument(actions[9], DefaultMediaTypes.ApplicationJson,  nameof(DynamicParameter), nameof(DynamicParameter), hasDefaultValues:true, parameterTypeRouteName:routeNameDynamicParameter, objectKeyString:"{ SchemaRouteValue = SchemaKey_1 }");
            AssertDynamicDefaultValues(actions[9], "3");
            
            AssertActionBasic(actions[10], nameof(ActionsHypermediaObject.DynamicActionPrefilledParameter_Object), "POST", routeNameDynamicAction, 6,  ActionClasses.ParameterActionClass);
            AssertActionArgument(actions[10], DefaultMediaTypes.ApplicationJson,  nameof(DynamicParameter), nameof(DynamicParameter), hasDefaultValues:true, parameterTypeRouteName:routeNameDynamicParameter, objectKeyString:"{ SchemaRouteValue = SchemaKey_2 }");
            AssertDynamicDefaultValues(actions[10], "4");
        }

        private void AssertFileUploadAction(JObject action, FileUploadConfiguration fileUploadConfiguration, string type, bool hasParameter = false)
        {
            action["type"].Value<string>().Should().Be(type);
            var fields = action["fields"];
            fields.Count().Should().Be(hasParameter ? 2 : 1);
            var fileUploadAction = fields[0];
            fileUploadAction.Should().NotBeNull();
            fileUploadAction!["type"]!.Value<string>().Should().Be("file");
            fileUploadAction!["accept"]!.Value<string>().Should().Be(string.Join(",", fileUploadConfiguration.Accept));
            fileUploadAction!["maxFileSizeBytes"]!.Value<long>().Should().Be(fileUploadConfiguration.MaxFileSizeBytes);
            (fileUploadAction!["allowMultiple"]?.Value<bool>() ?? false).Should().Be(fileUploadConfiguration.AllowMultiple);
        }

        private void AssertDefaultValues(JObject action, ActionParameter expectedDefaultValues)
        {
            var fields = action["fields"];
            var value = fields[0]["value"];
            var aInt = value["AInt"].Value<int>();
         
            Assert.AreEqual(1, fields.Count());
            Assert.AreEqual(expectedDefaultValues.AInt, aInt);
        }
        
        private void AssertDynamicDefaultValues(JObject action, string expectedValue)
        {
            var fields = action["fields"];
            var value = fields[0]["value"];
            var property1 = value["Property1"].Value<string>();
         
            Assert.AreEqual(1, fields.Count());
            Assert.AreEqual(expectedValue, property1);
        }

        private void AssertActionArgument(JObject action, string contentType, string actionParameterName, string actionParameterClass, bool hasDefaultValues = false, string  parameterTypeRouteName = RouteNames.ActionParameterTypes, string objectKeyString = null)
        {
            Assert.AreEqual(contentType, action["type"]);
            var fields = (JArray) action["fields"];
            Assert.AreEqual(fields.Count, 1);

            var singleField = (JObject)fields[0];
            
            var expectedProperties = 3;
            if (hasDefaultValues)
            {
                expectedProperties++;
            }
            
            Assert.AreEqual(expectedProperties, singleField.Properties().Count());

            Assert.AreEqual(actionParameterName, singleField["name"]);
            Assert.AreEqual(DefaultMediaTypes.ApplicationJson, singleField["type"]);

            var actionsArray = (JArray)singleField["class"];
            Assert.AreEqual(1, actionsArray.Count);

            var route = ((JValue)actionsArray[0]).Value<string>();
            
            if (objectKeyString == null)
            {
                AssertRoute(route, parameterTypeRouteName, $"{{ parameterTypeName = {actionParameterClass} }}");
            }
            else
            {
                AssertRoute(route, parameterTypeRouteName, objectKeyString);
            }
        }

        private void AssertFileUploadActionArgument(JObject action, string contentType, string actionParameterName, string actionParameterClass, bool hasDefaultValues = false, string  parameterTypeRouteName = RouteNames.ActionParameterTypes, string objectKeyString = null)
        {
            var fields = (JArray) action["fields"];
            fields.Should().HaveCount(2);

            var field = (JObject)fields[1];
            Assert.AreEqual(contentType, field["type"]);
            
            var expectedProperties = 3;
            if (hasDefaultValues)
            {
                expectedProperties++;
            }
            
            Assert.AreEqual(expectedProperties, field.Properties().Count());

            Assert.AreEqual(actionParameterName, field["name"]);
            Assert.AreEqual(DefaultMediaTypes.ApplicationJson, field["type"]);

            var actionsArray = (JArray)field["class"];
            Assert.AreEqual(1, actionsArray.Count);

            var route = ((JValue)actionsArray[0]).Value<string>();
            
            if (objectKeyString == null)
            {
                AssertRoute(route, parameterTypeRouteName, $"{{ parameterTypeName = {actionParameterClass} }}");
            }
            else
            {
                AssertRoute(route, parameterTypeRouteName, objectKeyString);
            }
        }

        private void AssertActionBasic(JObject action, string actionName, string method, string routeName, int propertyCount, string actionClass, string? actionTitle = null)
        {
            Assert.AreEqual(propertyCount, action.Properties().Count());
            Assert.AreEqual(actionName, action["name"]);
            Assert.AreEqual(method, action["method"]);
            
            Assert.AreEqual(actionClass, action["class"].Single());
            AssertRoute(((JValue)action["href"]).Value<string>(), routeName);

            if (!string.IsNullOrEmpty(actionTitle))
            {
                Assert.AreEqual(actionTitle, action["title"]);
            }
        }

        [HypermediaObject(Classes = [nameof(ActionsHypermediaObject)])]
        public class ActionsHypermediaObject : IHypermediaObject
        {
            [FormatterIgnoreHypermediaProperty]
            public HypermediaActionNoArgument ActionToIgnore { get; private set; }         // should not be in siren

            public HypermediaActionNotExecutable ActionNotExecutable { get; private set; } // should not be in siren

            [HypermediaAction(Name = "RenamedAction", Title = "A Title")]
            public HypermediaActionNoArgument ActionToRename { get; private set; }

            public HypermediaActionNoArgument ActionNoArgument { get; private set; }

            public HypermediaActionWithArgument ActionWithArgument { get; private set; }
            
            public ExternalActionNoArgument ExternalActionNoArgument { get; private set; }
            
            public ExternalActionWithArgument ExternalActionWithArgument { get; private set; }
            
            public FileUploadAction FileUploadAction { get; private set; }
            public FileUploadWithParameterAction FileUploadWithParameterAction { get; private set; }
            public ExternalFileUploadAction ExternalFileUploadAction { get; private set; }
            
            public DynamicAction DynamicActionPrefilledParameter_None { get; private set; }
            public DynamicAction DynamicActionPrefilledParameter_String { get; private set; }
            public DynamicAction DynamicActionPrefilledParameter_Object { get; private set; }


            public static readonly Uri  ExternalUri = new Uri(TestUrlConfig.Scheme +"://" + TestUrlConfig.Host + "/ExternalActionRoute");

            public static readonly ActionParameter ActionWithArgumentDefaultValues = new ActionParameter() { AInt = 3 };
            public static readonly ActionParameter  ExternalActionWithArgumentDefaultValues = new ActionParameter{AInt = 4};
            public static readonly string  DynamicActionPrefilledParameter_String_DefaultValues = @"{""Property1"":""3""}";
            public static readonly object  DynamicActionPrefilledParameter_Object_DefaultValues = new {Property1="4"};
            
            public ActionsHypermediaObject()
            {
                ActionToRename = new HypermediaActionNoArgument(() => true);
                ActionToIgnore = new HypermediaActionNoArgument(() => true);
                ActionNotExecutable = new HypermediaActionNotExecutable(() => false);
                ActionNoArgument = new HypermediaActionNoArgument(() => true);
                ActionWithArgument = new HypermediaActionWithArgument(() => true, ActionWithArgumentDefaultValues);
                ExternalActionNoArgument = new ExternalActionNoArgument(ExternalUri, HttpMethods.Post);
                ExternalActionWithArgument = new ExternalActionWithArgument(ExternalUri, HttpMethods.Delete, CustomMediaType, ExternalActionWithArgumentDefaultValues);
                FileUploadAction = new FileUploadAction(() => true, 
                    new FileUploadConfiguration
                    {
                        MaxFileSizeBytes = 14,
                        Accept = new List<string>{".png", ".jpg"},
                        AllowMultiple = true
                    });
                FileUploadWithParameterAction = new FileUploadWithParameterAction(
                    () => true,
                    new FileUploadConfiguration()
                    {
                        MaxFileSizeBytes = 27,
                        Accept = [".bmp"],
                        AllowMultiple = false,
                    });
                ExternalFileUploadAction = new ExternalFileUploadAction(
                    () => true,
                    ExternalUri,
                    HttpMethods.Post,
                    CustomMediaType,
                    new FileUploadConfiguration
                    {
                        MaxFileSizeBytes = 14,
                        Accept = new List<string>{".png", ".jpg"},
                        AllowMultiple = true
                    });

                DynamicActionPrefilledParameter_None = new DynamicAction("Should_not_be_used_no_schema_referenced", false);
                DynamicActionPrefilledParameter_String = new DynamicAction("SchemaKey_1", true, DynamicActionPrefilledParameter_String_DefaultValues);
                DynamicActionPrefilledParameter_Object = new DynamicAction("SchemaKey_2", true, DynamicActionPrefilledParameter_Object_DefaultValues);
            }
        }
    }

    public class HypermediaActionNotExecutable : HypermediaAction
    {
        public HypermediaActionNotExecutable(Func<bool> canExecute) : base(canExecute)
        {
        }

        public override object GetPrefilledParameter()
        {
            return null;
        }
    }

    public class HypermediaActionNoArgument : HypermediaAction
    {
        public HypermediaActionNoArgument(Func<bool> canExecute) : base(canExecute)
        {
        }

        public override object GetPrefilledParameter()
        {
            return null;
        }
    }

    public class HypermediaActionWithArgument : HypermediaAction<ActionParameter>
    {
        public HypermediaActionWithArgument(Func<bool> canExecute, ActionParameter defaultValues) : base(canExecute, defaultValues)
        {
        }
    }
    
    public class FileUploadAction : FileUploadHypermediaAction
    {
        public FileUploadAction(Func<bool> canExecute, FileUploadConfiguration? fileUploadConfiguration = null) : base(canExecute, fileUploadConfiguration)
        {
        }
    }

    public class FileUploadWithParameterAction : FileUploadHypermediaAction<ActionParameter>
    {
        public FileUploadWithParameterAction(Func<bool> canExecute,
            FileUploadConfiguration? fileUploadConfiguration = null)
            : base(canExecute, fileUploadConfiguration)
        {
        }
    }
    
    public class ExternalFileUploadAction : ExternalFileUploadHypermediaAction
    {
        public ExternalFileUploadAction(
            Func<bool> canExecute,
            Uri externalUri,
            string httpMethod,
            string acceptedMediaType,
            FileUploadConfiguration fileUploadConfiguration = null) : base(canExecute, externalUri, httpMethod, acceptedMediaType, fileUploadConfiguration)
        {
        }
    }
    
    public class ExternalActionNoArgument : HypermediaExternalAction
    {
        public ExternalActionNoArgument(Uri externalUri,
            string httpMethod) : base(() => true,
            externalUri,
            httpMethod)
        {
        }
    }
    
    public class ExternalActionWithArgument : HypermediaExternalAction<ActionParameter>
    {
        public ExternalActionWithArgument(Uri externalUri,
            string httpMethod,
            string acceptedMediaType,
            ActionParameter defaultValues = null) : base(() => true,
            externalUri,
            httpMethod,
            acceptedMediaType,
            defaultValues)
        {
        }
    }
    
    public class DynamicAction : DynamicHypermediaAction<DynamicParameter>
    {
        public DynamicAction(string schemaRouteValue, bool hasParameters, object prefilledValues = null)
            : base(hasParameters, prefilledValues)
        {
            SchemaRouteKeys = new {SchemaRouteValue=schemaRouteValue};
        }
    }

    public class ActionParameter : IHypermediaActionParameter
    {
        public int AInt { get; set; }
    }

    public class RegisteredActionParameter : IHypermediaActionParameter
    {
        public int AInt { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class DynamicParameter : IHypermediaActionParameter { }
}
