using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OLinq.Tests
{
    using NOS = NotificationObject<string>;

    [TestClass]
    public class GroupByTests
    {
        ObservableCollection<NOS> collection;
        GroupByOperation<NOS, string> operation;
        ObservableBuffer<IGrouping<string, NOS>> buffer;

        [TestInitialize]
        public void setupFilters()
        {
            collection = new ObservableCollection<NOS>()
            {
                new NOS( "Group1", "Item1" ),
                new NOS( "Group1", "Item2" ),
                new NOS( "Group1", "Item3" ),
                new NOS( "Group2", "Item4" ),
                new NOS( "Group2", "Item5" ),
                new NOS( "Group2", "Item6" ),
            };

            operation = new GroupByOperation<NOS, string>(new OperationContext(),
                Expression.Call(
                    typeof(Queryable).GetMethods()
                        .Where(i => i.Name == "GroupBy")
                        .Where(i => i.IsGenericMethodDefinition)
                        .Where(i => i.GetGenericArguments().Length == 2)
                        .Select(i => i.MakeGenericMethod(typeof(NOS), typeof(string)))
                        .Where(i => i.GetParameters().Length == 2)
                        .Where(i => i.GetParameters()[1].ParameterType == typeof(Expression<Func<NOS, string>>))
                        .Single(),
                    new ObservableQuery<NOS>(collection).Expression,
                    Expression.Lambda<Func<NOS, string>>(
                        Expression.MakeMemberAccess(
                            Expression.Parameter(typeof(NOS), "p"),
                            typeof(NOS).GetProperty("Value1")),
                        Expression.Parameter(typeof(NOS), "p"))));

            buffer = operation.AsObservableQuery().ToObservableView().ToBuffer();
        }


        [TestMethod]
        public void GroupByWorks()
        {
            Assert.AreEqual(2, buffer.Count());
            Assert.AreEqual(3, buffer.ElementAt(0).Count());
            Assert.AreEqual(3, buffer.ElementAt(1).Count());
        }

        [TestMethod]
        public void GrossNotificationsWork()
        {
            collection.Add(new NOS("Group1", "Item7"));
            Assert.AreEqual(2, buffer.Count());
            Assert.AreEqual(4, buffer.ElementAt(0).Count());
            Assert.AreEqual(3, buffer.ElementAt(1).Count());

            collection.Add(new NOS("Group2", "Item8"));
            Assert.AreEqual(2, buffer.Count());
            Assert.AreEqual(4, buffer.ElementAt(0).Count());
            Assert.AreEqual(4, buffer.ElementAt(1).Count());

            collection[0].Value1 = "Group3";
            Assert.AreEqual(3, buffer.Count());
            Assert.AreEqual(3, buffer.ElementAt(0).Count());
            Assert.AreEqual(4, buffer.ElementAt(1).Count());
        }

        [TestMethod]
        public void NotYetImplemented_OrderIsPreservedOnGroupAddition()
        {
            Assert.AreEqual("Group1", buffer.ElementAt(0).Key);
            Assert.AreEqual("Group2", buffer.ElementAt(1).Key);

            collection.Add(new NOS("Group3", "Item7"));
            Assert.AreEqual(3, buffer.Count());
            Assert.AreEqual("Group3", buffer.ElementAt(2).Key);

            collection.Insert(1, new NOS("Group4", "Item8"));
            Assert.AreEqual(4, buffer.Count());
            Assert.AreEqual("Group1", buffer.ElementAt(0).Key);
            Assert.AreEqual("Group4", buffer.ElementAt(1).Key);
            Assert.AreEqual("Group2", buffer.ElementAt(2).Key);
            Assert.AreEqual("Group3", buffer.ElementAt(3).Key);
        }

        [TestMethod]
        public void NotYetImplemented_OrderIsPreservedOnGroupRemoval()
        {
            collection.Add(new NOS("Group3", "Item7"));
            collection.Add(new NOS("Group4", "Item8"));

            collection.RemoveAt(1);
            Assert.AreEqual(3, buffer.Count());
            Assert.AreEqual("Group1", buffer.ElementAt(0).Key);
            Assert.AreEqual("Group3", buffer.ElementAt(1).Key);
            Assert.AreEqual("Group4", buffer.ElementAt(2).Key);
        }
    }
}
