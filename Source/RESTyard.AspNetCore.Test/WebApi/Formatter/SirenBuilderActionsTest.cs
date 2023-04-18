using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.WebApi.Formatter;
using RESTyard.AspNetCore.WebApi.RouteResolver;
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
            RouteRegister.AddHypermediaObjectRoute(typeof(ActionsHypermediaObject), routeName, HttpMethod.GET);

            var routeNameHypermediaActionNotExecutable = nameof(HypermediaActionNotExecutable) + "_Route";
            RouteRegister.AddActionRoute(typeof(HypermediaActionNotExecutable), routeNameHypermediaActionNotExecutable, HttpMethod.POST);

            var routeNameHypermediaActionNoArgument = nameof(HypermediaActionNoArgument) + "_Route";
            RouteRegister.AddActionRoute(typeof(HypermediaActionNoArgument), routeNameHypermediaActionNoArgument, HttpMethod.POST);

            var routeNameHypermediaActionWithArgument = nameof(HypermediaActionWithArgument) + "_Route";
            
            RouteRegister.AddActionRoute(typeof(HypermediaActionWithArgument), routeNameHypermediaActionWithArgument, HttpMethod.POST, CustomMediaType);

            var routeNameRegisteredActionParameter = nameof(RegisteredActionParameter) + "_Route";
            RouteRegister.AddParameterTypeRoute(typeof(RegisteredActionParameter), routeNameRegisteredActionParameter, HttpMethod.GET);
            
            var routeNameFileUpload = nameof(FileUploadAction) + "_Route";
            RouteRegister.AddActionRoute(typeof(FileUploadAction), routeNameFileUpload, HttpMethod.POST, DefaultMediaTypes.MultipartFormData);
            
            var ho = new ActionsHypermediaObject();

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(ActionsHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var actionsArray = (JArray) siren["actions"];
            Assert.AreEqual(7, actionsArray!.Count);
            AssertActionBasic((JObject)siren["actions"]![0], "RenamedAction", "POST", routeNameHypermediaActionNoArgument, 5,  ActionClasses.ParameterLessActionClass, "A Title");
            AssertActionBasic((JObject)siren["actions"][1], "ActionNoArgument", "POST", routeNameHypermediaActionNoArgument, 4,  ActionClasses.ParameterLessActionClass);

            AssertActionBasic((JObject)siren["actions"][2], "ActionWithArgument", "POST", routeNameHypermediaActionWithArgument, 6,  ActionClasses.ParameterActionClass);
            AssertActionArgument((JObject) siren["actions"][2], CustomMediaType,  nameof(ActionParameter), nameof(ActionParameter),hasDefaultValues:true);
            AssertDefaultValues((JObject) siren["actions"][2], ActionsHypermediaObject.ActionWithArgumentDefaultValues);
            
            AssertActionBasic((JObject)siren["actions"][3], nameof(ExternalActionNoArgument), "POST", "ExternalActionRoute", 4,  ActionClasses.ParameterLessActionClass);
            
            AssertActionBasic((JObject)siren["actions"][4], nameof(ExternalActionWithArgument), "DELETE", "ExternalActionRoute", 6,  ActionClasses.ParameterActionClass);
            AssertActionArgument((JObject) siren["actions"][4], CustomMediaType, nameof(ActionParameter),  nameof(ActionParameter),hasDefaultValues:true);
            AssertDefaultValues((JObject) siren["actions"][4], ActionsHypermediaObject.ExternalActionWithArgumentDefaultValues);
            
            AssertActionBasic((JObject)siren["actions"][5], nameof(FileUploadAction), "POST", routeNameFileUpload, 6,  ActionClasses.FileUploadActionClass);
            AssertFileUploadAction((JObject)siren["actions"][5], ho.FileUploadAction.FileUploadConfiguration, DefaultMediaTypes.MultipartFormData);
            
            AssertActionBasic((JObject)siren["actions"][6], nameof(ExternalFileUploadAction), "POST", "ExternalActionRoute", 6,  ActionClasses.FileUploadActionClass);
            AssertFileUploadAction((JObject)siren["actions"][6], ho.FileUploadAction.FileUploadConfiguration, CustomMediaType);
        }

        private void AssertFileUploadAction(JObject action, FileUploadConfiguration fileUploadConfiguration, string type)
        {
            action["type"].Value<string>().Should().Be(type);
            var fields = action["fields"];
            fields.Count().Should().Be(1);
            var fileUploadAction = fields[0];
            fileUploadAction.Should().NotBeNull();
            fileUploadAction!["type"]!.Value<string>().Should().Be("file");
            fileUploadAction!["accept"]!.Value<string>().Should().Be(string.Join(",", fileUploadConfiguration.Accept));
            fileUploadAction!["maxFileSizeBytes"]!.Value<long>().Should().Be(fileUploadConfiguration.MaxFileSizeBytes);
            fileUploadAction!["allowMultiple"]!.Value<bool>().Should().Be(fileUploadConfiguration.AllowMultiple);
        }

        private void AssertDefaultValues(JObject action, ActionParameter expectedDefaultValues)
        {
            var fields = action["fields"];
            var value = fields[0]["value"];
            var aInt = value["AInt"].Value<int>();
         
            Assert.AreEqual(1, fields.Count());
            Assert.AreEqual(expectedDefaultValues.AInt, aInt);
        }

        private void AssertActionArgument(JObject action, string contentType, string actionParameterName, string actionParameterClass, bool classIsRoute = false, bool hasDefaultValues = false)
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
            if (!classIsRoute)
            {
                AssertRoute(route, "ActionParameterTypes", $"{{ parameterTypeName = {actionParameterClass} }}");
            }
            else
            {
                AssertRoute(route, actionParameterClass);
            }
        }

        private void AssertActionBasic(JObject action, string actionName, string method, string routeName, int propertyCount, string actionClass, string actionTitle = null)
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

        public class ActionsHypermediaObject : HypermediaObject
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
            public ExternalFileUploadAction ExternalFileUploadAction { get; private set; }


            public static readonly Uri  ExternalUri = new Uri(TestUrlConfig.Scheme +"://" + TestUrlConfig.Host + "/ExternalActionRoute");

            public static readonly ActionParameter ActionWithArgumentDefaultValues = new ActionParameter() { AInt = 3 };
            public static readonly ActionParameter  ExternalActionWithArgumentDefaultValues = new ActionParameter{AInt = 4};
            
            public ActionsHypermediaObject()
            {
                ActionToRename = new HypermediaActionNoArgument(() => true);
                ActionToIgnore = new HypermediaActionNoArgument(() => true);
                ActionNotExecutable = new HypermediaActionNotExecutable(() => false);
                ActionNoArgument = new HypermediaActionNoArgument(() => true);
                ActionWithArgument = new HypermediaActionWithArgument(() => true, ActionWithArgumentDefaultValues);
                ExternalActionNoArgument = new ExternalActionNoArgument(ExternalUri, HttpMethod.POST);
                ExternalActionWithArgument = new ExternalActionWithArgument(ExternalUri, HttpMethod.DELETE, CustomMediaType, ExternalActionWithArgumentDefaultValues);
                FileUploadAction = new FileUploadAction(() => true, 
                    new FileUploadConfiguration
                    {
                        MaxFileSizeBytes = 14,
                        Accept = new List<string>{".png", ".jpg"},
                        AllowMultiple = true
                    });     
                ExternalFileUploadAction = new ExternalFileUploadAction(
                    () => true,
                    ExternalUri,
                    HttpMethod.POST,
                    CustomMediaType,
                    new FileUploadConfiguration
                    {
                        MaxFileSizeBytes = 14,
                        Accept = new List<string>{".png", ".jpg"},
                        AllowMultiple = true
                    });
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
        public FileUploadAction(Func<bool> canExecute, FileUploadConfiguration fileUploadConfiguration = null) : base(canExecute, fileUploadConfiguration)
        {
        }
    }
    
    public class ExternalFileUploadAction : ExternalFileUploadHypermediaAction
    {
        public ExternalFileUploadAction(
            Func<bool> canExecute,
            Uri externalUri,
            HttpMethod httpMethod,
            string acceptedMediaType,
            FileUploadConfiguration fileUploadConfiguration = null) : base(canExecute, externalUri, httpMethod, acceptedMediaType, fileUploadConfiguration)
        {
        }
    }
    
    public class ExternalActionNoArgument : HypermediaExternalAction
    {
        public ExternalActionNoArgument(Uri externalUri,
            HttpMethod httpMethod) : base(() => true,
            externalUri,
            httpMethod)
        {
        }
    }
    
    public class ExternalActionWithArgument : HypermediaExternalAction<ActionParameter>
    {
        public ExternalActionWithArgument(Uri externalUri,
            HttpMethod httpMethod,
            string acceptedMediaType,
            ActionParameter defaultValues = null) : base(() => true,
            externalUri,
            httpMethod,
            acceptedMediaType,
            defaultValues)
        {
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
}
