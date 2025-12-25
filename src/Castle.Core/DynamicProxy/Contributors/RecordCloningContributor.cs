// Copyright 2004-2025 Castle Project - http://www.castleproject.org/
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
	using System.Reflection;

	using Castle.DynamicProxy.Generators;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

	internal sealed class RecordCloningContributor : ITypeContributor
	{
		private readonly Type targetType;
		private MetaMethod cloneMethod;

		public RecordCloningContributor(Type targetType)
		{
			this.targetType = targetType;
		}

		public void CollectElementsToProxy(IProxyGenerationHook hook, MetaType model)
		{
			var cloneMethodInfo = targetType.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance);
			if (cloneMethodInfo == null)
			{
				return;
			}

			cloneMethod = model.FindMethod(cloneMethodInfo);
			if (cloneMethod != null)
			{
				// The target contributor may have chosen to generate interception code for this method.
				// We override that decision here. This effectively renders `<Clone>$` uninterceptable,
				// in favor of some default behavior provided by DynamicProxy. This may be a bad idea.
				cloneMethod.Ignore = true;
			}
		}

		public void Generate(ClassEmitter @class)
		{
			if (cloneMethod == null)
			{
				return;
			}

			ImplementCopyConstructor(@class, out var copyCtor);
			ImplementCloneMethod(@class, copyCtor);
		}

		private void ImplementCopyConstructor(ClassEmitter @class, out ConstructorInfo copyCtor)
		{
			var other = new ArgumentReference(@class.TypeBuilder);
			var copyCtorEmitter = @class.CreateConstructor(other);
			var baseCopyCtor = targetType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, [ targetType ], null);

			copyCtorEmitter.CodeBuilder.AddStatement(
				new ConstructorInvocationStatement(
					baseCopyCtor,
					other));

			foreach (var field in @class.GetAllFields())
			{
				if (field.Reference.IsStatic) continue;

				copyCtorEmitter.CodeBuilder.AddStatement(
					new AssignStatement(
						field,
						new FieldReference(
							field.Reference,
							other)));
			}

			copyCtorEmitter.CodeBuilder.AddStatement(
				new ReturnStatement());

			copyCtor = copyCtorEmitter.ConstructorBuilder;
		}

		private void ImplementCloneMethod(ClassEmitter @class, ConstructorInfo copyCtor)
		{
			var cloneMethod = @class.CreateMethod(
				name: this.cloneMethod.Method.Name,
				attrs: (this.cloneMethod.Method.Attributes & MethodAttributes.MemberAccessMask) | MethodAttributes.ReuseSlot | MethodAttributes.Virtual,
				returnType: targetType,
				argumentTypes: Type.EmptyTypes);

			cloneMethod.CodeBuilder.AddStatement(
				new ReturnStatement(
					new NewInstanceExpression(
						copyCtor,
						ThisExpression.Instance)));
		}
	}
}
