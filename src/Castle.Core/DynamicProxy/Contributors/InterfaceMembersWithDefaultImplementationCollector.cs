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

namespace Castle.DynamicProxy.Contributors
{
	using System;
	using System.Diagnostics;
	using System.Reflection;

	using Castle.DynamicProxy.Generators;

	internal sealed class InterfaceMembersWithDefaultImplementationCollector : MembersCollector
	{
		private readonly InterfaceMapping map;
		private readonly bool forProxyWithTarget;

		public InterfaceMembersWithDefaultImplementationCollector(Type @interface, Type classToProxy, bool forProxyWithTarget)
			: base(@interface)
		{
			Debug.Assert(@interface != null);
			Debug.Assert(@interface.IsInterface);

			Debug.Assert(classToProxy != null);
			Debug.Assert(classToProxy.IsClass);

			Debug.Assert(@interface.IsAssignableFrom(classToProxy));

			map = classToProxy.GetInterfaceMap(@interface);
			this.forProxyWithTarget = forProxyWithTarget;
		}

		protected override MetaMethod GetMethodToGenerate(MethodInfo method, IProxyGenerationHook hook, bool standalone)
		{
			var index = Array.IndexOf(map.InterfaceMethods, method);
			Debug.Assert(index >= 0);

			var methodOnTarget = map.TargetMethods[index];
			if (methodOnTarget.DeclaringType.IsInterface == false)
			{
				return null;
			}

			var hasTarget = !methodOnTarget.IsAbstract;
			Debug.Assert(hasTarget);

			var proxyable = AcceptMethod(method, onlyVirtuals: true, hook);
			if (!proxyable && !forProxyWithTarget)
			{
				return null;
			}

			return new MetaMethod(method, methodOnTarget, standalone, proxyable, hasTarget);
		}
	}
}
