using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Filters.Extensions;
using HotChocolate.Utilities;
using Xunit;

namespace HotChocolate.Types.Filters;

[Obsolete]
public class FilterFieldCollectionExtensionTest
{
    private readonly Expression<Func<Foo, string>> _property;
    private readonly PropertyInfo _propertyInfo;
    private readonly IDescriptorContext _descriptorContext;

    public FilterFieldCollectionExtensionTest()
    {
        _property = x => x.Bar;
        _propertyInfo = (PropertyInfo)_property.ExtractMember();
        _descriptorContext = DescriptorContext.Create();
    }

    [Fact]
    public void GetOrAddDescriptor_Argument_Fields()
    {
        //arrange
        var factory =
            () => new StringFilterFieldDescriptor(_descriptorContext, _propertyInfo);

        //act
        //assert
        var assertNullException =
            Assert.Throws<ArgumentNullException>(
                () => FilterFieldCollectionExtensions.GetOrAddDescriptor(
                    null, _propertyInfo, factory));
        Assert.Equal("fields", assertNullException.ParamName);
    }


    [Fact]
    public void GetOrAddDescriptor_Argument_PropertyInfo()
    {
        //arrange
        var factory =
            () => new StringFilterFieldDescriptor(_descriptorContext, _propertyInfo);
        IList<FilterFieldDescriptorBase> list = new List<FilterFieldDescriptorBase>();

        //act
        //assert
        var assertNullException =
            Assert.Throws<ArgumentNullException>(() => list.GetOrAddDescriptor(null, factory));
        Assert.Equal("propertyInfo", assertNullException.ParamName);
    }


    [Fact]
    public void GetOrAddDescriptor_Argument_Factory()
    {
        //arrange
        IList<FilterFieldDescriptorBase> list = new List<FilterFieldDescriptorBase>();

        //act
        //assert
        var assertNullException =
            Assert.Throws<ArgumentNullException>(
                () => list.GetOrAddDescriptor<StringFilterFieldDescriptor>(
                    _propertyInfo, null));

        Assert.Equal("valueFactory", assertNullException.ParamName);
    }

    [Fact]
    public void GetOrAddDescriptor_Should_AddDescriptorIfNotExists()
    {
        //arrange
        IList<FilterFieldDescriptorBase> list = new List<FilterFieldDescriptorBase>();
        var descriptor = new StringFilterFieldDescriptor(_descriptorContext, _propertyInfo);
        var valueFactory = () => descriptor;

        //act
        var result =
            list.GetOrAddDescriptor(_propertyInfo, valueFactory);

        //assert
        Assert.Single(list);
        Assert.Same(descriptor, list[0]);
        Assert.Same(descriptor, result);
    }

    [Fact]
    public void GetOrAddDescriptor_Should_ReturnDescriptorIfAlreadyExists()
    {
        //arrange
        IList<FilterFieldDescriptorBase> list = new List<FilterFieldDescriptorBase>();
        var descriptorShouldNotBeRemoved =
            new StringFilterFieldDescriptor(_descriptorContext, _propertyInfo);
        var newDescriptorShouldNotHaveAnyEffect =
            new StringFilterFieldDescriptor(_descriptorContext, _propertyInfo);
        var valueFactory =
            () => newDescriptorShouldNotHaveAnyEffect;
        list.Add(descriptorShouldNotBeRemoved);

        //act
        var result =
            list.GetOrAddDescriptor(_propertyInfo, valueFactory);

        //assert
        Assert.Single(list);
        Assert.Same(descriptorShouldNotBeRemoved, list[0]);
        Assert.Same(descriptorShouldNotBeRemoved, result);
        Assert.NotSame(newDescriptorShouldNotHaveAnyEffect, result);
    }

    [Fact]
    public void GetOrAddDescriptor_Should_ReplaceDescriptorIfDifferentType()
    {
        //arrange
        IList<FilterFieldDescriptorBase> list = new List<FilterFieldDescriptorBase>();
        var descriptorToRemove =
            new IgnoredFilterFieldDescriptor(_descriptorContext, _propertyInfo);
        var descriptorToAdd =
            new StringFilterFieldDescriptor(_descriptorContext, _propertyInfo);
        var valueFactory = () => descriptorToAdd;

        list.Add(descriptorToRemove);

        //act
        var result =
            list.GetOrAddDescriptor(_propertyInfo, valueFactory);

        //assert
        Assert.Single(list);
        Assert.Same(descriptorToAdd, list[0]);
        Assert.Same(descriptorToAdd, result);
        Assert.NotSame(descriptorToRemove, result);
    }


    [Fact]
    public void GetOrAddOperation_Argument_Fields()
    {
        //arrange
        Func<FilterOperationDescriptorBase> factory =
            () => CreateOperation(FilterOperationKind.Equals);

        //act
        //assert
        var assertNullException =
            Assert.Throws<ArgumentNullException>(
                () => FilterFieldCollectionExtensions.GetOrAddOperation(
                    null, FilterOperationKind.Equals, factory));
        Assert.Equal("fields", assertNullException.ParamName);
    }


    [Fact]
    public void GetOrAddOperation_Argument_Factory()
    {
        //arrange
        IList<FilterOperationDescriptorBase> list = new List<FilterOperationDescriptorBase>();
        Func<FilterOperationDescriptorBase> factory =
            () => CreateOperation(FilterOperationKind.Equals);

        //act
        //assert
        var assertNullException =
            Assert.Throws<ArgumentNullException>(
                () => list.GetOrAddOperation<BooleanFilterOperationDescriptor>(
                    FilterOperationKind.Equals, null));
        Assert.Equal("valueFactory", assertNullException.ParamName);
    }

    [Fact]
    public void GetOrAddOperation_Should_AddDescriptorIfNotExists()
    {
        //arrange
        IList<FilterOperationDescriptorBase> list = new List<FilterOperationDescriptorBase>();
        var operation =
            CreateOperation(FilterOperationKind.Equals);
        var valueFactory = () => operation;

        //act
        var result =
            list.GetOrAddOperation(FilterOperationKind.Equals, valueFactory);

        //assert
        Assert.Single(list);
        Assert.Same(operation, list[0]);
        Assert.Same(operation, result);
    }

    [Fact]
    public void GetOrAddOperation_Should_ReturnDescriptorIfAlreadyExists()
    {
        //arrange
        IList<FilterOperationDescriptorBase> list = new List<FilterOperationDescriptorBase>();
        var descriptorShouldNotBeRemoved =
            CreateOperation(FilterOperationKind.Equals);
        var newDescriptorShouldNotHaveAnyEffect =
            CreateOperation(FilterOperationKind.Equals);
        var valueFactory =
            () => newDescriptorShouldNotHaveAnyEffect;

        list.Add(descriptorShouldNotBeRemoved);

        //act
        var result =
            list.GetOrAddOperation(FilterOperationKind.Equals, valueFactory);

        //assert
        Assert.Single(list);
        Assert.Same(descriptorShouldNotBeRemoved, list[0]);
        Assert.Same(descriptorShouldNotBeRemoved, result);
        Assert.NotSame(newDescriptorShouldNotHaveAnyEffect, result);
    }


    [Fact]
    public void GetOrAddOperation_Throws_ExceptionIfExistingDescriptorIsOfDifferentType()
    {
        //arrange
        IList<FilterOperationDescriptorBase> list = new List<FilterOperationDescriptorBase>();
        var descriptorShouldNotBeRemoved =
            CreateOperation(FilterOperationKind.Equals);
        var newDescriptorShouldThrowException =
            CreateComparableOperation(FilterOperationKind.Equals);
        var valueFactory =
            () => newDescriptorShouldThrowException;

        list.Add(descriptorShouldNotBeRemoved);

        //act

        //assert
        Assert.Throws<SchemaException>(() =>
            list.GetOrAddOperation(FilterOperationKind.Equals, valueFactory));
    }



    private BooleanFilterOperationDescriptor CreateOperation(
        FilterOperationKind operationKind)
    {
        var descirptor = new BooleanFilterFieldDescriptor(
            _descriptorContext, _propertyInfo);

        var typeReference = _descriptorContext.TypeInspector.GetTypeRef(
            typeof(Foo),
            TypeContext.Input);
        var definition = new FilterOperationDefinition()
        {
            Name = "Foo",
            Type = typeReference,
            Operation = new FilterOperation(typeof(string), operationKind, _propertyInfo),
            Property = _propertyInfo
        };

        var operation = new FilterOperation(
            typeof(bool),
            operationKind,
            definition.Property);

        return BooleanFilterOperationDescriptor.New(
            _descriptorContext,
            descirptor,
            "Foo",
            typeReference,
            operation);
    }


    private ComparableFilterOperationDescriptor CreateComparableOperation(
        FilterOperationKind operationKind)
    {
        var descriptor = new ComparableFilterFieldDescriptor(
            _descriptorContext, _propertyInfo);
        var typeReference = _descriptorContext.TypeInspector.GetTypeRef(
            typeof(Foo),
            TypeContext.Input);
        var definition = new FilterOperationDefinition()
        {
            Name = "Foo",
            Type = typeReference,
            Operation = new FilterOperation(typeof(string), operationKind, _propertyInfo),
            Property = _propertyInfo
        };

        var operation = new FilterOperation(
            typeof(bool),
            operationKind,
            definition.Property);

        return ComparableFilterOperationDescriptor.New(
            _descriptorContext,
            descriptor,
            "Foo",
            typeReference,
            operation);
    }

    private sealed class Foo
    {
        public string Bar { get; set; }
    }

}
