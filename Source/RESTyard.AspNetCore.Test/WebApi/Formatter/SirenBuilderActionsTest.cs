using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter
{
    [TestClass]
    public class SirenBuilderActionsTest : SirenBuilderTestBase
    {
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
            var routeName = typeof(ActionsHypermediaObject).Name + "_Route";
            RouteRegister.AddHypermediaObjectRoute(typeof(ActionsHypermediaObject), routeName, HttpMethod.GET);

            var routeNameHypermediaActionNotExecutable = typeof(HypermediaActionNotExecutable).Name + "_Route";
            RouteRegister.AddActionRoute(typeof(HypermediaActionNotExecutable), routeNameHypermediaActionNotExecutable, HttpMethod.POST);

            var routeNameHypermediaActionNoArgument = typeof(HypermediaActionNoArgument).Name + "_Route";
            RouteRegister.AddActionRoute(typeof(HypermediaActionNoArgument), routeNameHypermediaActionNoArgument, HttpMethod.POST);

            var routeNameHypermediaActionWithArgument = typeof(HypermediaActionWithArgument).Name + "_Route";
            var customMediaType = "custome/mediatype";
            RouteRegister.AddActionRoute(typeof(HypermediaActionWithArgument), routeNameHypermediaActionWithArgument, HttpMethod.POST, customMediaType);

            var routeNameRegisteredActionParameter = typeof(RegisteredActionParameter).Name + "_Route";
            RouteRegister.AddParameterTypeRoute(typeof(RegisteredActionParameter), routeNameRegisteredActionParameter, HttpMethod.GET);

            var ho = new ActionsHypermediaObject();

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(ActionsHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var actionsArray = (JArray) siren["actions"];
            Assert.AreEqual(5, actionsArray.Count);
            AssertActionBasic((JObject)siren["actions"][0], "RenamedAction", "POST", routeNameHypermediaActionNoArgument, 4,  "A Title");
            AssertActionBasic((JObject)siren["actions"][1], "ActionNoArgument", "POST", routeNameHypermediaActionNoArgument, 3);

            AssertActionBasic((JObject)siren["actions"][2], "ActionWithArgument", "POST", routeNameHypermediaActionWithArgument, 5);
            AssertActionArgument((JObject) siren["actions"][2], customMediaType,  nameof(ActionParameter), nameof(ActionParameter));
            AssertDefaultValues((JObject) siren["actions"][2], ActionsHypermediaObject.ActionWithArgumentDefaultValues);
            
            AssertActionBasic((JObject)siren["actions"][3], "ExternalActionNoArgument", "POST", ActionsHypermediaObject.ExternalUri.ToString(), 5);
            AssertActionArgument((JObject) siren["actions"][3], customMediaType,  nameof(ActionParameter), nameof(ActionParameter));
            
            AssertActionBasic((JObject)siren["actions"][4], "ExternalActionWithArgument", "DELETE", ActionsHypermediaObject.ExternalUri.ToString(), 5);
            AssertActionArgument((JObject) siren["actions"][4], customMediaType, nameof(ActionParameter),  nameof(ActionParameter));
            AssertDefaultValues((JObject) siren["actions"][4], ActionsHypermediaObject.ExternalActionWithArgumentDefaultValues);
        }

        private void AssertDefaultValues(JObject action, ActionParameter expecteddefaultValues)
        {
            Assert.Fail("Check default values");
        }

        private void AssertActionArgument(JObject action, string contentType, string actionParameterName, string actionParameterClass, bool classIsRoute = false)
        {
            Assert.AreEqual(action["type"], contentType);
            var fields = (JArray) action["fields"];
            Assert.AreEqual(fields.Count, 1);

            var singleField = (JObject)fields[0];
            Assert.AreEqual(singleField.Properties().Count(), 3);

            Assert.AreEqual(singleField["name"], actionParameterName);
            Assert.AreEqual(singleField["type"], DefaultMediaTypes.ApplicationJson);

            var actionsArray = (JArray)singleField["class"];
            Assert.AreEqual(actionsArray.Count, 1);

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

        private void AssertActionBasic(JObject action, string actionName, string method, string routeName, int propertyCount, string actionTitle = null)
        {
            Assert.AreEqual(action.Properties().Count(), propertyCount);
            Assert.AreEqual(action["name"], actionName);
            Assert.AreEqual(action["method"], method);
            AssertRoute(((JValue)action["href"]).Value<string>(), routeName);

            if (!string.IsNullOrEmpty(actionTitle))
            {
                Assert.AreEqual(action["title"], actionTitle);
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


            public static readonly Uri  ExternalUri = new Uri("http://example.com");

            public static readonly ActionParameter ActionWithArgumentDefaultValues = new ActionParameter() { AInt = 3 };
            public static readonly ActionParameter  ExternalActionWithArgumentDefaultValues = new ActionParameter{AInt = 4};
            
            public ActionsHypermediaObject()
            {
                ActionToRename = new HypermediaActionNoArgument(() => true);
                ActionToIgnore = new HypermediaActionNoArgument(() => true);
                ActionNotExecutable = new HypermediaActionNotExecutable(() => false);
                ActionNoArgument = new HypermediaActionNoArgument(() => true);
                ActionWithArgument = new HypermediaActionWithArgument(() => true, ActionWithArgumentDefaultValues);
                ExternalActionNoArgument = new ExternalActionNoArgument(ExternalUri, HttpMethod.POST, DefaultMediaTypes.ApplicationJson);
                ExternalActionWithArgument = new ExternalActionWithArgument(ExternalUri, HttpMethod.DELETE, DefaultMediaTypes.ApplicationJson, ExternalActionWithArgumentDefaultValues);
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
    
    public class ExternalActionNoArgument : HypermediaExternalAction
    {
        public ExternalActionNoArgument(Uri externalUri,
            HttpMethod httpMethod,
            string mediaType) : base(() => true,
            externalUri,
            httpMethod,
            mediaType)
        {
        }
    }
    
    public class ExternalActionWithArgument : HypermediaExternalAction<ActionParameter>
    {
        public ExternalActionWithArgument(Uri externalUri,
            HttpMethod httpMethod,
            string mediaType,
            ActionParameter defaultValues = null) : base(() => true,
            externalUri,
            httpMethod,
            mediaType,
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
