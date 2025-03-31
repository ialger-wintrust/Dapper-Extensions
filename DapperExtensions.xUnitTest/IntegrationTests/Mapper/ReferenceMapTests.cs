using DapperExtensions.Mapper;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions.xUnitTest.IntegrationTests.Mapper;

public class ReferenceMapTests
{
    private class ReferenceMapTestEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }

        public ReferencedMapTestEntity entity { get; set; }
    }

    private class ReferencedMapTestEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }

    private class ReferenceMapTestEntityMapper : ClassMapper<ReferenceMapTestEntity>
    {
        public ReferenceMapTestEntityMapper()
        {
            base.ReferenceMap(r => r.Message);
        }
    }

    //[Fact]
    //public void SetParentIdentity_ShouldSetTheParentIdentityOnTheReferenceMap()
    //{
    //    var expectedGuid = Guid.NewGuid();
    //    var testReferenceMap = new ReferenceMapTestEntityMapper();

    //    testReferenceMap.SetParentIdentity(expectedGuid);

    //    var actualGuid = testReferenceMap.ParentIdentity;
    //    Assert.Equal(expectedGuid, actualGuid);
    //}

    [Fact]
    public void SetParentIdentity_ShouldSetTheParentIdentityOfTheReferenceMap()
    {
        var expectedGuid = Guid.NewGuid();

        Expression<Func<ReferenceMapTestEntity, object>> expression = entity => entity.Message;
        var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;

        var testReferenceMap = new ReferenceMap<ReferenceMapTestEntity>(propertyInfo, Guid.NewGuid());

        testReferenceMap.SetParentIdentity(expectedGuid);

        var actualGuid = testReferenceMap.ParentIdentity;
        Assert.Equal(expectedGuid, actualGuid);
    }

    [Fact]
    public void SetIdentity_ShouldSetTheIdentityOfTheReferenceMap()
    {
        var expectedGuid = Guid.NewGuid();

        Expression<Func<ReferenceMapTestEntity, object>> expression = entity => entity.Message;
        var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;

        var testReferenceMap = new ReferenceMap<ReferenceMapTestEntity>(propertyInfo, Guid.NewGuid());

        testReferenceMap.SetIdentity(expectedGuid);

        var actualGuid = testReferenceMap.Identity;
        Assert.Equal(expectedGuid, actualGuid);
    }

    [Fact]
    public void SetJoinType_ShouldSetTheJoinTypeOfTheReferenceMap()
    {
        var expectedJoinType = JoinType.Left;

        Expression<Func<ReferenceMapTestEntity, object>> expression = entity => entity.Message;
        var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;

        var testReferenceMap = new ReferenceMap<ReferenceMapTestEntity>(propertyInfo, Guid.NewGuid());

        testReferenceMap.SetJoinType(expectedJoinType);

        var actualJoinType = testReferenceMap.JoinType;
        Assert.Equal(expectedJoinType, actualJoinType);
    }
}