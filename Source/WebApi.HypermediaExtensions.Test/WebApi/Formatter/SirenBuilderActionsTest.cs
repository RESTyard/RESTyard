using System;
using System.Linq;
using Bluehands.Hypermedia.MediaTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.WebApi.Extensions.Hypermedia;
using RESTyard.WebApi.Extensions.Hypermedia.Actions;
using RESTyard.WebApi.Extensions.Hypermedia.Attributes;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.Test.WebApi.Formatter
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

            var routeNameHypermediaActionWithTypedArgument = typeof(HypermediaFunctionWithTypedArgument).Name + "_Route";
            RouteRegister.AddActionRoute(typeof(HypermediaFunctionWithTypedArgument), routeNameHypermediaActionWithTypedArgument, HttpMethod.POST);

            var routeNameHypermediaActionFuncNoArgument = typeof(HypermediaFunctionNoArgument).Name + "_Route";
            RouteRegister.AddActionRoute(typeof(HypermediaFunctionNoArgument), routeNameHypermediaActionFuncNoArgument, HttpMethod.POST);

            var routeNameRegisteredActionParameter = typeof(RegisteredActionParameter).Name + "_Route";
            RouteRegister.AddParameterTypeRoute(typeof(RegisteredActionParameter), routeNameRegisteredActionParameter, HttpMethod.GET);



            var ho = new ActionsHypermediaObject();

            var siren = SirenConverter.ConvertToJson(ho);

            AssertDefaultClassName(siren, typeof(ActionsHypermediaObject));
            AssertEmptyProperties(siren);
            AssertEmptyEntities(siren);
            AssertHasOnlySelfLink(siren, routeName);

            var actionsArray = (JArray) siren["actions"];
            Assert.AreEqual(actionsArray.Count, 5);
            AssertActionBasic((JObject)siren["actions"][0], "RenamedAction", "POST", routeNameHypermediaActionNoArgument, 4,  "A Title");
            AssertActionBasic((JObject)siren["actions"][1], "ActionNoArgument", "POST", routeNameHypermediaActionNoArgument, 3);

            AssertActionBasic((JObject)siren["actions"][2], "ActionWithArgument", "POST", routeNameHypermediaActionWithArgument, 5);
            AssertActionArgument((JObject) siren["actions"][2], customMediaType, "ActionParameter", "ActionParameter");

            AssertActionBasic((JObject)siren["actions"][3], "FunctionWithTypedArgument", "POST", routeNameHypermediaActionWithTypedArgument, 5);
            AssertActionArgument((JObject)siren["actions"][3], DefaultMediaTypes.ApplicationJson, "RegisteredActionParameter", routeNameRegisteredActionParameter, true);

            AssertActionBasic((JObject)siren["actions"][4], "FunctionNoArgument", "POST", routeNameHypermediaActionFuncNoArgument, 3);
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

            public HypermediaFunctionWithTypedArgument FunctionWithTypedArgument { get; private set; }

            public HypermediaFunctionNoArgument FunctionNoArgument { get; private set; }

            [FormatterIgnoreHypermediaProperty]
            public int ActionToRenameCallCount { get; set; }

            [FormatterIgnoreHypermediaProperty]
            public int ActionToIgnoreCallCount { get; set; }

            [FormatterIgnoreHypermediaProperty]
            public int ActionNotExecutableCallCount { get; set; }

            [FormatterIgnoreHypermediaProperty]
            public int ActionNoArgumentCallCount { get; set; }

            [FormatterIgnoreHypermediaProperty]
            public int ActionWithArgumentCallCount { get; set; }

            [FormatterIgnoreHypermediaProperty]
            public int ActionWithTypedArgumentCallCount { get; set; }

            public ActionsHypermediaObject()
            {
                ActionToRename = new HypermediaActionNoArgument(() => true, () => ActionToRenameCallCount++);
                ActionToIgnore = new HypermediaActionNoArgument(() => true, () => ActionToIgnoreCallCount++);
                ActionNotExecutable = new HypermediaActionNotExecutable(() => false, () => ActionNotExecutableCallCount++);
                ActionNoArgument = new HypermediaActionNoArgument(() => true, () => ActionNoArgumentCallCount++);
                ActionWithArgument = new HypermediaActionWithArgument(() => true, argument => ActionWithArgumentCallCount++);
                FunctionWithTypedArgument = new HypermediaFunctionWithTypedArgument(() => true, argument =>
                {
                    ActionWithTypedArgumentCallCount++;
                    return true;
                });

                FunctionNoArgument = new HypermediaFunctionNoArgument(() => true);
            }
        }
    }

    public class HypermediaActionNotExecutable : HypermediaAction
    {
        public HypermediaActionNotExecutable(Func<bool> canExecute, Action command) : base(canExecute, command)
        {
        }

        public override object GetPrefilledParameter()
        {
            return null;
        }
    }

    public class HypermediaActionNoArgument : HypermediaAction
    {
        public HypermediaActionNoArgument(Func<bool> canExecute, Action command) : base(canExecute, command)
        {
        }

        public override object GetPrefilledParameter()
        {
            return null;
        }
    }

    public class HypermediaActionWithArgument : HypermediaAction<ActionParameter>
    {
        public HypermediaActionWithArgument(Func<bool> canExecute, Action<ActionParameter> command = null) : base(canExecute, command)
        {
        }
    }

    public class HypermediaFunctionNoArgument : HypermediaFunction<bool>
    {
        public HypermediaFunctionNoArgument(Func<bool> canExecute, Func<bool> command = null) : base(canExecute, command)
        {
        }
    }

    public class HypermediaFunctionWithTypedArgument : HypermediaFunction<RegisteredActionParameter, bool>
    {
        public HypermediaFunctionWithTypedArgument(Func<bool> canExecute, Func<RegisteredActionParameter, bool> command = null) : base(canExecute, command)
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
