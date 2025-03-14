using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DapperExtensions.xUnitTest.Helpers;

[CollectionDefinition(nameof(NonParallelTestCollection), DisableParallelization = true)]
public class NonParallelTestCollection
{
}