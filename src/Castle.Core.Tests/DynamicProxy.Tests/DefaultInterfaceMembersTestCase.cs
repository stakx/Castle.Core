// Copyright 2004-2021 Castle Project - http://www.castleproject.org/
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

#if FEATURE_DEFAULT_INTERFACE_MEMBERS

namespace Castle.DynamicProxy.Tests
{
	using Castle.DynamicProxy.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class DefaultInterfaceMembersTestCase : BasePEVerifyTestCase
	{
		public static IInterceptor intercept = new SetReturnValueInterceptor("intercepted");
		public static IInterceptor proceed = new ProceedNTimesInterceptor(1);

		[Test]
		public void Proxy_of__class__with_explicit_impl__cannot_intercept() // because explicitly implemented methods are private and sealed
		{
			var c = generator.CreateClassProxy<C_explicit_IX>(intercept);
			var x = (IX)c;
			Assert.AreEqual("C_explicit_IX.Method", actual: x.Method());
		}

		[Test]
		public void Proxy_of__class__with_explicit_impl__can_proceed_to_class_impl()
		{
			var c = generator.CreateClassProxy<C_explicit_IX>(proceed);
			var x = (IX)c;
			Assert.AreEqual("C_explicit_IX.Method", actual: x.Method());
		}

		[Test]
		public void Proxy_of__class__with_explicit_impl__and_additional_interface__can_intercept()
		{
			var c = (C_explicit_IX)generator.CreateClassProxy(typeof(C_explicit_IX), new[] { typeof(IX) }, intercept);
			var x = (IX)c;
			Assert.AreEqual("intercepted", actual: x.Method());
		}

		[Test]
		public void Proxy_of__class__with_explicit_impl__and_additional_interface__can_proceed()
		{
			var c = (C_explicit_IX)generator.CreateClassProxy(typeof(C_explicit_IX), new[] { typeof(IX) }, proceed);
			var x = (IX)c;
			Assert.AreEqual("C_explicit_IX.Method", actual: x.Method());
		}

		[Test]
		public void Proxy_of__class__with_implicit_implementation__can_intercept()
		{
			var c = generator.CreateClassProxy<C_implicit_IX>(intercept);
			var x = (IX)c;
			Assert.AreEqual("intercepted", actual: c.Method());
			Assert.AreEqual("intercepted", actual: x.Method());
		}

		[Test]
		public void Proxy_of__class__with_implicit_impl__can_proceed_to_class_impl()
		{
			var c = generator.CreateClassProxy<C_implicit_IX>(proceed);
			var x = (IX)c;
			Assert.AreEqual("C_implicit_IX.Method", actual: c.Method());
			Assert.AreEqual("C_implicit_IX.Method", actual: x.Method());
		}

		[Test]
		public void Proxy_of__class__without_own_impl__can_intercept()
		{
			var c = generator.CreateClassProxy<C_default_IX>(intercept);
			var x = (IX)c;
			Assert.AreEqual("intercepted", actual: x.Method());
		}

		[Test]
		public void Proxy_of__class__without_own_impl__can_proceed_to_default_impl()
		{
			var c = generator.CreateClassProxy<C_default_IX>(proceed);
			var x = (IX)c;
			Assert.AreEqual("IX.Method", actual: x.Method());
		}

		[Test]
		public void Proxy_of__interface__with_default_impl__can_intercept()
		{
			var x = generator.CreateInterfaceProxyWithoutTarget<IX>(intercept);
			Assert.AreEqual("intercepted", actual: x.Method());
		}

		[Test]
		public void Proxy_of__interface__with_default_impl__can_proceed_to_default_impl()
		{
			var x = generator.CreateInterfaceProxyWithoutTarget<IX>(proceed);
			Assert.AreEqual("IX.Method", actual: x.Method());
		}

		public class C_default_IX : IX
		{
		}

		public class C_explicit_IX : IX
		{
			string IX.Method() => "C_explicit_IX.Method";
		}

		public class C_implicit_IX : IX
		{
			public virtual string Method() => "C_implicit_IX.Method";
		}

		public interface IX
		{
			string Method() => "IX.Method";
		}
	}
}

#endif
