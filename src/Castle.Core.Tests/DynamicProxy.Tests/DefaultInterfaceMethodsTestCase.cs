// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if NETCOREAPP3_0_OR_GREATER

namespace Castle.DynamicProxy.Tests
{
	using System.Reflection;

	using Castle.DynamicProxy.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class DefaultInterfaceMethodsTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void Can_proxy__type_having_method_with_default_implementation__in_interface_proxy()
		{
			_ = generator.CreateInterfaceProxyWithoutTarget<IHaveMethodWithDefaultImplementation>();
		}

		[Test]
		public void Can_proxy__type_having_method_with_default_implementation__in_class_proxy()
		{
			_ = generator.CreateClassProxy<ImplementsIHaveMethodWithDefaultImplementationWithoutOverride>();
		}

		[Test]
		public void Can_intercept__method_with_default_implementation__in_interface_proxy()
		{
			var expectedReturnValue = MethodBase.GetCurrentMethod();

			var interceptor = new SetReturnValueInterceptor(expectedReturnValue);
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IHaveMethodWithDefaultImplementation>(interceptor);

			var actualReturnValue = proxy.MethodWithDefaultImplementation();

			Assert.AreEqual(expectedReturnValue, actualReturnValue);
		}

		[Test]
		public void Can_intercept__method_with_default_implementation__in_class_proxy()
		{
			var expectedReturnValue = MethodBase.GetCurrentMethod();

			var interceptor = new SetReturnValueInterceptor(expectedReturnValue);
			var proxy = (IHaveMethodWithDefaultImplementation)generator.CreateClassProxy<ImplementsIHaveMethodWithDefaultImplementationWithoutOverride>(interceptor);

			var actualReturnValue = proxy.MethodWithDefaultImplementation();

			Assert.AreEqual(expectedReturnValue, actualReturnValue);
		}

		[Test]
		public void Default_implementation_gets_called__when_method_not_intercepted__in_interface_proxy()
		{
			var expectedReturnValue = typeof(IHaveMethodWithDefaultImplementation)
			                          .GetMethod(nameof(IHaveMethodWithDefaultImplementation.MethodWithDefaultImplementation));

			var options = new ProxyGenerationOptions { Hook = new ProxyNothingHook() };
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IHaveMethodWithDefaultImplementation>(options);

			var actualReturnValue = proxy.MethodWithDefaultImplementation();

			Assert.AreEqual(expectedReturnValue, actualReturnValue);
		}

		[Test]
		public void Default_implementation_gets_called__when_method_not_intercepted__in_class_proxy()
		{
			var expectedReturnValue = typeof(IHaveMethodWithDefaultImplementation)
			                          .GetMethod(nameof(IHaveMethodWithDefaultImplementation.MethodWithDefaultImplementation));

			var options = new ProxyGenerationOptions { Hook = new ProxyNothingHook() };
			var proxy = (IHaveMethodWithDefaultImplementation)generator.CreateClassProxy<ImplementsIHaveMethodWithDefaultImplementationWithoutOverride>(options);

			var actualReturnValue = proxy.MethodWithDefaultImplementation();

			Assert.AreEqual(expectedReturnValue, actualReturnValue);
		}

		public interface IHaveMethodWithDefaultImplementation
		{
			MethodBase MethodWithDefaultImplementation()
			{
				return MethodBase.GetCurrentMethod();
			}
		}

		public class ImplementsIHaveMethodWithDefaultImplementationWithoutOverride : IHaveMethodWithDefaultImplementation
		{
		}
	}
}

#endif
