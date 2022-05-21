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
		private static readonly MethodInfo IHaveDefaultInterfaceMethod_Method =
			typeof(IHaveDefaultInterfaceMethod).GetMethod(nameof(IHaveDefaultInterfaceMethod.Method));

		[Test]
		public void Can_create_class_proxy()
		{
			_ = generator.CreateClassProxy<InheritsDefaultInterfaceMethod>();
		}

		[Test]
		public void Can_create_class_proxy_with_target()
		{
			var target = new InheritsDefaultInterfaceMethod();
			_ = generator.CreateClassProxyWithTarget(target);
		}

		[Test]
		public void Can_create_interface_proxy_without_target()
		{
			_ = generator.CreateInterfaceProxyWithoutTarget<IHaveDefaultInterfaceMethod>();
		}

		[Test]
		public void Can_create_interface_proxy_with_target()
		{
			var target = new InheritsDefaultInterfaceMethod();
			_ = generator.CreateInterfaceProxyWithTarget<IHaveDefaultInterfaceMethod>(target);
		}

		[Test]
		public void Can_create_interface_proxy_with_target_interface()
		{
			var target = new InheritsDefaultInterfaceMethod();
			_ = generator.CreateInterfaceProxyWithTargetInterface<IHaveDefaultInterfaceMethod>(target);
		}

		[Test]
		public void Can_intercept_method_in_class_proxy()
		{
			var interceptor = new SetReturnValueInterceptor("intercepted");
			var proxy = (IHaveDefaultInterfaceMethod)generator.CreateClassProxy<InheritsDefaultInterfaceMethod>(interceptor);

			var returnValue = proxy.Method();

			Assert.AreSame("intercepted", returnValue);
		}

		[Test]
		public void Can_intercept_method_in_class_proxy_with_target()
		{
			var interceptor = new SetReturnValueInterceptor("intercepted");
			var target = new InheritsDefaultInterfaceMethod();
			var proxy = (IHaveDefaultInterfaceMethod)generator.CreateClassProxyWithTarget<InheritsDefaultInterfaceMethod>(target, interceptor);

			var returnValue = proxy.Method();

			Assert.AreSame("intercepted", returnValue);
		}

		[Test]
		public void Can_intercept_method_in_interface_proxy_without_target()
		{
			var interceptor = new SetReturnValueInterceptor("intercepted");
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IHaveDefaultInterfaceMethod>(interceptor);

			var returnValue = proxy.Method();

			Assert.AreSame("intercepted", returnValue);
		}

		[Test]
		public void Can_intercept_method_in_interface_proxy_with_target()
		{
			var interceptor = new SetReturnValueInterceptor("intercepted");
			var target = new InheritsDefaultInterfaceMethod();
			var proxy = generator.CreateInterfaceProxyWithTarget<IHaveDefaultInterfaceMethod>(target, interceptor);

			var returnValue = proxy.Method();

			Assert.AreSame("intercepted", returnValue);
		}

		[Test]
		public void Can_intercept_method_in_interface_proxy_with_target_interface()
		{
			var interceptor = new SetReturnValueInterceptor("intercepted");
			var target = new InheritsDefaultInterfaceMethod();
			var proxy = generator.CreateInterfaceProxyWithTargetInterface<IHaveDefaultInterfaceMethod>(target, interceptor);

			var returnValue = proxy.Method();

			Assert.AreSame("intercepted", returnValue);
		}

		[Test]
		public void Default_implementation_called_when_method_not_proxied_in_class_proxy()
		{
			var options = new ProxyGenerationOptions { Hook = new ProxyNothingHook() };
			var proxy = (IHaveDefaultInterfaceMethod)generator.CreateClassProxy<InheritsDefaultInterfaceMethod>(options);

			var returnValue = proxy.Method();

			Assert.AreEqual("default implementation", returnValue);
		}

		[Test]
		public void Default_implementation_called_when_method_not_proxied_in_class_proxy_with_target()
		{
			var options = new ProxyGenerationOptions { Hook = new ProxyNothingHook() };
			var target = new InheritsDefaultInterfaceMethod();
			var proxy = (IHaveDefaultInterfaceMethod)generator.CreateClassProxyWithTarget(target, options);

			var returnValue = proxy.Method();

			Assert.AreEqual("default implementation", returnValue);
		}

		[Test]
		public void Default_implementation_called_when_method_not_proxied_in_interface_proxy_without_target()
		{
			var options = new ProxyGenerationOptions { Hook = new ProxyNothingHook() };
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IHaveDefaultInterfaceMethod>(options);

			var returnValue = proxy.Method();

			Assert.AreEqual("default implementation", returnValue);
		}

		[Test]
		public void Default_implementation_called_when_method_not_proxied_in_interface_proxy_with_target()
		{
			var options = new ProxyGenerationOptions { Hook = new ProxyNothingHook() };
			var target = new InheritsDefaultInterfaceMethod();
			var proxy = generator.CreateInterfaceProxyWithTarget<IHaveDefaultInterfaceMethod>(target, options);

			var returnValue = proxy.Method();

			Assert.AreEqual("default implementation", returnValue);
		}

		[Test]
		public void Default_implementation_called_when_method_not_proxied_in_interface_proxy_with_target_interface()
		{
			var options = new ProxyGenerationOptions { Hook = new ProxyNothingHook() };
			var target = new InheritsDefaultInterfaceMethod();
			var proxy = generator.CreateInterfaceProxyWithTargetInterface<IHaveDefaultInterfaceMethod>(target, options);

			var returnValue = proxy.Method();

			Assert.AreEqual("default implementation", returnValue);
		}

		public interface IHaveDefaultInterfaceMethod
		{
			string Method()
			{
				return "default implementation";
			}
		}

		public class InheritsDefaultInterfaceMethod : IHaveDefaultInterfaceMethod
		{
		}
	}
}

#endif
