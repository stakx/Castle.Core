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
	using NUnit.Framework;

	[TestFixture]
	public class DefaultInterfaceMethodsTestCase : BasePEVerifyTestCase
	{
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

		public interface IHaveDefaultInterfaceMethod
		{
			void Method()
			{
			}
		}

		public class InheritsDefaultInterfaceMethod : IHaveDefaultInterfaceMethod
		{
		}
	}
}

#endif
